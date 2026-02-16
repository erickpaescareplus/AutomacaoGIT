using AutomacaoGIT.Models.Enums;

namespace AutomacaoGIT.Models.DTOs
{
    /// <summary>
    /// Resultado da execução da automação Git
    /// </summary>
    public class GitAutomationResult
    {
        public GitOperationStatus Status { get; set; }
        public List<string> Logs { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => Status == GitOperationStatus.Success;
        public string? SolutionPath { get; set; }
        public string? ProjectPath { get; set; }
        public string? PromptFilePath { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;

        public void AddLog(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Logs.Add(timestampedMessage);
        }

        public void AddError(string error)
        {
            var timestampedError = $"[{DateTime.Now:HH:mm:ss}] ERRO: {error}";
            Errors.Add(timestampedError);
            Logs.Add(timestampedError);
        }
    }
}
