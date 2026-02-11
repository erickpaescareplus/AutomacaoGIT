using System.IO;

namespace AutomacaoGIT.Models.DTOs
{
    /// <summary>
    /// Requisição para automação Git
    /// </summary>
    public class GitAutomationRequest
    {
        /// <summary>
        /// URL do repositório Git (ex: https://dev.azure.com/org/project/_git/repo)
        /// </summary>
        public required string RepositoryUrl { get; set; }

        /// <summary>
        /// Caminho local base onde o repositório será clonado (ex: C:\Projetos)
        /// </summary>
        public required string LocalBasePath { get; set; }

        /// <summary>
        /// Nome da pasta do projeto (ex: MeuProjeto)
        /// </summary>
        public required string ProjectFolderName { get; set; }

        /// <summary>
        /// Nome da branch (opcional). Se informado, fará checkout/criação da branch
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// Nome da branch feature base (opcional). Se informado, será usada como base para criar a nova branch
        /// </summary>
        public string? FeatureBranch { get; set; }

        /// <summary>
        /// Credenciais de acesso (opcional, pode usar credenciais do sistema)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Token de acesso pessoal (PAT) ou senha
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Caminho completo do projeto (LocalBasePath + ProjectFolderName)
        /// </summary>
        public string FullProjectPath => Path.Combine(LocalBasePath, ProjectFolderName);
    }
}
