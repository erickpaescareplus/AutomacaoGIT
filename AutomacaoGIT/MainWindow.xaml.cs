using System.Windows;
using System.Windows.Controls;
using AutomacaoGIT.Models.DTOs;
using AutomacaoGIT.Services.Implementations;
using AutomacaoGIT.Services.Interfaces;
using Microsoft.Win32;
using System.IO;
using WpfMessageBox = System.Windows.MessageBox;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfButton = System.Windows.Controls.Button;

namespace AutomacaoGIT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IGitAutomationService _gitAutomationService;
        private readonly IVisualStudioLauncher _vsLauncher;
        private readonly IGitHubService _gitHubService;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            
            // Injeção de dependências manual (pode ser substituído por DI Container)
            _gitAutomationService = new GitAutomationService();
            _vsLauncher = new VisualStudioLauncher();
            _gitHubService = new GitHubService();

            // Inicializa ComboBox vazio - branches serão carregadas ao selecionar projeto
            CmbFeatureBranch.Items.Add("?? Selecione um projeto primeiro");
            CmbFeatureBranch.SelectedIndex = 0;
            CmbFeatureBranch.IsEnabled = false;

            AddLog("?? Sistema Mufasa iniciado. Pronto para executar automações!");
            AddLog("?? Instruções:");
            AddLog("   1. Selecione um projeto (CORE/BFF/WEB)");
            AddLog("   2. Aguarde o carregamento das branches feature");
            AddLog("   3. Configure os demais campos");
            AddLog("   4. Execute a automação");
            AddLog("");
            AddLog("?? Dica: Configure o token do GitHub (botão '?? Token') para acessar repositórios privados.");
        }

        private async void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Preparar interface
            SetUIState(isExecuting: true);
            ClearLogs();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Extrai o nome do projeto da URL do repositório
                var repositoryUrl = TxtRepositoryUrl.Text.Trim();
                var projectFolderName = repositoryUrl.Split('/').LastOrDefault()?.Replace(".git", "") ?? "projeto";

                // A branch WIT será apenas o nome digitado pelo usuário (ou null se for apenas clonar)
                var isCloneOnly = ChkCloneOnly?.IsChecked == true;
                var witName = isCloneOnly ? null : TxtWitName.Text.Trim();

                var request = new GitAutomationRequest
                {
                    RepositoryUrl = repositoryUrl,
                    LocalBasePath = TxtLocalBasePath.Text.Trim(),
                    ProjectFolderName = projectFolderName,
                    BranchName = witName,
                    FeatureBranch = CmbFeatureBranch.SelectedItem?.ToString()
                };

                AddLog("=== INICIANDO EXECUÇÃO ===");
                if (isCloneOnly)
                {
                    AddLog("?? Modo: Apenas Clonar (sem criar branch WIT)");
                }
                AddLog("");

                // Executa a automação com logs em tempo real
                var result = await _gitAutomationService.ExecuteAutomationAsync(
                    request,
                    onLogReceived: (log) => Dispatcher.Invoke(() => AddLog(log)),
                    cancellationToken: _cancellationTokenSource.Token
                );

                AddLog("");

                if (result.IsSuccess)
                {
                    AddLog("=== SUCESSO! ===");
                    
                    // Verificar qual editor abrir
                    if (RbVisualStudio != null && RbVisualStudio.IsChecked == true)
                    {
                        // Buscar e abrir solution no Visual Studio
                        var solutionPath = _vsLauncher.FindSolutionFile(request.FullProjectPath);
                        
                        if (solutionPath != null)
                        {
                            AddLog($"Solution encontrada: {solutionPath}");
                            AddLog("Abrindo Visual Studio...");
                            
                            var vsProcess = await _vsLauncher.OpenSolutionAsync(solutionPath);
                            
                            if (vsProcess != null)
                            {
                                AddLog("? Visual Studio aberto com sucesso!");
                            }
                            else
                            {
                                AddLog("?? Não foi possível abrir o Visual Studio automaticamente");
                            }
                        }
                        else
                        {
                            AddLog("?? Nenhuma solution (.sln) encontrada no projeto");
                        }
                    }
                    else if (RbVisualStudioCode != null && RbVisualStudioCode.IsChecked == true)
                    {
                        AddLog("Abrindo Visual Studio Code...");
                        var opened = await OpenVSCodeAsync(request.FullProjectPath);
                        
                        if (opened)
                        {
                            AddLog("? Visual Studio Code aberto com sucesso!");
                        }
                        else
                        {
                            AddLog("?? Não foi possível abrir o Visual Studio Code automaticamente");
                        }
                    }

                    WpfMessageBox.Show(
                        "Automação executada com sucesso!\n\n" +
                        $"Duração: {result.Duration?.TotalSeconds:F2}s\n" +
                        $"Projeto: {request.FullProjectPath}",
                        "Sucesso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    AddLog("=== FALHA NA EXECUÇÃO ===");
                    AddLog($"Status: {result.Status}");
                    
                    if (result.Errors.Any())
                    {
                        AddLog("");
                        AddLog("ERROS ENCONTRADOS:");
                        foreach (var error in result.Errors)
                        {
                            AddLog($"  • {error}");
                        }
                    }

                    WpfMessageBox.Show(
                        "A automação falhou. Verifique os logs para mais detalhes.",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AddLog($"ERRO CRÍTICO: {ex.Message}");
                WpfMessageBox.Show(
                    $"Erro inesperado:\n{ex.Message}",
                    "Erro Crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SetUIState(isExecuting: false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                AddLog("? CANCELAMENTO SOLICITADO PELO USUÁRIO");
                _cancellationTokenSource.Cancel();
                BtnCancel.IsEnabled = false;
            }
        }

        private void BtnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            ClearLogs();
        }

        private void BtnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Selecione o diretório base",
                InitialDirectory = TxtLocalBasePath.Text
            };

            if (dialog.ShowDialog() == true)
            {
                TxtLocalBasePath.Text = dialog.FolderName;
            }
        }

        private void BtnConfigureGitHub_Click(object sender, RoutedEventArgs e)
        {
            var tokenWindow = new GitHubTokenWindow
            {
                Owner = this
            };

            if (tokenWindow.ShowDialog() == true && tokenWindow.TokenWasUpdated)
            {
                // Token foi atualizado, recarregar branches
                _gitHubService.RefreshAuthentication();
                
                AddLog("?? Token do GitHub atualizado.");
                AddLog("   Recarregando branches...");
                
                // Recarregar branches do projeto atual
                if (CmbProject.SelectedItem is ComboBoxItem selectedItem)
                {
                    var repositoryUrl = selectedItem.Tag?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(repositoryUrl))
                    {
                        _ = LoadFeatureBranchesFromGitHubAsync(repositoryUrl);
                    }
                }
            }
        }

        private async void CmbProject_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Evita erro durante inicialização quando os controles ainda não foram criados
            if (TxtRepositoryUrl == null || CmbFeatureBranch == null)
                return;

            if (CmbProject.SelectedItem is ComboBoxItem selectedItem)
            {
                // Atualiza a URL do repositório    
                var repositoryUrl = selectedItem.Tag?.ToString() ?? "";
                
                // Se for a opção padrão "Selecione um projeto", limpa tudo e não busca branches
                if (string.IsNullOrEmpty(repositoryUrl))
                {
                    TxtRepositoryUrl.Text = "";
                    CmbFeatureBranch.Items.Clear();
                    CmbFeatureBranch.Items.Add("?? Selecione um projeto primeiro");
                    CmbFeatureBranch.SelectedIndex = 0;
                    CmbFeatureBranch.IsEnabled = false;
                    AddLog("?? Aguardando seleção de projeto...");
                    return;
                }
                
                
                TxtRepositoryUrl.Text = repositoryUrl;

                // Carregar branches do GitHub
                await LoadFeatureBranchesFromGitHubAsync(repositoryUrl);
            }
        }

        private async Task LoadFeatureBranchesFromGitHubAsync(string repositoryUrl)
        {
            CmbFeatureBranch.Items.Clear();
            CmbFeatureBranch.Items.Add("?? Carregando branches do GitHub...");
            CmbFeatureBranch.SelectedIndex = 0;
            CmbFeatureBranch.IsEnabled = false;

            try
            {
                if (string.IsNullOrEmpty(repositoryUrl))
                {
                    AddLog("?? URL do repositório não fornecida.");
                    CmbFeatureBranch.Items.Clear();
                    CmbFeatureBranch.Items.Add("? URL inválida");
                    CmbFeatureBranch.SelectedIndex = 0;
                    return;
                }

                AddLog($"?? Buscando branches com prefixo 'feature-' do repositório...");
                
                // Buscar branches com prefixo "feature-"
                var branches = await _gitHubService.GetBranchesAsync(repositoryUrl, "feature-");
                
                CmbFeatureBranch.Items.Clear();

                if (branches.Any())
                {
                    AddLog($"? {branches.Count} branch(es) feature encontrada(s).");
                    
                    // Preencher ComboBox
                    foreach (var branch in branches)
                    {
                        CmbFeatureBranch.Items.Add(branch);
                    }
                    
                    CmbFeatureBranch.SelectedIndex = 0;
                }
                else
                {
                    AddLog("?? Nenhuma branch com prefixo 'feature-' encontrada no repositório.");
                    AddLog("   Verifique se o repositório possui branches feature.");
                    
                    CmbFeatureBranch.Items.Add("? Nenhuma branch 'feature-*' encontrada");
                    CmbFeatureBranch.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                var isAuthError = errorMessage.Contains("404") || errorMessage.Contains("Not Found");
                var isRateLimitError = errorMessage.Contains("RATE LIMIT EXCEDIDO");
                var isAccessDeniedError = errorMessage.Contains("ACESSO NEGADO");
                var isTokenError = errorMessage.Contains("401") || errorMessage.Contains("Unauthorized");
                
                AddLog($"? ERRO ao buscar branches: {errorMessage}");
                
                if (isRateLimitError)
                {
                    AddLog("?? Limite de requisições excedido. Configure um token do GitHub (botão '?? Token').");
                }
                else if (isAccessDeniedError)
                {
                    AddLog("?? Acesso negado ao repositório.");
                    AddLog("?? Solução: Configure SSO no token (github.com/settings/tokens ? Configure SSO ? Authorize CareplusBR)");
                }
                else if (isTokenError)
                {
                    AddLog("?? Token inválido ou expirado. Atualize o token (botão '?? Token').");
                }
                else if (isAuthError)
                {
                    AddLog("?? Repositório privado. Configure um token do GitHub (botão '?? Token').");
                }
                
                CmbFeatureBranch.Items.Clear();
                var errorText = isRateLimitError ? "? Limite excedido" :
                                isAccessDeniedError ? "? Acesso negado - Configure SSO" :
                                isTokenError ? "? Token inválido" :
                                isAuthError ? "? Repositório privado" : 
                                "? Erro ao carregar";
                CmbFeatureBranch.Items.Add(errorText);
                CmbFeatureBranch.SelectedIndex = 0;
            }
            finally
            {
                CmbFeatureBranch.IsEnabled = CmbFeatureBranch.Items.Count > 0 && 
                                           !CmbFeatureBranch.Items[0].ToString()!.StartsWith("?");
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TxtRepositoryUrl.Text))
            {
                WpfMessageBox.Show("Informe a URL do repositório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtRepositoryUrl.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLocalBasePath.Text))
            {
                WpfMessageBox.Show("Informe o caminho local base.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLocalBasePath.Focus();
                return false;
            }

            // Validar WIT apenas se não for "Apenas Clonar"
            var isCloneOnly = ChkCloneOnly?.IsChecked == true;
            if (!isCloneOnly && string.IsNullOrWhiteSpace(TxtWitName.Text))
            {
                WpfMessageBox.Show("Informe o nome da WIT ou marque 'Apenas Clonar'.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtWitName.Focus();
                return false;
            }

            return true;
        }

        private void AddLog(string message)
        {
            TxtLogs.Text += message + Environment.NewLine;
            
            // Auto-scroll para o final
            LogScrollViewer.ScrollToBottom();
        }

        private void ClearLogs()
        {
            TxtLogs.Text = string.Empty;
        }

        private void SetUIState(bool isExecuting)
        {
            BtnExecute.IsEnabled = !isExecuting;
            BtnCancel.IsEnabled = isExecuting;
            
            CmbProject.IsEnabled = !isExecuting;
            TxtRepositoryUrl.IsEnabled = !isExecuting;
            TxtLocalBasePath.IsEnabled = !isExecuting;
            CmbFeatureBranch.IsEnabled = !isExecuting;
            TxtWitName.IsEnabled = !isExecuting;
            
            if (RbVisualStudio != null)
                RbVisualStudio.IsEnabled = !isExecuting;
            if (RbVisualStudioCode != null)
                RbVisualStudioCode.IsEnabled = !isExecuting;
        }

        private async Task<bool> OpenVSCodeAsync(string projectPath)
        {
            try
            {
                AddLog($"?? Abrindo VS Code no diretório: {projectPath}");
                
                // Verificar se o diretório existe
                if (!Directory.Exists(projectPath))
                {
                    AddLog($"? Erro: Diretório não existe: {projectPath}");
                    return false;
                }

                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c code \"{projectPath}\"", // /c fecha o CMD após executar
                    UseShellExecute = false, // Não usa shell para não mostrar janela
                    CreateNoWindow = true,   // Não cria janela do CMD
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden // Janela oculta
                };

                AddLog("?? Executando comando: code \"" + projectPath + "\"");
                
                var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    AddLog("? VS Code iniciado com sucesso!");
                    // Aguarda um pouco para garantir que o VS Code iniciou
                    await Task.Delay(500);
                    // Não precisa aguardar o processo terminar pois o CMD já fecha sozinho com /c
                    return true;
                }
                
                AddLog("? Processo não foi iniciado (retornou null)");
                return false;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                AddLog($"? Erro ao abrir VS Code: {ex.Message}");
                AddLog($"   Código de erro: {ex.NativeErrorCode}");
                
                if (ex.NativeErrorCode == 2) // ERROR_FILE_NOT_FOUND
                {
                    AddLog("");
                    AddLog("?? O comando 'code' não foi encontrado!");
                    AddLog("   Possíveis soluções:");
                    AddLog("   1. Verifique se o VS Code está instalado");
                    AddLog("   2. Durante a instalação, certifique-se de marcar:");
                    AddLog("      ? Adicionar ao PATH");
                    AddLog("   3. Se já instalado, adicione manualmente ao PATH:");
                    AddLog("      C:\\Users\\[seu-usuario]\\AppData\\Local\\Programs\\Microsoft VS Code\\bin");
                    AddLog("   4. Reinicie o terminal/aplicação após adicionar ao PATH");
                }
                else
                {
                    AddLog($"   Diretório tentado: {projectPath}");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                AddLog($"? Erro inesperado ao abrir VS Code: {ex.Message}");
                AddLog($"   Tipo: {ex.GetType().Name}");
                AddLog($"   Diretório: {projectPath}");
                return false;
            }
        }

        private void ChkCloneOnly_Changed(object sender, RoutedEventArgs e)
        {
            if (TxtWitName != null)
            {
                var isCloneOnly = ChkCloneOnly?.IsChecked == true;
                TxtWitName.IsEnabled = !isCloneOnly;
                TxtWitName.Background = isCloneOnly 
                    ? System.Windows.Media.Brushes.LightGray 
                    : System.Windows.Media.Brushes.White;
                
                if (isCloneOnly)
                {
                    TxtWitName.Text = "";
                }
            }
        }

        // ============================================
        // MÉTODOS DA ABA STANDALONE DE GERADOR DE PROMPT
        // ============================================

        private string? _generatedPromptCache;

        private void CmbPromptProjectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableNameSectionStandalone == null) return;
            
            // Mostrar seção de nome da tabela apenas para projetos CORE
            var selectedProject = (CmbPromptProjectType?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            var isCoreProject = selectedProject.Contains("CORE");
            
            TableNameSectionStandalone.Visibility = isCoreProject 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void BtnAddColumnStandalone_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnsContainerStandalone == null) return;

            var columnIndex = ColumnsContainerStandalone.Children.Count + 1;
            var newColumnGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            
            newColumnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            newColumnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            newColumnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            newColumnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            newColumnGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var txtColumnName = new WpfTextBox
            {
                Name = $"TxtColumnNameStandalone{columnIndex}",
                Style = (Style)FindResource("TextBoxStyle"),
                Tag = "ColumnName"
            };
            Grid.SetColumn(txtColumnName, 0);

            var cmbColumnType = new WpfComboBox
            {
                Name = $"CmbColumnTypeStandalone{columnIndex}",
                Style = (Style)FindResource("ComboBoxStyle"),
                Tag = "ColumnType",
                SelectedIndex = 0
            };
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "int" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "string" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "bool" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "decimal" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "DateTime" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "Guid" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "long" });
            cmbColumnType.Items.Add(new ComboBoxItem { Content = "double" });
            Grid.SetColumn(cmbColumnType, 2);

            var btnRemove = new WpfButton
            {
                Content = "?",
                Width = 30,
                Height = 30,
                Background = System.Windows.Media.Brushes.Red,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            btnRemove.Click += BtnRemoveColumnStandalone_Click;
            Grid.SetColumn(btnRemove, 4);

            newColumnGrid.Children.Add(txtColumnName);
            newColumnGrid.Children.Add(cmbColumnType);
            newColumnGrid.Children.Add(btnRemove);

            ColumnsContainerStandalone.Children.Add(newColumnGrid);
            UpdateRemoveButtonsVisibilityStandalone();
        }

        private void BtnRemoveColumnStandalone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is WpfButton button && button.Parent is Grid grid)
            {
                ColumnsContainerStandalone?.Children.Remove(grid);
                UpdateRemoveButtonsVisibilityStandalone();
            }
        }

        private void UpdateRemoveButtonsVisibilityStandalone()
        {
            if (ColumnsContainerStandalone == null) return;

            var shouldShowRemove = ColumnsContainerStandalone.Children.Count > 1;
            
            foreach (var child in ColumnsContainerStandalone.Children)
            {
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is WpfButton btn && btn.Content?.ToString() == "?")
                        {
                            btn.Visibility = shouldShowRemove ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private async void BtnGeneratePrompt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtGeneratedPrompt.Text = "⏳ Gerando prompt...";
                BtnSavePromptToFile.IsEnabled = false;

                // Detectar tipo automaticamente baseado no projeto selecionado
                var selectedProject = (CmbPromptProjectType?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var templateType = selectedProject.Contains("CORE") ? "CORE" : 
                                  selectedProject.Contains("BFF") ? "BFF" : "CORE";
                
                var controller = TxtPromptControllerStandalone?.Text?.Trim() ?? "";
                var tableName = TxtPromptTableNameStandalone?.Text?.Trim() ?? "";
                var endpointType = (CmbEndpointTypeStandalone?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Get";
                var endpointName = TxtPromptEndpointStandalone?.Text?.Trim() ?? "";
                var methodName = TxtPromptMethodStandalone?.Text?.Trim() ?? "";

                if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(endpointName) || string.IsNullOrEmpty(methodName))
                {
                    TxtGeneratedPrompt.Text = "❌ Erro: Preencha todos os campos obrigatórios (Controller, Endpoint e Método).";
                    return;
                }

                var columns = new List<(string name, string type)>();
                if (ColumnsContainerStandalone != null)
                {
                    foreach (var child in ColumnsContainerStandalone.Children)
                    {
                        if (child is Grid grid)
                        {
                            string? columnName = null;
                            string? columnType = null;

                            foreach (var gridChild in grid.Children)
                            {
                                if (gridChild is WpfTextBox txt && txt.Tag?.ToString() == "ColumnName")
                                    columnName = txt.Text?.Trim();
                                else if (gridChild is WpfComboBox cmb && cmb.Tag?.ToString() == "ColumnType")
                                    columnType = (cmb.SelectedItem as ComboBoxItem)?.Content?.ToString();
                            }

                            if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(columnType))
                                columns.Add((columnName, columnType));
                        }
                    }
                }

                // Usar o serviço de templates
                var templateService = new PromptTemplateService();
                var prompt = await templateService.GeneratePromptFromTemplateAsync(
                    templateType,
                    controller,
                    tableName,
                    endpointType,
                    endpointName,
                    methodName,
                    columns
                );

                if (!string.IsNullOrEmpty(prompt))
                {
                    _generatedPromptCache = prompt;
                    TxtGeneratedPrompt.Text = prompt;
                    BtnSavePromptToFile.IsEnabled = true;
                }
                else
                {
                    TxtGeneratedPrompt.Text = "❌ Erro ao gerar prompt. Verifique os dados informados.";
                }
            }
            catch (Exception ex)
            {
                TxtGeneratedPrompt.Text = $"❌ Erro ao gerar prompt:\n{ex.Message}";
            }
        }

        private void BtnSavePromptToFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_generatedPromptCache))
            {
                WpfMessageBox.Show(
                    "Nenhum prompt foi gerado ainda. Clique em 'Gerar Prompt' primeiro.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Salvar Prompt",
                    Filter = "Arquivos de Texto (*.txt)|*.txt|Arquivos Markdown (*.md)|*.md|Todos os Arquivos (*.*)|*.*",
                    FileName = "COPILOT-PROMPT.txt",
                    DefaultExt = ".txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, _generatedPromptCache);
                    
                    WpfMessageBox.Show(
                        $"Prompt salvo com sucesso!\n\nArquivo: {saveDialog.FileName}",
                        "Sucesso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show(
                    $"Erro ao salvar arquivo:\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
