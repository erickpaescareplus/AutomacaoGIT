using AutomacaoGIT.Services.Interfaces;
using System.Diagnostics;
using System.IO;

namespace AutomacaoGIT.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço para abrir Visual Studio
    /// </summary>
    public class VisualStudioLauncher : IVisualStudioLauncher
    {
        public async Task<Process?> OpenSolutionAsync(string solutionPath)
        {
            try
            {
                if (!File.Exists(solutionPath))
                {
                    return null;
                }

                // Captura processos do Visual Studio existentes antes de abrir
                var existingProcessIds = Process.GetProcessesByName("devenv")
                    .Select(p => p.Id)
                    .ToHashSet();

                // Tenta abrir usando o comando devenv (Visual Studio)
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = solutionPath,
                    UseShellExecute = true // Usa a associação padrão do Windows
                };

                Process.Start(processStartInfo);
                
                // Aguarda um pouco para o processo iniciar
                await Task.Delay(2000);

                // Tenta encontrar o novo processo do Visual Studio
                for (int i = 0; i < 10; i++) // Tenta por até 10 segundos
                {
                    var currentProcesses = Process.GetProcessesByName("devenv");
                    
                    // Procura por um processo que não existia antes
                    var newProcess = currentProcesses.FirstOrDefault(p => 
                        !existingProcessIds.Contains(p.Id) && 
                        !p.HasExited);

                    if (newProcess != null)
                    {
                        // Registra o processo para uso posterior
                        CopilotChatService.RegisterVisualStudioProcess(newProcess);
                        return newProcess;
                    }

                    await Task.Delay(1000);
                }

                // Se não encontrou o novo processo, retorna o mais recente
                var recentProcess = Process.GetProcessesByName("devenv")
                    .Where(p => !p.HasExited)
                    .OrderByDescending(p => p.StartTime)
                    .FirstOrDefault();

                if (recentProcess != null)
                {
                    CopilotChatService.RegisterVisualStudioProcess(recentProcess);
                }

                return recentProcess;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string? FindSolutionFile(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return null;
            }

            // Procura arquivo .sln no diretório raiz
            var solutionFiles = Directory.GetFiles(directoryPath, "*.sln", SearchOption.TopDirectoryOnly);
            
            if (solutionFiles.Length > 0)
            {
                return solutionFiles[0]; // Retorna a primeira solution encontrada
            }

            // Se não encontrou na raiz, procura em subdiretórios (máximo 2 níveis)
            solutionFiles = Directory.GetFiles(directoryPath, "*.sln", SearchOption.AllDirectories);
            
            if (solutionFiles.Length > 0)
            {
                // Retorna a solution mais próxima da raiz (caminho mais curto)
                return solutionFiles.OrderBy(f => f.Length).First();
            }

            return null;
        }
    }
}
