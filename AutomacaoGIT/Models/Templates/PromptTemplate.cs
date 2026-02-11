namespace AutomacaoGIT.Models.Templates;

public class PromptStep
{
    public required string Titulo { get; set; }
    public string? Caminho { get; set; }
    public string? Descricao { get; set; }
    public string? Exemplo { get; set; }
    public string? CaminhoInterface { get; set; }
    public string? CaminhoService { get; set; }
    public string? InterfaceExemplo { get; set; }
    public string? ImplementacaoExemplo { get; set; }
    public string? CaminhoDomain { get; set; }
    public string? ExemploInterface { get; set; }
    public string? Interfaces { get; set; }
    public string? Implementacao { get; set; }
    public string? Namespace { get; set; }
    public string? Arquivo { get; set; }
    public string? ModeloBase { get; set; }
    public string? Observacao { get; set; }
    public string? InterfaceApiService { get; set; }
    public string? ImplementacaoApiService { get; set; }
}

public class PromptTemplate
{
    public required List<PromptStep> Steps { get; set; }
}
