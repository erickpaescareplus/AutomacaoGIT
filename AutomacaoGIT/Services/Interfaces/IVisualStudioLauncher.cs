using System.Diagnostics;

namespace AutomacaoGIT.Services.Interfaces
{
    /// <summary>
    /// Serviço para lançar Visual Studio
    /// </summary>
    public interface IVisualStudioLauncher
    {
        /// <summary>
        /// Abre uma solution no Visual Studio
        /// </summary>
        /// <param name="solutionPath">Caminho completo da .sln</param>
        /// <param name="fileToOpen">Caminho do arquivo adicional a ser aberto (opcional)</param>
        /// <returns>Processo do Visual Studio se conseguiu abrir, null caso contrário</returns>
        Task<Process?> OpenSolutionAsync(string solutionPath, string? fileToOpen = null);

        /// <summary>
        /// Busca arquivo .sln em um diretório
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <returns>Caminho da solution encontrada ou null</returns>
        string? FindSolutionFile(string directoryPath);
    }
}
