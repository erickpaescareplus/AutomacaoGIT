using System.Windows;
using System.Windows.Controls;
using AutomacaoGIT.Models.DTOs;
using AutomacaoGIT.Services.Implementations;
using AutomacaoGIT.Services.Interfaces;
using Microsoft.Win32;

namespace AutomacaoGIT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IGitAutomationService _gitAutomationService;
        private readonly IVisualStudioLauncher _vsLauncher;
        private CancellationTokenSource? _cancellationTokenSource;

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
            }
        };

        public MainWindow()
        {
            InitializeComponent();
            
            // Injeção de dependências manual (pode ser substituído por DI Container)
            _gitAutomationService = new GitAutomationService();
            _vsLauncher = new VisualStudioLauncher();

            // Inicializa as branches features para CORE (padrão)
            LoadFeatureBranches("CORE");

            AddLog("Sistema iniciado. Pronto para executar automações.");
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
                    FeatureBranch = CmbFeatureBranch.SelectedItem?.ToString(),
                    AIPrompt = null // Desabilitado por enquanto
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
                    
                    // Buscar e abrir solution no Visual Studio
                    var solutionPath = _vsLauncher.FindSolutionFile(request.FullProjectPath);
                    
                    if (solutionPath != null)
                    {
                        AddLog($"Solution encontrada: {solutionPath}");
                        AddLog("Abrindo Visual Studio...");
                        
                        var opened = await _vsLauncher.OpenSolutionAsync(solutionPath);
                        
                        if (opened)
                        {
                            AddLog("? Visual Studio aberto com sucesso!");
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

                    MessageBox.Show(
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

                    MessageBox.Show(
                        "A automação falhou. Verifique os logs para mais detalhes.",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AddLog($"ERRO CRÍTICO: {ex.Message}");
                MessageBox.Show(
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
                var projectKey = selectedItem.Content?.ToString()?.StartsWith("CORE") == true ? "CORE" : "BFF";
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
                MessageBox.Show("Informe a URL do repositório.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtRepositoryUrl.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLocalBasePath.Text))
            {
                MessageBox.Show("Informe o caminho local base.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLocalBasePath.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtWitName.Text))
            {
                MessageBox.Show("Informe o nome da WIT.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        }
    }
}
