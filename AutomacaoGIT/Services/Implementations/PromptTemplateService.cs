using System.Text;

namespace AutomacaoGIT.Services.Implementations
{
    public class PromptTemplateService
    {
        public async Task<string> GeneratePromptFromTemplateAsync(
            string templateType,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            return await Task.Run(() => GeneratePromptFromTemplate(
                templateType, controller, tableName, endpointType, endpointName, methodName, columns));
        }

        private string GeneratePromptFromTemplate(
            string templateType,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# ?? Prompt para GitHub Copilot");
            sb.AppendLine();
            sb.AppendLine($"**Tipo de Projeto:** {templateType}");
            sb.AppendLine($"**Controller:** {controller}");
            
            if (templateType == "CORE" && !string.IsNullOrEmpty(tableName))
            {
                sb.AppendLine($"**Tabela:** {tableName}");
            }
            
            sb.AppendLine($"**Endpoint:** {endpointType} /{endpointName}");
            sb.AppendLine($"**Método:** {methodName}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            if (templateType == "CORE")
            {
                GenerateCorePrompt(sb, controller, tableName, endpointType, endpointName, methodName, columns);
            }
            else if (templateType == "BFF")
            {
                GenerateBffPrompt(sb, controller, endpointType, endpointName, methodName, columns);
            }
            else if (templateType == "WEB")
            {
                GenerateWebPrompt(sb, controller, endpointType, endpointName, methodName, columns);
            }

            return sb.ToString();
        }

        private void GenerateCorePrompt(
            StringBuilder sb,
            string controller,
            string tableName,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            sb.AppendLine("## ?? Instruções para Implementação (CORE API)");
            sb.AppendLine();
            sb.AppendLine("### 1. Criar Entity");
            sb.AppendLine($"Criar uma nova Entity chamada `{endpointName}` na pasta `Domain/Entities` com as seguintes propriedades:");
            sb.AppendLine();
            
            foreach (var column in columns)
            {
                sb.AppendLine($"- `{column.type} {ConvertToPascalCase(column.name)}`");
            }
            
            sb.AppendLine();
            sb.AppendLine("### 2. Criar Mapping (Dapper)");
            sb.AppendLine($"Criar o mapping Dapper para a entidade `{endpointName}` na pasta `Infrastructure/Data/Mappings`, mapeando para a tabela `{tableName}`:");
            sb.AppendLine();
            
            foreach (var column in columns)
            {
                sb.AppendLine($"- `{ConvertToPascalCase(column.name)}` -> `{column.name}`");
            }
            
            sb.AppendLine();
            sb.AppendLine("### 3. Criar Repository Interface");
            sb.AppendLine($"Criar interface `I{endpointName}Repository` em `Domain/Interfaces/Repositories` com o método:");
            sb.AppendLine($"```csharp");
            sb.AppendLine($"Task<IEnumerable<{endpointName}>> {methodName}Async();");
            sb.AppendLine("```");
            sb.AppendLine();
            
            sb.AppendLine("### 4. Implementar Repository");
            sb.AppendLine($"Implementar `{endpointName}Repository` em `Infrastructure/Repositories` usando Dapper.");
            sb.AppendLine();
            
            sb.AppendLine("### 5. Criar Service Interface");
            sb.AppendLine($"Criar interface `I{endpointName}Service` em `Application/Interfaces` com o método:");
            sb.AppendLine($"```csharp");
            sb.AppendLine($"Task<IEnumerable<{endpointName}>> {methodName}Async();");
            sb.AppendLine("```");
            sb.AppendLine();
            
            sb.AppendLine("### 6. Implementar Service");
            sb.AppendLine($"Implementar `{endpointName}Service` em `Application/Services` usando o repository.");
            sb.AppendLine();
            
            sb.AppendLine("### 7. Adicionar Endpoint no Controller");
            sb.AppendLine($"Adicionar endpoint `{endpointType.ToUpper()} /{endpointName}` no controller `{controller}Controller`:");
            sb.AppendLine($"```csharp");
            sb.AppendLine($"[Http{endpointType}(\"{endpointName}\")]");
            sb.AppendLine($"public async Task<IActionResult> {methodName}()");
            sb.AppendLine($"{{");
            sb.AppendLine($"    var result = await _{ToCamelCase(endpointName)}Service.{methodName}Async();");
            sb.AppendLine($"    return Ok(result);");
            sb.AppendLine($"}}");
            sb.AppendLine("```");
            sb.AppendLine();
            
            sb.AppendLine("### 8. Registrar no DI Container");
            sb.AppendLine("Registrar as dependências no `Startup.cs` ou `Program.cs`.");
            sb.AppendLine();
            
            sb.AppendLine("### 9. Criar Testes Unitários");
            sb.AppendLine($"Criar testes para `{endpointName}Service` e `{controller}Controller`.");
        }

        private void GenerateBffPrompt(
            StringBuilder sb,
            string controller,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            sb.AppendLine("## ?? Instruções para Implementação (BFF API)");
            sb.AppendLine();
            sb.AppendLine("### 1. Criar DTO de Request");
            sb.AppendLine($"Criar `{endpointName}Request` em `Models/Requests` com as propriedades:");
            sb.AppendLine();
            
            foreach (var column in columns)
            {
                sb.AppendLine($"- `{column.type} {ConvertToPascalCase(column.name)}`");
            }
            
            sb.AppendLine();
            sb.AppendLine("### 2. Criar DTO de Response");
            sb.AppendLine($"Criar `{endpointName}Response` em `Models/Responses` com as propriedades necessárias.");
            sb.AppendLine();
            
            sb.AppendLine("### 3. Criar HTTP Client Service");
            sb.AppendLine($"Criar interface e implementação `I{endpointName}HttpService` para chamar a API CORE.");
            sb.AppendLine();
            
            sb.AppendLine("### 4. Criar BFF Service");
            sb.AppendLine($"Criar `{endpointName}Service` que orquestra chamadas e transforma dados.");
            sb.AppendLine();
            
            sb.AppendLine("### 5. Adicionar Endpoint no Controller");
            sb.AppendLine($"Adicionar endpoint `{endpointType.ToUpper()} /{endpointName}` no controller `{controller}Controller`.");
            sb.AppendLine();
            
            sb.AppendLine("### 6. Configurar HttpClient");
            sb.AppendLine("Registrar o HttpClient no DI Container com a URL base da API CORE.");
        }

        private void GenerateWebPrompt(
            StringBuilder sb,
            string controller,
            string endpointType,
            string endpointName,
            string methodName,
            List<(string name, string type)> columns)
        {
            sb.AppendLine("## ?? Instruções para Implementação (WEB)");
            sb.AppendLine();
            sb.AppendLine("### 1. Criar Model/ViewModel");
            sb.AppendLine($"Criar `{endpointName}ViewModel` com as propriedades:");
            sb.AppendLine();
            
            foreach (var column in columns)
            {
                sb.AppendLine($"- `{column.type} {ConvertToPascalCase(column.name)}`");
            }
            
            sb.AppendLine();
            sb.AppendLine("### 2. Criar Service para HTTP Client");
            sb.AppendLine($"Criar `{endpointName}Service` para chamar a BFF API.");
            sb.AppendLine();
            
            sb.AppendLine("### 3. Criar Controller");
            sb.AppendLine($"Criar action `{methodName}` no controller `{controller}Controller`.");
            sb.AppendLine();
            
            sb.AppendLine("### 4. Criar View");
            sb.AppendLine($"Criar view `{endpointName}.cshtml` com formulário/listagem.");
            sb.AppendLine();
            
            sb.AppendLine("### 5. Adicionar Validação");
            sb.AppendLine("Adicionar data annotations e validação client-side.");
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

        private string ToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var pascalCase = ConvertToPascalCase(text);
            if (pascalCase.Length == 0)
                return pascalCase;

            return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
        }
    }
}
