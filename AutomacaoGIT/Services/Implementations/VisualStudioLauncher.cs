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
        public async Task<Process?> OpenSolutionAsync(string solutionPath, string? fileToOpen = null)
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

                // Se tiver arquivo adicional para abrir, usa o comando devenv diretamente
                if (!string.IsNullOrWhiteSpace(fileToOpen) && File.Exists(fileToOpen))
                {
                    var devenvPath = FindDevenvPath();
                    if (!string.IsNullOrEmpty(devenvPath))
                    {
                        // Abre o Visual Studio com a solution e o arquivo específico
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = devenvPath,
                            Arguments = $"\"{solutionPath}\" \"{fileToOpen}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        Process.Start(processStartInfo);
                    }
                    else
                    {
                        // Fallback: abre a solution normalmente
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = solutionPath,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    // Abre apenas a solution
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = solutionPath,
                        UseShellExecute = true
                    });
                }
                
                // Aguarda um pouco para o processo iniciar
                await Task.Delay(2000);

                // Tenta encontrar o novo processo do Visual Studio
                for (int i = 0; i < 10; i++)
                {
                    var currentProcesses = Process.GetProcessesByName("devenv");
                    
                    var newProcess = currentProcesses.FirstOrDefault(p => 
                        !existingProcessIds.Contains(p.Id) && 
                        !p.HasExited);

                    if (newProcess != null)
                    {
                        return newProcess;
                    }

                    await Task.Delay(1000);
                }

                // Se não encontrou o novo processo, retorna o mais recente
                var recentProcess = Process.GetProcessesByName("devenv")
                    .Where(p => !p.HasExited)
                    .OrderByDescending(p => p.StartTime)
                    .FirstOrDefault();

                return recentProcess;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string? FindDevenvPath()
        {
            // Procura por devenv.exe nas localizações comuns do Visual Studio
            var possiblePaths = new[]
            {
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe"
            };

            return possiblePaths.FirstOrDefault(File.Exists);
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
