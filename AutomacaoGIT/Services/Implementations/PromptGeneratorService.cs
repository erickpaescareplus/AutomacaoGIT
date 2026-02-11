using System.IO;
using System.Text;
using AutomacaoGIT.Models.DTOs;
using AutomacaoGIT.Models.Templates;
using AutomacaoGIT.Services.Interfaces;

namespace AutomacaoGIT.Services.Implementations;

public class PromptGeneratorService : IPromptGeneratorService
{
    public string GeneratePromptContent(PromptGeneratorRequest request)
    {
        var template = request.TipoTemplate.ToUpper() == "CORE" 
            ? GetCoreTemplate() 
            : GetBffTemplate();

        var sb = new StringBuilder();

        foreach (var step in template.Steps)
        {
            sb.AppendLine($"## {ReplaceVariables(step.Titulo, request)}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(step.Descricao))
            {
                sb.AppendLine(ReplaceVariables(step.Descricao, request));
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Caminho))
            {
                sb.AppendLine($"**Caminho:** `{ReplaceVariables(step.Caminho, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.CaminhoInterface))
            {
                sb.AppendLine($"**Caminho Interface:** `{ReplaceVariables(step.CaminhoInterface, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.CaminhoService))
            {
                sb.AppendLine($"**Caminho Service:** `{ReplaceVariables(step.CaminhoService, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.CaminhoDomain))
            {
                sb.AppendLine($"**Caminho Domain:** `{ReplaceVariables(step.CaminhoDomain, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Namespace))
            {
                sb.AppendLine($"**Namespace:** `{ReplaceVariables(step.Namespace, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Arquivo))
            {
                sb.AppendLine($"**Arquivo:** `{ReplaceVariables(step.Arquivo, request)}`");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Exemplo))
            {
                sb.AppendLine("**Exemplo:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.Exemplo, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.InterfaceExemplo))
            {
                sb.AppendLine("**Interface:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.InterfaceExemplo, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.ImplementacaoExemplo))
            {
                sb.AppendLine("**Implementação:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.ImplementacaoExemplo, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.ExemploInterface))
            {
                sb.AppendLine("**Exemplo Interface:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.ExemploInterface, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Interfaces))
            {
                sb.AppendLine("**Interface:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.Interfaces, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Implementacao))
            {
                sb.AppendLine("**Implementação:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.Implementacao, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.ModeloBase))
            {
                sb.AppendLine("**Modelo Base:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.ModeloBase, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.InterfaceApiService))
            {
                sb.AppendLine("**Interface API Service:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.InterfaceApiService, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.ImplementacaoApiService))
            {
                sb.AppendLine("**Implementação API Service:**");
                sb.AppendLine("```csharp");
                sb.AppendLine(ReplaceVariables(step.ImplementacaoApiService, request));
                sb.AppendLine("```");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(step.Observacao))
            {
                sb.AppendLine($"**Observação:** {ReplaceVariables(step.Observacao, request)}");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public async Task<string> CreatePromptFileAsync(
        PromptGeneratorRequest request,
        string projectPath,
        Action<string>? onLogReceived = null)
    {
        try
        {
            onLogReceived?.Invoke("?? Gerando conteúdo do prompt...");

            var promptContent = GeneratePromptContent(request);
            var fileName = $"PROMPT_{request.NomeController}_{request.NomeEndpoint}_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            var filePath = Path.Combine(projectPath, fileName);

            onLogReceived?.Invoke($"?? Criando arquivo: {fileName}");

            await File.WriteAllTextAsync(filePath, promptContent);

            onLogReceived?.Invoke($"? Arquivo criado com sucesso: {filePath}");

            return filePath;
        }
        catch (Exception ex)
        {
            onLogReceived?.Invoke($"? Erro ao criar arquivo de prompt: {ex.Message}");
            throw;
        }
    }

    private string ReplaceVariables(string text, PromptGeneratorRequest request)
    {
        return text
            .Replace("{nomeController}", request.NomeController)
            .Replace("{tipoEndpoint}", request.TipoEndpoint)
            .Replace("{nomeEndpoint}", request.NomeEndpoint)
            .Replace("{nomeMetodo}", request.NomeMetodo)
            .Replace("{colunas}", request.Colunas)
            .Replace("{tabela}", request.NomeTabela ?? "TB_NOME_TABELA")
            .Replace("\\n", Environment.NewLine);
    }

    private PromptTemplate GetCoreTemplate()
    {
        return new PromptTemplate
        {
            Steps = new List<PromptStep>
            {
                new PromptStep
                {
                    Titulo = "ETAPA 1 - CRIAÇÃO DO MAPEAMENTO DO RESPONSE/DOMAIN",
                    Caminho = "building-blocks/Application/{nomeController}/Mappings/ProviderProfile.cs",
                    Descricao = "Inserir novo mapeamento conforme abaixo:",
                    Exemplo = "CreateMap<{nomeEndpoint}Response, {nomeEndpoint}>().ReverseMap();"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 2 - CRIAÇÃO DO RESPONSE",
                    Caminho = "building-blocks/Application/{nomeController}/Response/",
                    Descricao = "Criar um response com o nome {nomeEndpoint}Response.cs com as variáveis {colunas}"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 3 - CRIAÇÃO DA INTERFACE E DO SERVICES",
                    CaminhoInterface = "building-blocks/Application/{nomeController}/Services/Interfaces/I{nomeController}Service.cs",
                    CaminhoService = "building-blocks/Application/{nomeController}/Services/{nomeController}Service.cs",
                    Descricao = "Adicionar método na interface e implementação do service",
                    InterfaceExemplo = "Task<Result<IList<{nomeEndpoint}Response>>> Get{nomeEndpoint}(CancellationToken cancellationToken = default);",
                    ImplementacaoExemplo = @"public async Task<Result<IList<{nomeEndpoint}Response>>> Get{nomeEndpoint}(CancellationToken cancellationToken = default)
{
    var result = await _providerRegisterRepository.Get{nomeEndpoint}(cancellationToken);
    return result.AdaptToMapperResult<IList<{nomeEndpoint}Response>, IList<{nomeEndpoint}>>(_mapper);
}"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 4 - CRIAÇÃO DOMAIN (DOMAIN+INTERFACE REPOSITORY)",
                    CaminhoInterface = "building-blocks/Domain/{nomeController}/Interface/I{nomeEndpoint}Repository.cs",
                    CaminhoDomain = "building-blocks/Domain/{nomeEndpoint}.cs",
                    Descricao = "Criar interface e classe domain com as mesmas propriedades do Response",
                    ExemploInterface = @"public interface I{nomeEndpoint}Repository : IBaseRepository,
    IRepositoryGet<{nomeEndpoint}, int>,
    IRepositoryActionCreateReturnEntity<{nomeEndpoint}, int>,
    IRepositoryActionCreateReturnId<{nomeEndpoint}, int>,
    IRepositoryActionUpdate<{nomeEndpoint}, int>,
    IRepositoryActionRemove<{nomeEndpoint}, int>
{
    Task<List<{nomeEndpoint}>> Get{nomeEndpoint}(CancellationToken cancellationToken = default);
    void SetTraceModelDbContext(IAuditTraceInfoModel traceInfoModel);
}"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 5 - APONTAR O SERVIÇO NO MÓDULO",
                    Caminho = "building-blocks/Infrastructure/Configurations/Modules/InfrastructureModule.cs",
                    Descricao = "Adicionar a linha do serviço conforme exemplo:",
                    Exemplo = "services.AddTransient<I{nomeController}Service, {nomeController}Service>();"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 6 - ADICIONANDO A QUERY SQL NO REPOSITORY",
                    Caminho = "building-blocks/Infrastructure/Repository/QueriesSql/{nomeController}QueriesSql.cs",
                    Descricao = "Adicionar método para buscar dados via SQL",
                    Exemplo = @"async Task<List<{nomeEndpoint}>> I{nomeController}RepositoryQueriesSql.Get{nomeEndpoint}(CancellationToken cancellationToken)
{
    string commandsql = @""select * from {tabela} "";
    var entity = await _{nomeController}Repository.GetConnection()
        .QueryAsync<{nomeEndpoint}>(commandsql);
    return entity.AsList();
}"
                },
                new PromptStep
                {
                    Titulo = "ETAPA 7 - CRIAR O CONTROLLER",
                    Descricao = "Criar um endpoint do tipo {tipoEndpoint} no #{nomeController}Controller com o nome {nomeEndpoint}",
                    Exemplo = @"/// <summary>
/// Inclui Vinculo Especialidade Prestador
/// </summary>
[Http{tipoEndpoint}(""{nomeEndpoint}"")]
[MapToApiVersion(""1.0"")]
[Authorize(Policy = Permissions.ProviderRegister.View)]
[ServiceFilter(typeof(ValidationFilterAttribute))]
[ProducesResponseType(typeof(CustomResponse200<ProviderBaseTableResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Get{nomeEndpoint}(CancellationToken cancellationToken = default)
{
    var result = await _providerRegisterService.Get{nomeEndpoint}(cancellationToken);
    return CustomResponse(result);
}"
                }
            }
        };
    }

    private PromptTemplate GetBffTemplate()
    {
        return new PromptTemplate
        {
            Steps = new List<PromptStep>
            {
                new PromptStep
                {
                    Titulo = "Etapa 1",
                    Descricao = "Criar um endpoint do tipo {tipoEndpoint} no #{nomeController}Controller com o nome {nomeEndpoint}",
                    Exemplo = @"/// <summary>
///  Consulta na core - estipulantes PolicyHolder
/// </summary>
/// <param name=""cancellationToken""></param>
/// <returns>Status 200 OK</returns>
[Http{tipoEndpoint}(""PolicyHolder"")]
[ServiceFilter(typeof(ValidationFilterAttribute))]
[Authorize(Policy = Permissions.History.View)]
[ProducesResponseType(typeof(Result<IList<ProviderRegisterApiResponse>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType((int)HttpStatusCode.NotFound)]
public async Task<IActionResult> PolicyHolder(CancellationToken cancellationToken)
{
    var response = await _providerRegisterService.{tipoEndpoint}PolicyHolder(cancellationToken);
    return CustomResponse(response);
}"
                },
                new PromptStep
                {
                    Titulo = "Etapa 2",
                    Descricao = "Criar no #I{nomeController}Services e no #{nomeController}Services o método {nomeMetodo} baseado no método abaixo",
                    Interfaces = "Task<Result<IList<AdjustmentIndexResponse>>> {tipoEndpoint}AdjustmentIndex(CancellationToken cancellationToken = default);",
                    Implementacao = @"public async Task<Result<IList<AdjustmentIndexResponse>>> {tipoEndpoint}AdjustmentIndex(CancellationToken cancellationToken = default)
{
    try
    {
        _logger.LogInformation(""Iniciando consulta de índice de reajuste."");

        var response = await _providerRegisterApiService.{tipoEndpoint}AdjustmentIndex(cancellationToken);

        if (response.Succeeded && response.Data != null)
        {
            _logger.LogInformation(""Consulta de índice de reajuste realizada com sucesso. Total encontrado: {Count}"", response.Data.Count);
        }
        else
        {
            _logger.LogWarning(""Consulta de índice de reajuste não retornou dados."");
        }

        return response;
    }
    catch (System.ArgumentException ex)
    {
        _logger.LogWarning(ex, ""Parâmetros inválidos na consulta de índice de reajuste: {Message}"", ex.Message);
        AddNotification(nameof(AdjustmentIndexResponse), ex.Message);
        return await Result<IList<AdjustmentIndexResponse>>.FailAsync(ex.Message);
    }
    catch (System.Exception ex)
    {
        _logger.LogError(ex, ""Erro inesperado ao consultar índice de reajuste"");
        AddNotification(""AdjustmentIndexError"", ""Ocorreu um erro ao processar sua solicitação"");
        return await Result<IList<AdjustmentIndexResponse>>.FailAsync(""Ocorreu um erro ao processar sua solicitação"");
    }
}"
                },
                new PromptStep
                {
                    Titulo = "Etapa 3",
                    Descricao = "Criar dentro desse namespace: CarePlus.Accreditation.Provider.Register.Infra.Bff.Api.{nomeController}.Response um arquivo cs chamado {nomeEndpoint}Response, baseado no Response abaixo",
                    Namespace = "CarePlus.Accreditation.Provider.Register.Infra.Bff.Api.{nomeController}.Response",
                    Arquivo = "{nomeEndpoint}Response.cs",
                    ModeloBase = @"public class AdjustmentIndexResponse
{
    public int CODINDEX { get; set; }
    public string NOMEINDEXREAJ { get; set; } = string.Empty;
}",
                    Observacao = "Onde a response é para a tabela com as seguintes colunas: {colunas}"
                },
                new PromptStep
                {
                    Titulo = "Etapa 4",
                    Descricao = "Adicionar um método para {nomeMetodo} dentro de #I{nomeController}ApiService baseado no método abaixo",
                    InterfaceApiService = "Task<Result<IList<BaseTableResponse>>> {tipoEndpoint}BaseTable(CancellationToken cancellationToken = default);"
                },
                new PromptStep
                {
                    Titulo = "Etapa 5",
                    Descricao = "Adicionar um método para {nomeMetodo} dentro de #{nomeController}ApiService baseado no método abaixo",
                    ImplementacaoApiService = @"public async Task<Result<IList<BaseTableResponse>>> {tipoEndpoint}BaseTable(CancellationToken cancellationToken = default)
{
    var token = await _authApi.SystemAuthenticateApiAsync(_apiClientOptions);
    var action = ""BaseTable"";

    HttpParameter parameter = new(_apiClientOptions.NameApiUrl)
    {
        NameApiUrl = _apiClientOptions!.NameApiUrl,
        Url = UrlHelper.BuildUrl(host: _apiClientOptions.Host, controller: controller, action: action),
        Headers = token
    };
    return await new HttpClientInvoker<Result<IList<BaseTableResponse>>>().{tipoEndpoint}Async(parameter);
}"
                }
            }
        };
    }
}
