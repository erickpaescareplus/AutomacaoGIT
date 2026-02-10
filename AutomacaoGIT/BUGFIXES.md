# ?? Correções de Bugs - Automação Git

## Versão 2.1 - Correções Críticas

### ?? Problema 1: CMD Aparecendo ao Abrir VS Code

#### ?? **Sintoma**
Ao selecionar "Visual Studio Code" e executar a automação, uma janela CMD (prompt de comando) aparecia brevemente antes de abrir o VS Code.

#### ?? **Causa Raiz**
No método `OpenVSCodeAsync` em `MainWindow.xaml.cs`, a propriedade `UseShellExecute = true` estava configurada. Isso faz com que o Windows use o shell do sistema (cmd.exe) para executar o comando, resultando em uma janela CMD visível.

```csharp
// ? ANTES (com problema)
var processInfo = new System.Diagnostics.ProcessStartInfo
{
    FileName = "code",
    Arguments = $"\"{projectPath}\"",
    UseShellExecute = true,  // ? Causa o problema
    CreateNoWindow = true
};
```

#### ? **Solução Implementada**
Alterado para `UseShellExecute = false` e adicionado redirecionamento das streams de saída:

```csharp
// ? DEPOIS (corrigido)
var processInfo = new System.Diagnostics.ProcessStartInfo
{
    FileName = "code",
    Arguments = $"\"{projectPath}\"",
    UseShellExecute = false,      // ? Corrigido
    CreateNoWindow = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};
```

#### ?? **Detalhes Técnicos**
- `UseShellExecute = false`: Executa o processo diretamente sem passar pelo shell
- `CreateNoWindow = true`: Garante que nenhuma janela de console seja criada
- Redirecionamento de streams: Permite capturar erros caso necessário

---

### ?? Problema 2: Logs Não Aparecem Durante a Execução

#### ?? **Sintoma**
Os logs da automação Git (clone, fetch, pull, checkout) só apareciam após todo o processo ser concluído. Durante a execução, a área de logs ficava vazia, dando a impressão de que a aplicação estava travada.

#### ?? **Causa Raiz**
O `ProcessHelper` estava acumulando toda a saída dos comandos Git em buffers (`StringBuilder`) e só retornava ao final do processo. Não havia callback para enviar os logs em tempo real.

```csharp
// ? ANTES (sem logs em tempo real)
process.OutputDataReceived += (sender, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
        outputBuilder.AppendLine(e.Data);  // Só acumula
};
```

#### ? **Solução Implementada**

**1. Atualizado ProcessHelper.cs:**
Adicionado parâmetro `onOutputReceived` para callback em tempo real:

```csharp
// ? Assinatura atualizada
public static async Task<(int exitCode, string output, string error)> ExecuteCommandAsync(
    string fileName,
    string arguments,
    string? workingDirectory = null,
    CancellationToken cancellationToken = default,
    Action<string>? onOutputReceived = null)  // ? Novo parâmetro
```

```csharp
// ? Callback invocado em tempo real
process.OutputDataReceived += (sender, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        outputBuilder.AppendLine(e.Data);
        onOutputReceived?.Invoke(e.Data);  // ? Envia log imediatamente
    }
};
```

**2. Atualizado GitAutomationService.cs:**
Todos os métodos que executam comandos Git agora passam um callback:

```csharp
// ? Clone com logs em tempo real
var (exitCode, output, error) = await ProcessHelper.ExecuteGitCommandAsync(
    arguments,
    request.LocalBasePath,
    cancellationToken,
    (line) => log($"  {line}"));  // ? Callback para logs
```

#### ?? **Comandos Git com Logs em Tempo Real**
- ? `git clone` - Mostra progresso do download
- ? `git fetch` - Mostra branches sendo atualizadas
- ? `git pull` - Mostra arquivos sendo atualizados
- ? `git checkout` - Mostra troca de branch

---

## ?? Benefícios das Correções

### Para o Usuário
- ? **Experiência mais limpa**: Sem janelas CMD aparecendo
- ? **Feedback visual**: Logs aparecem em tempo real
- ? **Transparência**: Vê exatamente o que está acontecendo
- ? **Confiança**: Não parece que a aplicação travou

### Técnicos
- ? **Melhor debugging**: Erros aparecem imediatamente
- ? **Performance percebida**: Usuário vê progresso
- ? **Arquitetura correta**: Callback pattern implementado
- ? **Código reutilizável**: ProcessHelper agora suporta callbacks

---

## ?? Como Testar

### Teste 1: Verificar que CMD não aparece
1. Selecione "Visual Studio Code" como editor
2. Execute uma automação
3. **Esperado**: VS Code abre sem mostrar janela CMD

### Teste 2: Verificar logs em tempo real
1. Execute uma automação de clone (repositório novo)
2. **Esperado**: Ver mensagens como:
   ```
   Executando git clone...
     Cloning into 'projeto'...
     remote: Enumerating objects: 100, done.
     remote: Counting objects: 100% (100/100), done.
     ...
   ? Repositório clonado com sucesso
   ```

3. Execute uma automação de update (repositório existente)
4. **Esperado**: Ver mensagens de fetch e pull em tempo real

---

## ?? Arquivos Modificados

### 1. `AutomacaoGIT\MainWindow.xaml.cs`
- Método `OpenVSCodeAsync`: Corrigido UseShellExecute
- Adicionado tratamento melhor de erros com dica para o usuário

### 2. `AutomacaoGIT\Helpers\ProcessHelper.cs`
- Adicionado parâmetro `onOutputReceived` em `ExecuteCommandAsync`
- Adicionado parâmetro `onOutputReceived` em `ExecuteGitCommandAsync`
- Callback invocado para cada linha de saída

### 3. `AutomacaoGIT\Services\Implementations\GitAutomationService.cs`
- Método `CloneRepositoryAsync`: Passa callback de log
- Método `UpdateRepositoryAsync`: Passa callback de log (fetch e pull)
- Método `ManageBranchAsync`: Passa callback de log (checkout e criação)
- Removido logs redundantes que já são enviados pelo callback

---

## ?? Próximas Melhorias Sugeridas

- [ ] Adicionar barra de progresso visual para operações longas
- [ ] Colorir logs por tipo (sucesso=verde, erro=vermelho, aviso=amarelo)
- [ ] Adicionar timestamp nos logs
- [ ] Salvar logs em arquivo para debug posterior
- [ ] Adicionar opção para abrir pasta do projeto no Windows Explorer

---

**Versão**: 2.1  
**Data**: Hoje  
**Status**: ? Corrigido e Testado
