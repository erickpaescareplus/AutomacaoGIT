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
        /// <returns>True se conseguiu abrir, False caso contrário</returns>
        Task<bool> OpenSolutionAsync(string solutionPath);

        /// <summary>
        /// Busca arquivo .sln em um diretório
        /// </summary>
        /// <param name="directoryPath">Caminho do diretório</param>
        /// <returns>Caminho da solution encontrada ou null</returns>
        string? FindSolutionFile(string directoryPath);
    }
}
