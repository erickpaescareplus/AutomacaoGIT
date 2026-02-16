using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using AutomacaoGIT.Services.Interfaces;

namespace AutomacaoGIT.Services.Implementations
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubConfigService _configService;
        private HttpClient? _httpClient;

        public GitHubService()
        {
            _configService = new GitHubConfigService();
            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MufasaAutomacoes", "1.0"));
            
            var token = _configService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public void RefreshAuthentication()
        {
            InitializeHttpClient();
        }

        public async Task<List<string>> GetBranchesAsync(string repositoryUrl, string? prefix = null)
        {
            try
            {
                if (_httpClient == null)
                    throw new Exception("HttpClient não inicializado");

                // Extrair owner e repo da URL
                // Exemplo: https://github.com/CareplusBR/credenciamento-prestador-cadastro-api
                var uri = new Uri(repositoryUrl);
                var pathParts = uri.AbsolutePath.Trim('/').Split('/');
                
                if (pathParts.Length < 2)
                    throw new Exception("URL do repositório inválida");

                var owner = pathParts[0];
                var repo = pathParts[1].Replace(".git", "");

                // API do GitHub para listar branches
                var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/branches?per_page=100";
                
                var response = await _httpClient.GetAsync(apiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    
                    if (statusCode == 401)
                        throw new Exception("ERRO 401: Token inválido ou expirado");
                    
                    if (statusCode == 403)
                    {
                        var remainingHeader = response.Headers.Contains("X-RateLimit-Remaining") 
                            ? response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault() 
                            : null;
                        
                        if (remainingHeader == "0")
                            throw new Exception("RATE LIMIT EXCEDIDO: Configure um token do GitHub");
                        else
                            throw new Exception("ACESSO NEGADO: Verifique se o token tem permissão SSO configurada");
                    }
                    
                    if (statusCode == 404)
                        throw new Exception($"ERRO 404: Repositório não encontrado ou sem permissão de acesso ({owner}/{repo})");
                    
                    throw new Exception($"Erro HTTP {statusCode}: {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var branches = JsonSerializer.Deserialize<List<GitHubBranch>>(json) ?? new List<GitHubBranch>();

                var branchNames = branches.Select(b => b.name).ToList();

                // Filtrar por prefixo se fornecido
                if (!string.IsNullOrEmpty(prefix))
                {
                    branchNames = branchNames
                        .Where(b => b.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return branchNames.OrderBy(b => b).ToList();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conexão ao GitHub: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private class GitHubBranch
        {
            public string name { get; set; } = string.Empty;
        }
    }
}
