namespace AutomacaoGIT.Services.Interfaces
{
    public interface IGitHubService
    {
        Task<List<string>> GetBranchesAsync(string repositoryUrl, string? prefix = null);
        void RefreshAuthentication();
    }
}
