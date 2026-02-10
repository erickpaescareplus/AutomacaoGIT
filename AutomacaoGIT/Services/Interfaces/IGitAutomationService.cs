using AutomacaoGIT.Models.DTOs;

namespace AutomacaoGIT.Services.Interfaces
{
    /// <summary>
    /// Serviço de automação Git
    /// </summary>
    public interface IGitAutomationService
    {
        /// <summary>
        /// Executa o fluxo completo de automação Git
        /// </summary>
        /// <param name="request">Requisição com parâmetros da automação</param>
        /// <param name="onLogReceived">Callback para receber logs em tempo real</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado da operação</returns>
        Task<GitAutomationResult> ExecuteAutomationAsync(
            GitAutomationRequest request,
            Action<string>? onLogReceived = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida se git está instalado no sistema
        /// </summary>
        Task<bool> IsGitInstalledAsync();
    }
}
