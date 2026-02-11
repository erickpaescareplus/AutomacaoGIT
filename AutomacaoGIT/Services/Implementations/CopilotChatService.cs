using AutomacaoGIT.Services.Interfaces;
using System.IO;

namespace AutomacaoGIT.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço para criar arquivos de prompt para o Copilot
    /// </summary>
    public class CopilotChatService : ICopilotChatService
    {

        public async Task<string?> CreatePromptFileAsync(string prompt, string projectPath, Action<string>? onLogReceived = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    onLogReceived?.Invoke("? Prompt está vazio");
                    return null;
                }

                if (!Directory.Exists(projectPath))
                {
                    onLogReceived?.Invoke($"? Diretório não existe: {projectPath}");
                    return null;
                }

                // Define o nome do arquivo com timestamp para evitar sobrescrita
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"COPILOT_PROMPT_{timestamp}.md";
                var filePath = Path.Combine(projectPath, fileName);

                onLogReceived?.Invoke($"?? Criando arquivo de prompt: {fileName}");

                // Cria o conteúdo do arquivo .MD - apenas o prompt, sem instruções extras
                var content = prompt;

                // Salva o arquivo
                await File.WriteAllTextAsync(filePath, content);
                onLogReceived?.Invoke($"? Arquivo criado com sucesso: {filePath}");

                return filePath;
            }
            catch (Exception ex)
            {
                onLogReceived?.Invoke($"? Erro ao criar arquivo de prompt: {ex.Message}");
                return null;
            }
        }
    }
}
