using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AutomacaoGIT.Models.DTOs;
using AutomacaoGIT.Services.Implementations;
using AutomacaoGIT.Services.Interfaces;
using Microsoft.Win32;
using WpfMessageBox = System.Windows.MessageBox;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace AutomacaoGIT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IGitAutomationService _gitAutomationService;
        private readonly IVisualStudioLauncher _vsLauncher;
        private readonly IPromptGeneratorService _promptGeneratorService;
        private CancellationTokenSource? _cancellationTokenSource;
        private int _columnCounter = 1; // Contador para IDs únicos dos campos de coluna

        private readonly Dictionary<string, List<string>> _featureBranches = new()
        {
            ["CORE"] = new List<string>
            {
                "feature-B252856_PreCad_Negociacao_Mat_Med",
                "feature-B252857_PreCad_Divulgacao_Atendimento",
                "feature-B252854_PreCad_Controle_Docs",
                "feature-B253650_PreCad_Corpo_Clinico",
                "feature-B253645_PreCad_Especialidades",
                "feature-B239441_Historico_Prestadores_Beta",
                "feature-B218164_Cadastro_Prestador_Inclusao",
                "feature-B253637_PreCad_Dados_Gerais",
                "feature-B253657_PreCad_Dados_De_Pagamnto",
                "feature-B253647_PreCad_CHs",
                "feature-B252858_PreCad_Qualificacoes",
                "feature_inclusao_espelho"
            },
            ["BFF"] = new List<string>
            {
                "feature-B252854_PreCad_Controle_Docs",
                "feature-B253645_PreCad_Especialidades",
                "feature-B252857_PreCad_Divulgacao_Atendimento",
                "feature-B252856_PreCad_Negociacao_Mat_Med",
                "feature-B239441_Historico_Prestadores_Beta",
                "feature-B218164_Cadastro_Prestador_Inclusao",
                "feature-B253650_PreCad_Corpo_Clinico",
                "feature-B253647_PreCad_CHs",
                "feature-B253657_PreCad_Dados_De_Pagamnto",
                "feature-B253637_PreCad_Dados_Gerais",
                "feature-B252858_PreCad_Qualificacoes"
            },
            ["WEB"] = new List<string>
            {
                "feature-B239447_PCadOrdemRedeCredenciada_Beta",
                "feature-B253650_PreCad_Corpo_Clinico",
                "feature-B218164_Cadastro_Prestador_Inclusao",
                "feature-B253647_PreCad_CHs",
                "feature-B239441_Historico_Prestadores_Beta",
                "feature-B253637_PreCad_Dados_Gerais",
                "feature-B253645_PreCad_Especialidades",
                "feature-B252854_PreCad_Controle_Docs",
                "feature-B253657_PreCad_Dados_De_Pagamnto",
                "feature-B252858_PreCad_Qualificacoes",
                "feature-B252857_PreCad_Divulgacao_Atendimento",
                "feature-B252856_PreCad_Negociacao_Mat_Med"
            }
        };

        public MainWindow()
        {
            InitializeComponent();
            
            // Injeção de dependências manual (pode ser substituído por DI Container)
            _gitAutomationService = new GitAutomationService();
            _vsLauncher = new VisualStudioLauncher();
            _promptGeneratorService = new PromptGeneratorService();

            // Inicializa as branches features para CORE (padrão)
            LoadFeatureBranches("CORE");

            // Configura visibilidade inicial das seções de prompt
            InitializePromptSections();

            AddLog("Sistema iniciado. Pronto para executar automações.");
        }

        private void InitializePromptSections()
        {
            // Por padrão, o gerador está desativado, então esconde a seção
            PromptGeneratorSection.Visibility = Visibility.Collapsed;
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

                // A branch WIT será apenas o nome digitado pelo usuário
                var witName = TxtWitName.Text.Trim();

                var request = new GitAutomationRequest
                {
                    RepositoryUrl = repositoryUrl,
                    LocalBasePath = TxtLocalBasePath.Text.Trim(),
                    ProjectFolderName = projectFolderName,
                    BranchName = witName,
                    FeatureBranch = CmbFeatureBranch.SelectedItem?.ToString()
                };

                AddLog("=== INICIANDO EXECUÇÃO ===");
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
                            
                            string? promptFilePath = null;
                            
                            // Verificar se deve gerar prompt automaticamente
                            if (ChkGeneratePrompt.IsChecked == true)
                            {
                                AddLog("");
                                AddLog("?? Gerando prompt automaticamente...");
                                
                                promptFilePath = await GeneratePromptFileAsync(request.FullProjectPath);
                            }
                            
                            AddLog("Abrindo Visual Studio...");
                            var vsProcess = await _vsLauncher.OpenSolutionAsync(solutionPath, promptFilePath);
                            
                            if (vsProcess != null)
                            {
                                AddLog("? Visual Studio aberto com sucesso!");
                                
                                if (promptFilePath != null)
                                {
                                    AddLog("");
                                    AddLog("?? PRÓXIMOS PASSOS:");
                                    AddLog("   1. O arquivo com o prompt já está aberto no Visual Studio");
                                    AddLog("   2. Pressione Ctrl + / para abrir o GitHub Copilot Chat");
                                    AddLog("   3. Digite '@workspace' ou selecione o arquivo ativo");
                                    AddLog("   4. Peça ao Copilot para executar as instruções do documento");
                                    AddLog("");
                                }
                            }
                            else
                            {
                                AddLog("? Não foi possível abrir o Visual Studio automaticamente");
                            }
                        }
                        else
                        {
                            AddLog("? Nenhuma solution (.sln) encontrada no projeto");
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
                            AddLog("? Não foi possível abrir o Visual Studio Code automaticamente");
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

        private void CmbProject_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Evita erro durante inicialização quando os controles ainda não foram criados
            if (TxtRepositoryUrl == null || CmbFeatureBranch == null)
                return;

            if (CmbProject.SelectedItem is ComboBoxItem selectedItem)
            {
                // Atualiza a URL do repositório    
                TxtRepositoryUrl.Text = selectedItem.Tag?.ToString() ?? "";

                // Atualiza as branches features disponíveis
                var projectKey = "BFF";
                var content = selectedItem.Content?.ToString() ?? "";
                if (content.StartsWith("CORE"))
                    projectKey = "CORE";
                else if (content.StartsWith("WEB"))
                    projectKey = "WEB";
                
                LoadFeatureBranches(projectKey);
            }
        }

        private void LoadFeatureBranches(string projectKey)
        {
            CmbFeatureBranch.Items.Clear();
            
            if (_featureBranches.TryGetValue(projectKey, out var branches))
            {
                foreach (var branch in branches)
                {
                    CmbFeatureBranch.Items.Add(branch);
                }
                
                if (CmbFeatureBranch.Items.Count > 0)
                {
                    CmbFeatureBranch.SelectedIndex = 0;
                }
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

            if (string.IsNullOrWhiteSpace(TxtWitName.Text))
            {
                WpfMessageBox.Show("Informe o nome da WIT.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "code",
                    Arguments = $"\"{projectPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    await Task.Delay(1000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AddLog($"Erro ao abrir VS Code: {ex.Message}");
                AddLog("Dica: Certifique-se de que o VS Code está instalado e o comando 'code' está no PATH.");
                return false;
            }
        }

        private void ChkGeneratePrompt_Changed(object sender, RoutedEventArgs e)
        {
            // Mostra ou esconde a seção do gerador baseado no checkbox
            PromptGeneratorSection.Visibility = ChkGeneratePrompt.IsChecked == true 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void CmbTemplateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Mostra campo de tabela apenas para CORE
            if (TableNameSection != null && CmbTemplateType.SelectedItem is ComboBoxItem selected)
            {
                var content = selected.Content?.ToString() ?? "";
                TableNameSection.Visibility = content.StartsWith("CORE") 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
        }

        private void BtnAddColumn_Click(object sender, RoutedEventArgs e)
        {
            _columnCounter++;
            
            // Criar novo Grid para nova linha
            var newGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            
            // Definir colunas
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            // TextBox para nome da coluna
            var txtName = new WpfTextBox
            {
                Name = $"TxtColumnName{_columnCounter}",
                Style = (Style)FindResource("TextBoxStyle"),
                Tag = "ColumnName"
            };
            Grid.SetColumn(txtName, 0);
            newGrid.Children.Add(txtName);
            
            // ComboBox para tipo
            var cmbType = new WpfComboBox
            {
                Name = $"CmbColumnType{_columnCounter}",
                Style = (Style)FindResource("ComboBoxStyle"),
                Tag = "ColumnType",
                SelectedIndex = 0
            };
            
            // Adicionar tipos
            cmbType.Items.Add(new ComboBoxItem { Content = "int" });
            cmbType.Items.Add(new ComboBoxItem { Content = "string" });
            cmbType.Items.Add(new ComboBoxItem { Content = "bool" });
            cmbType.Items.Add(new ComboBoxItem { Content = "decimal" });
            cmbType.Items.Add(new ComboBoxItem { Content = "DateTime" });
            cmbType.Items.Add(new ComboBoxItem { Content = "Guid" });
            cmbType.Items.Add(new ComboBoxItem { Content = "long" });
            cmbType.Items.Add(new ComboBoxItem { Content = "double" });
            
            Grid.SetColumn(cmbType, 2);
            newGrid.Children.Add(cmbType);
            
            // Botão remover
            var btnRemove = new WpfButton
            {
                Content = "?",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            
            btnRemove.Click += BtnRemoveColumn_Click;
            btnRemove.Template = CreateButtonTemplate();
            
            Grid.SetColumn(btnRemove, 4);
            newGrid.Children.Add(btnRemove);
            
            // Adicionar ao container
            ColumnsContainer.Children.Add(newGrid);
            
            // Mostrar botão remover do primeiro campo se tiver mais de 1
            UpdateRemoveButtonsVisibility();
        }

        private void BtnRemoveColumn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is WpfButton button && button.Parent is Grid grid)
            {
                ColumnsContainer.Children.Remove(grid);
                UpdateRemoveButtonsVisibility();
            }
        }

        private void UpdateRemoveButtonsVisibility()
        {
            // Mostra botões de remover apenas se houver mais de 1 campo
            bool showRemoveButtons = ColumnsContainer.Children.Count > 1;
            
            foreach (var child in ColumnsContainer.Children)
            {
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is WpfButton button && button.Content.ToString() == "?")
                        {
                            button.Visibility = showRemoveButtons ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private ControlTemplate CreateButtonTemplate()
        {
            var template = new ControlTemplate(typeof(WpfButton));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            });
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            
            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            contentFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            
            factory.AppendChild(contentFactory);
            template.VisualTree = factory;
            
            return template;
        }

        private string GetColumnsFromUI()
        {
            var columns = new List<string>();
            
            foreach (var child in ColumnsContainer.Children)
            {
                if (child is Grid grid)
                {
                    string? columnName = null;
                    string? columnType = null;
                    
                    foreach (var gridChild in grid.Children)
                    {
                        if (gridChild is WpfTextBox txt && txt.Tag?.ToString() == "ColumnName")
                        {
                            columnName = txt.Text?.Trim();
                        }
                        else if (gridChild is WpfComboBox cmb && cmb.Tag?.ToString() == "ColumnType")
                        {
                            if (cmb.SelectedItem is ComboBoxItem item)
                            {
                                columnType = item.Content?.ToString();
                            }
                        }
                    }
                    
                    if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(columnType))
                    {
                        columns.Add($"{columnName}:{columnType}");
                    }
                }
            }
            
            return string.Join("; ", columns);
        }

        private async Task<string?> GeneratePromptFileAsync(string projectPath)
        {
            try
            {
                // Validar campos do gerador de prompt
                if (string.IsNullOrWhiteSpace(TxtPromptController.Text))
                {
                    AddLog("? Nome do Controller é obrigatório");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(TxtPromptEndpoint.Text))
                {
                    AddLog("? Nome do Endpoint é obrigatório");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(TxtPromptMethod.Text))
                {
                    AddLog("? Nome do Método é obrigatório");
                    return null;
                }

                // Obter colunas dos campos dinâmicos
                var colunas = GetColumnsFromUI();
                
                if (string.IsNullOrWhiteSpace(colunas))
                {
                    AddLog("? Adicione pelo menos uma coluna");
                    return null;
                }

                // Determinar o tipo de template
                var templateType = "CORE";
                if (CmbTemplateType.SelectedItem is ComboBoxItem selectedTemplate)
                {
                    var content = selectedTemplate.Content?.ToString() ?? "";
                    if (content.StartsWith("BFF"))
                        templateType = "BFF";
                }

                // Validar nome da tabela para CORE
                if (templateType == "CORE" && string.IsNullOrWhiteSpace(TxtPromptTableName.Text))
                {
                    AddLog("? Nome da Tabela é obrigatório para template CORE");
                    return null;
                }

                // Obter tipo de endpoint
                var endpointType = CmbEndpointType.SelectedItem is ComboBoxItem selectedEndpoint
                    ? selectedEndpoint.Content?.ToString() ?? "Get"
                    : "Get";

                var request = new PromptGeneratorRequest
                {
                    NomeController = TxtPromptController.Text.Trim(),
                    TipoEndpoint = endpointType,
                    NomeEndpoint = TxtPromptEndpoint.Text.Trim(),
                    NomeMetodo = TxtPromptMethod.Text.Trim(),
                    Colunas = colunas,
                    TipoTemplate = templateType,
                    NomeTabela = templateType == "CORE" ? TxtPromptTableName.Text.Trim() : null
                };

                var filePath = await _promptGeneratorService.CreatePromptFileAsync(
                    request,
                    projectPath,
                    onLogReceived: (log) => Dispatcher.Invoke(() => AddLog(log))
                );

                return filePath;
            }
            catch (Exception ex)
            {
                AddLog($"? Erro ao gerar prompt: {ex.Message}");
                return null;
            }
        }
    }
}
