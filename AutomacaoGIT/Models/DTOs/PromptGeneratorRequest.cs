namespace AutomacaoGIT.Models.DTOs;

public class PromptGeneratorRequest
{
    public required string NomeController { get; set; }
    public required string TipoEndpoint { get; set; }
    public required string NomeEndpoint { get; set; }
    public required string NomeMetodo { get; set; }
    public required string Colunas { get; set; }
    public required string TipoTemplate { get; set; } // "Core" ou "BFF"
    public string? NomeTabela { get; set; } // Apenas para CORE
}
