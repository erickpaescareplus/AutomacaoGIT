using AutomacaoGIT.Models.DTOs;

namespace AutomacaoGIT.Services.Interfaces;

public interface IPromptGeneratorService
{
    /// <summary>
    /// Gera o conteúdo do prompt baseado no template e requisição
    /// </summary>
    string GeneratePromptContent(PromptGeneratorRequest request);
    
    /// <summary>
    /// Cria o arquivo .md com o prompt gerado
    /// </summary>
    Task<string> CreatePromptFileAsync(
        PromptGeneratorRequest request,
        string projectPath,
        Action<string>? onLogReceived = null);
}
