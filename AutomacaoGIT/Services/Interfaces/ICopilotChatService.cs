namespace AutomacaoGIT.Services.Interfaces
{
    /// <summary>
    /// Serviço para criar arquivos de prompt para o Copilot
    /// </summary>
    public interface ICopilotChatService
    {
        /// <summary>
        /// Cria um arquivo .MD com o prompt do usuário
        /// </summary>
        /// <param name="prompt">Texto do prompt</param>
        /// <param name="projectPath">Caminho do projeto onde o arquivo será salvo</param>
        /// <param name="onLogReceived">Callback para logs do processo</param>
        /// <returns>Caminho completo do arquivo criado ou null em caso de erro</returns>
        Task<string?> CreatePromptFileAsync(string prompt, string projectPath, Action<string>? onLogReceived = null);
    }
}
