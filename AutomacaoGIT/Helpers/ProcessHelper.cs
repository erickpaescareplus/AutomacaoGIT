using System.Diagnostics;
using System.Text;

namespace AutomacaoGIT.Helpers
{
    /// <summary>
    /// Helper para executar processos de linha de comando
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Executa um comando e retorna a saída
        /// </summary>
        public static async Task<(int exitCode, string output, string error)> ExecuteCommandAsync(
            string fileName,
            string arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        }

        /// <summary>
        /// Executa comando Git
        /// </summary>
        public static Task<(int exitCode, string output, string error)> ExecuteGitCommandAsync(
            string arguments,
            string? workingDirectory = null,
            CancellationToken cancellationToken = default)
        {
            return ExecuteCommandAsync("git", arguments, workingDirectory, cancellationToken);
        }
    }
}
