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
        public async Task<bool> OpenSolutionAsync(string solutionPath)
        {
            try
            {
                if (!File.Exists(solutionPath))
                {
                    return false;
                }

                // Tenta abrir usando o comando devenv (Visual Studio)
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = solutionPath,
                    UseShellExecute = true // Usa a associação padrão do Windows
                };

                Process.Start(processStartInfo);
                
                // Pequeno delay para garantir que o processo iniciou
                await Task.Delay(500);
                
                return true;
            }
            catch (Exception)
            {
                return false;
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
