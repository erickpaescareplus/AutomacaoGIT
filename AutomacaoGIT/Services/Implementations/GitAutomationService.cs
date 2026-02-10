using AutomacaoGIT.Helpers;
using AutomacaoGIT.Models.DTOs;
using AutomacaoGIT.Models.Enums;
using AutomacaoGIT.Services.Interfaces;
using System.IO;

namespace AutomacaoGIT.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de automação Git
    /// </summary>
    public class GitAutomationService : IGitAutomationService
    {
        public async Task<GitAutomationResult> ExecuteAutomationAsync(
            GitAutomationRequest request,
            Action<string>? onLogReceived = null,
            CancellationToken cancellationToken = default)
        {
            var result = new GitAutomationResult
            {
                Status = GitOperationStatus.InProgress,
                StartTime = DateTime.Now,
                ProjectPath = request.FullProjectPath
            };

            void Log(string message)
            {
                result.AddLog(message);
                onLogReceived?.Invoke(message);
            }

            void LogError(string error)
            {
                result.AddError(error);
                onLogReceived?.Invoke($"ERRO: {error}");
            }

            try
            {
                Log("=== INICIANDO AUTOMAÇÃO GIT ===");
                Log($"Repositório: {request.RepositoryUrl}");
                Log($"Caminho local: {request.FullProjectPath}");

                // Valida se Git está instalado
                if (!await IsGitInstalledAsync())
                {
                    LogError("Git não está instalado ou não está no PATH do sistema");
                    result.Status = GitOperationStatus.Failed;
                    result.EndTime = DateTime.Now;
                    return result;
                }

                Log("? Git encontrado no sistema");

                // Valida se a pasta base existe
                if (!Directory.Exists(request.LocalBasePath))
                {
                    Log($"Criando diretório base: {request.LocalBasePath}");
                    Directory.CreateDirectory(request.LocalBasePath);
                }

                // Verifica se o repositório já existe
                bool repositoryExists = Directory.Exists(request.FullProjectPath);

                if (repositoryExists)
                {
                    Log($"Repositório já existe em: {request.FullProjectPath}");
                    await UpdateRepositoryAsync(request, Log, LogError, cancellationToken);
                }
                else
                {
                    Log($"Clonando repositório...");
                    await CloneRepositoryAsync(request, Log, LogError, cancellationToken);
                }

                // Gerenciar branch se especificada
                if (!string.IsNullOrWhiteSpace(request.BranchName))
                {
                    await ManageBranchAsync(request, Log, LogError, cancellationToken);
                }

                // Armazena o prompt de IA se fornecido
                if (!string.IsNullOrWhiteSpace(request.AIPrompt))
                {
                    await SaveAIPromptAsync(request, Log, cancellationToken);
                }

                result.Status = GitOperationStatus.Success;
                result.EndTime = DateTime.Now;
                Log($"=== AUTOMAÇÃO CONCLUÍDA COM SUCESSO EM {result.Duration?.TotalSeconds:F2}s ===");

                return result;
            }
            catch (OperationCanceledException)
            {
                LogError("Operação cancelada pelo usuário");
                result.Status = GitOperationStatus.Cancelled;
                result.EndTime = DateTime.Now;
                return result;
            }
            catch (Exception ex)
            {
                LogError($"Erro não tratado: {ex.Message}");
                result.Status = GitOperationStatus.Failed;
                result.EndTime = DateTime.Now;
                return result;
            }
        }

        public async Task<bool> IsGitInstalledAsync()
        {
            try
            {
                var (exitCode, _, _) = await ProcessHelper.ExecuteGitCommandAsync("--version");
                return exitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task CloneRepositoryAsync(
            GitAutomationRequest request,
            Action<string> log,
            Action<string> logError,
            CancellationToken cancellationToken)
        {
            log("Executando git clone...");

            var arguments = $"clone \"{request.RepositoryUrl}\" \"{request.ProjectFolderName}\"";
            var (exitCode, output, error) = await ProcessHelper.ExecuteGitCommandAsync(
                arguments,
                request.LocalBasePath,
                cancellationToken);

            if (exitCode != 0)
            {
                logError($"Falha ao clonar repositório: {error}");
                throw new InvalidOperationException($"Erro ao clonar: {error}");
            }

            log("? Repositório clonado com sucesso");
            if (!string.IsNullOrWhiteSpace(output))
                log($"  {output.Trim()}");
        }

        private async Task UpdateRepositoryAsync(
            GitAutomationRequest request,
            Action<string> log,
            Action<string> logError,
            CancellationToken cancellationToken)
        {
            log("Atualizando repositório existente...");

            // Verifica status
            var (statusCode, statusOutput, _) = await ProcessHelper.ExecuteGitCommandAsync(
                "status --porcelain",
                request.FullProjectPath,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(statusOutput))
            {
                log("? Alterações locais detectadas:");
                log(statusOutput);
            }

            // Fetch
            log("Executando git fetch...");
            var (fetchCode, fetchOutput, fetchError) = await ProcessHelper.ExecuteGitCommandAsync(
                "fetch --all",
                request.FullProjectPath,
                cancellationToken);

            if (fetchCode != 0)
            {
                logError($"Aviso ao executar fetch: {fetchError}");
            }
            else
            {
                log("? Fetch executado");
            }

            // Pull
            log("Executando git pull...");
            var (pullCode, pullOutput, pullError) = await ProcessHelper.ExecuteGitCommandAsync(
                "pull",
                request.FullProjectPath,
                cancellationToken);

            if (pullCode != 0)
            {
                logError($"Aviso ao executar pull: {pullError}");
                log("? Continue manualmente se necessário");
            }
            else
            {
                log("? Pull executado com sucesso");
                if (!string.IsNullOrWhiteSpace(pullOutput))
                    log($"  {pullOutput.Trim()}");
            }
        }

        private async Task ManageBranchAsync(
            GitAutomationRequest request,
            Action<string> log,
            Action<string> logError,
            CancellationToken cancellationToken)
        {
            log($"Gerenciando branch: {request.BranchName}");

            // Verifica se a branch existe localmente
            var (listCode, listOutput, _) = await ProcessHelper.ExecuteGitCommandAsync(
                "branch --list",
                request.FullProjectPath,
                cancellationToken);

            bool branchExistsLocally = listOutput.Contains(request.BranchName!);

            if (branchExistsLocally)
            {
                log($"Fazendo checkout para branch existente: {request.BranchName}");
                var (checkoutCode, checkoutOutput, checkoutError) = await ProcessHelper.ExecuteGitCommandAsync(
                    $"checkout {request.BranchName}",
                    request.FullProjectPath,
                    cancellationToken);

                if (checkoutCode != 0)
                {
                    logError($"Erro ao fazer checkout: {checkoutError}");
                }
                else
                {
                    log($"? Checkout realizado para: {request.BranchName}");
                }
            }
            else
            {
                // Se foi especificada uma feature branch, usa ela como base
                if (!string.IsNullOrWhiteSpace(request.FeatureBranch))
                {
                    log($"Branch feature base: {request.FeatureBranch}");
                    
                    // Verifica se a branch feature existe remotamente
                    var (remoteBranchCode, remoteBranchOutput, _) = await ProcessHelper.ExecuteGitCommandAsync(
                        $"ls-remote --heads origin {request.FeatureBranch}",
                        request.FullProjectPath,
                        cancellationToken);
                    
                    if (remoteBranchCode == 0 && !string.IsNullOrWhiteSpace(remoteBranchOutput))
                    {
                        log($"Fazendo checkout da branch feature base: {request.FeatureBranch}");
                        var (checkoutFeatureCode, _, checkoutFeatureError) = await ProcessHelper.ExecuteGitCommandAsync(
                            $"checkout {request.FeatureBranch}",
                            request.FullProjectPath,
                            cancellationToken);
                        
                        if (checkoutFeatureCode != 0)
                        {
                            logError($"Aviso ao fazer checkout da feature base: {checkoutFeatureError}");
                        }
                        else
                        {
                            log($"? Checkout realizado para feature base: {request.FeatureBranch}");
                        }
                    }
                    else
                    {
                        log($"? Feature branch não encontrada no remoto, criando a partir da branch atual");
                    }
                }
                
                log($"Criando nova branch: {request.BranchName}");
                var (createCode, createOutput, createError) = await ProcessHelper.ExecuteGitCommandAsync(
                    $"checkout -b {request.BranchName}",
                    request.FullProjectPath,
                    cancellationToken);

                if (createCode != 0)
                {
                    logError($"Erro ao criar branch: {createError}");
                }
                else
                {
                    log($"? Branch criada e checkout realizado: {request.BranchName}");
                }
            }
        }

        private async Task SaveAIPromptAsync(
            GitAutomationRequest request,
            Action<string> log,
            CancellationToken cancellationToken)
        {
            try
            {
                var promptFilePath = Path.Combine(request.FullProjectPath, ".ai-prompt.txt");
                await File.WriteAllTextAsync(promptFilePath, request.AIPrompt!, cancellationToken);
                log($"? Prompt de IA salvo em: {promptFilePath}");
            }
            catch (Exception ex)
            {
                log($"? Não foi possível salvar o prompt de IA: {ex.Message}");
            }
        }
    }
}
