using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace AutomacaoGIT.Services.Implementations
{
    public class PromptTemplateService
    {
        private const string TEMPLATE_FOLDER = "templatesGeradorPrompt";

        public async Task<string> GeneratePromptFromTemplateAsync(
            string templateType,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            try
            {
                // Determinar qual arquivo de template usar
                string templateFileName = templateType switch
                {
                    "CORE" => "netCore8-CQRS-API-Sandbox012026.json",
                    "BFF" => "netCore8-CQRS-BFF-Sandbox012026.json",
                    _ => throw new ArgumentException($"Template tipo '{templateType}' não suportado")
                };

                // Buscar o arquivo de template
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TEMPLATE_FOLDER, templateFileName);

                if (!File.Exists(templatePath))
                {
                    // Fallback: tentar no diretório do projeto
                    var projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
                    if (projectDir != null)
                    {
                        templatePath = Path.Combine(projectDir, TEMPLATE_FOLDER, templateFileName);
                    }
                }

                if (!File.Exists(templatePath))
                {
                    return $"? Erro: Template '{templateFileName}' não encontrado.\n\nCaminho esperado: {templatePath}";
                }

                // Ler e processar o template JSON
                string jsonContent = await File.ReadAllTextAsync(templatePath);
                var template = JsonSerializer.Deserialize<PromptTemplate>(jsonContent);

                if (template?.Steps == null || template.Steps.Count == 0)
                {
                    return "? Erro: Template JSON inválido ou vazio.";
                }

                // Gerar o prompt baseado no template
                return GeneratePromptFromJsonTemplate(template, controller, tableName, endpointType, endpointName, methodName, columns);
            }
            catch (Exception ex)
            {
                return $"? Erro ao gerar prompt:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            }
        }

        private string GeneratePromptFromJsonTemplate(
            PromptTemplate template,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            var sb = new StringBuilder();

            // Criar lista de colunas formatada
            string columnsFormatted = FormatColumnsForPrompt(columns);

            // Cabeçalho do Prompt
            sb.AppendLine("# ?? Prompt para GitHub Copilot - Implementação de Endpoint");
            sb.AppendLine();
            sb.AppendLine("## ?? Informações do Endpoint");
            sb.AppendLine();
            sb.AppendLine($"- **Controller:** {controller}");
            if (!string.IsNullOrEmpty(tableName))
            {
                sb.AppendLine($"- **Tabela:** {tableName}");
            }
            sb.AppendLine($"- **Tipo de Endpoint:** {endpointType}");
            sb.AppendLine($"- **Nome do Endpoint:** {endpointName}");
            sb.AppendLine($"- **Nome do Método:** {methodName}");
            
            if (columns.Any())
            {
                sb.AppendLine($"- **Colunas/Propriedades:**");
                foreach (var col in columns)
                {
                    sb.AppendLine($"  - `{col.type} {ConvertToPascalCase(col.name)}`");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            // Processar cada etapa do template
            sb.AppendLine("## ?? Etapas de Implementação");
            sb.AppendLine();

            for (int i = 0; i < template.Steps.Count; i++)
            {
                var step = template.Steps[i];
                sb.AppendLine($"### {step.Titulo ?? $"Etapa {i + 1}"}");
                sb.AppendLine();

                // Descrição
                if (!string.IsNullOrEmpty(step.Descricao))
                {
                    string descricao = ReplaceVariables(step.Descricao, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Descrição:** {descricao}");
                    sb.AppendLine();
                }

                // Caminho (se houver)
                if (!string.IsNullOrEmpty(step.Caminho))
                {
                    string caminho = ReplaceVariables(step.Caminho, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Caminho:** `{caminho}`");
                    sb.AppendLine();
                }

                // Namespace (se houver)
                if (!string.IsNullOrEmpty(step.Namespace))
                {
                    string ns = ReplaceVariables(step.Namespace, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Namespace:** `{ns}`");
                    sb.AppendLine();
                }

                // Arquivo (se houver)
                if (!string.IsNullOrEmpty(step.Arquivo))
                {
                    string arquivo = ReplaceVariables(step.Arquivo, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Arquivo:** `{arquivo}`");
                    sb.AppendLine();
                }

                // Exemplo de código
                if (!string.IsNullOrEmpty(step.Exemplo))
                {
                    string exemplo = ReplaceVariables(step.Exemplo, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    exemplo = exemplo.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Exemplo de implementação:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(exemplo);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Interface (BFF)
                if (!string.IsNullOrEmpty(step.Interfaces))
                {
                    string interfaceCode = ReplaceVariables(step.Interfaces, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine("**Interface:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(interfaceCode);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Implementação (BFF)
                if (!string.IsNullOrEmpty(step.Implementacao))
                {
                    string impl = ReplaceVariables(step.Implementacao, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    impl = impl.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Implementação:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(impl);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Caminho Interface (API)
                if (!string.IsNullOrEmpty(step.CaminhoInterface))
                {
                    string caminhoInterface = ReplaceVariables(step.CaminhoInterface, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Caminho Interface:** `{caminhoInterface}`");
                    sb.AppendLine();
                }

                // Caminho Service (API)
                if (!string.IsNullOrEmpty(step.CaminhoService))
                {
                    string caminhoService = ReplaceVariables(step.CaminhoService, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Caminho Service:** `{caminhoService}`");
                    sb.AppendLine();
                }

                // Interface Exemplo (API)
                if (!string.IsNullOrEmpty(step.InterfaceExemplo))
                {
                    string interfaceEx = ReplaceVariables(step.InterfaceExemplo, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine("**Interface (exemplo):**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(interfaceEx);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Implementação Exemplo (API)
                if (!string.IsNullOrEmpty(step.ImplementacaoExemplo))
                {
                    string implEx = ReplaceVariables(step.ImplementacaoExemplo, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    implEx = implEx.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Implementação (exemplo):**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(implEx);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Caminho Domain (API)
                if (!string.IsNullOrEmpty(step.CaminhoDomain))
                {
                    string caminhoDomain = ReplaceVariables(step.CaminhoDomain, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Caminho Domain:** `{caminhoDomain}`");
                    sb.AppendLine();
                }

                // Exemplo Interface (API)
                if (!string.IsNullOrEmpty(step.ExemploInterface))
                {
                    string exemploInterface = ReplaceVariables(step.ExemploInterface, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    exemploInterface = exemploInterface.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Interface (definição):**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(exemploInterface);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Interface API Service (BFF)
                if (!string.IsNullOrEmpty(step.InterfaceApiService))
                {
                    string interfaceApiService = ReplaceVariables(step.InterfaceApiService, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine("**Interface API Service:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(interfaceApiService);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Implementação API Service (BFF)
                if (!string.IsNullOrEmpty(step.ImplementacaoApiService))
                {
                    string implApiService = ReplaceVariables(step.ImplementacaoApiService, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    implApiService = implApiService.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Implementação API Service:**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(implApiService);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Modelo Base (BFF)
                if (!string.IsNullOrEmpty(step.ModeloBase))
                {
                    string modeloBase = ReplaceVariables(step.ModeloBase, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    modeloBase = modeloBase.Replace("\\n", "\n").Replace("\\\"", "\"");
                    sb.AppendLine("**Modelo Base (exemplo):**");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(modeloBase);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }

                // Observação
                if (!string.IsNullOrEmpty(step.Observacao))
                {
                    string obs = ReplaceVariables(step.Observacao, controller, tableName, endpointType, endpointName, methodName, columnsFormatted);
                    sb.AppendLine($"**Observação:** {obs}");
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            // Rodapé
            sb.AppendLine("## ? Checklist Final");
            sb.AppendLine();
            sb.AppendLine("- [ ] Todas as classes foram criadas");
            sb.AppendLine("- [ ] Dependências foram registradas no DI Container");
            sb.AppendLine("- [ ] Testes unitários foram implementados");
            sb.AppendLine("- [ ] Documentação Swagger foi atualizada");
            sb.AppendLine("- [ ] Code review foi realizado");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("*Prompt gerado automaticamente pelo Mufasa Automações* ??");

            return sb.ToString();
        }

        private string FormatColumnsForPrompt(List<(string name, string type)> columns)
        {
            if (columns == null || columns.Count == 0)
                return "Nenhuma coluna especificada";

            return string.Join(", ", columns.Select(c => $"{c.name} ({c.type})"));
        }

        private string ReplaceVariables(
            string text,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            string columnsFormatted)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("{nomeController}", controller)
                .Replace("{tabela}", tableName)
                .Replace("{tipoEndpoint}", endpointType)
                .Replace("{nomeEndpoint}", endpointName)
                .Replace("{nomeMetodo}", methodName)
                .Replace("{colunas}", columnsFormatted);
        }

        private string ConvertToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var words = text.Split('_');
            var sb = new StringBuilder();

            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    sb.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                        sb.Append(word.Substring(1).ToLower());
                }
            }

            return sb.ToString();
        }
    }

    // Classes para deserialização do JSON
    public class PromptTemplate
    {
        [JsonPropertyName("steps")]
        public List<PromptStep> Steps { get; set; } = new();
    }

    public class PromptStep
    {
        [JsonPropertyName("titulo")]
        public string? Titulo { get; set; }
        
        [JsonPropertyName("descricao")]
        public string? Descricao { get; set; }
        
        [JsonPropertyName("caminho")]
        public string? Caminho { get; set; }
        
        [JsonPropertyName("exemplo")]
        public string? Exemplo { get; set; }
        
        [JsonPropertyName("namespace")]
        public string? Namespace { get; set; }
        
        [JsonPropertyName("arquivo")]
        public string? Arquivo { get; set; }
        
        [JsonPropertyName("modeloBase")]
        public string? ModeloBase { get; set; }
        
        [JsonPropertyName("observacao")]
        public string? Observacao { get; set; }
        
        [JsonPropertyName("interfaces")]
        public string? Interfaces { get; set; }
        
        [JsonPropertyName("implementacao")]
        public string? Implementacao { get; set; }
        
        [JsonPropertyName("caminhoInterface")]
        public string? CaminhoInterface { get; set; }
        
        [JsonPropertyName("caminhoService")]
        public string? CaminhoService { get; set; }
        
        [JsonPropertyName("interfaceExemplo")]
        public string? InterfaceExemplo { get; set; }
        
        [JsonPropertyName("implementacaoExemplo")]
        public string? ImplementacaoExemplo { get; set; }
        
        [JsonPropertyName("caminhoDomain")]
        public string? CaminhoDomain { get; set; }
        
        [JsonPropertyName("exemploInterface")]
        public string? ExemploInterface { get; set; }
        
        [JsonPropertyName("interfaceApiService")]
        public string? InterfaceApiService { get; set; }
        
        [JsonPropertyName("implementacaoApiService")]
        public string? ImplementacaoApiService { get; set; }
    }
}
