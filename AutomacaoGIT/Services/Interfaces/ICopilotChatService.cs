namespace AutomacaoGIT.Services.Interfaces
{
    /// <summary>
    /// Serviço para interação com o Copilot Chat do Visual Studio
    /// </summary>
    public interface ICopilotChatService
    {
        /// <summary>
        /// Envia um prompt para o Copilot Chat do Visual Studio
        /// </summary>
        /// <param name="prompt">Texto do prompt a ser enviado</param>
        /// <param name="waitForVsReady">Se deve aguardar o VS estar totalmente carregado</param>
        /// <param name="maxWaitSeconds">Tempo máximo de espera em segundos</param>
        /// <param name="onLogReceived">Callback para logs de debug</param>
        /// <returns>True se o prompt foi enviado com sucesso</returns>
        Task<bool> SendPromptToCopilotAsync(string prompt, bool waitForVsReady = true, int maxWaitSeconds = 120, Action<string>? onLogReceived = null);

        /// <summary>
        /// Verifica se o Visual Studio está totalmente carregado e pronto
        /// </summary>
        /// <param name="processId">ID do processo do Visual Studio</param>
        /// <returns>True se o Visual Studio está pronto</returns>
        Task<bool> IsVisualStudioReadyAsync(int processId);
    }
}
