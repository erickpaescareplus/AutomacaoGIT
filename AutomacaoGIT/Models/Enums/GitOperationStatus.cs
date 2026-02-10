namespace AutomacaoGIT.Models.Enums
{
    /// <summary>
    /// Status das operações Git executadas
    /// </summary>
    public enum GitOperationStatus
    {
        NotStarted,
        InProgress,
        Success,
        Failed,
        PartialSuccess,
        Cancelled
    }
}
