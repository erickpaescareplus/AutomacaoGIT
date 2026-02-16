using AutomacaoGIT.Services.Interfaces;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using EnvDTE;
using EnvDTE80;
using SysProcess = System.Diagnostics.Process;

namespace AutomacaoGIT.Services.Implementations
{
    /// <summary>
    /// Implementação do serviço de integração com o Copilot Chat do Visual Studio
    /// </summary>
    public class CopilotChatService : ICopilotChatService
    {
        // Importações do Windows API para funções auxiliares
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        private static SysProcess? _lastVsProcess;

        public async Task<bool> SendPromptToCopilotAsync(string prompt, bool waitForVsReady = true, int maxWaitSeconds = 120, Action<string>? onLogReceived = null)
        {
            try
            {
                onLogReceived?.Invoke("?? [DEBUG] Iniciando SendPromptToCopilotAsync...");
                
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    onLogReceived?.Invoke("? [DEBUG] Prompt está vazio ou nulo");
                    return false;
                }

                onLogReceived?.Invoke($"?? [DEBUG] Prompt a ser enviado: {prompt.Substring(0, Math.Min(50, prompt.Length))}...");

                // ESTRATÉGIA SIMPLIFICADA: Aguardar tempo fixo de 45 segundos
                if (waitForVsReady && _lastVsProcess != null)
                {
                    onLogReceived?.Invoke($"? [ESTRATÉGIA] Aguardando 45 segundos para VS carregar completamente...");
                    onLogReceived?.Invoke($"   Process ID: {_lastVsProcess.Id}");
                    onLogReceived?.Invoke($"   Esta espera é necessária para garantir que o VS carregue todos os componentes");
                    onLogReceived?.Invoke("");
                    
                    // Countdown visual a cada 5 segundos
                    for (int i = 45; i > 0; i -= 5)
                    {
                        onLogReceived?.Invoke($"??  {i} segundos restantes...");
                        await Task.Delay(5000);
                    }
                    
                    onLogReceived?.Invoke("");
                    onLogReceived?.Invoke("? [ESTRATÉGIA] Tempo de espera concluído! VS deve estar pronto.");
                }
                else if (_lastVsProcess == null)
                {
                    onLogReceived?.Invoke("?? [DEBUG] _lastVsProcess é null! Pulando espera.");
                }

                // Busca o processo do Visual Studio
                onLogReceived?.Invoke("?? [DEBUG] Buscando processos 'devenv'...");
                var vsProcesses = SysProcess.GetProcessesByName("devenv");
                onLogReceived?.Invoke($"?? [DEBUG] Encontrados {vsProcesses.Length} processo(s) do VS");
                
                if (vsProcesses.Length == 0)
                {
                    onLogReceived?.Invoke("? [DEBUG] Nenhum processo 'devenv' encontrado");
                    return false;
                }

                // Pega o processo mais recente (último aberto)
                var vsProcess = _lastVsProcess ?? vsProcesses.OrderByDescending(p => p.StartTime).FirstOrDefault();
                if (vsProcess == null)
                {
                    onLogReceived?.Invoke("? [DEBUG] Não foi possível obter o processo do VS");
                    return false;
                }

                onLogReceived?.Invoke($"? [DEBUG] Processo selecionado: PID={vsProcess.Id}, Name={vsProcess.ProcessName}");

                // Verifica se o processo ainda está vivo
                if (vsProcess.HasExited)
                {
                    onLogReceived?.Invoke("? [DEBUG] Processo do VS foi fechado!");
                    return false;
                }

                // Obtém o título da janela
                var mainWindowHandle = vsProcess.MainWindowHandle;
                var windowTitle = GetWindowTitle(mainWindowHandle);
                onLogReceived?.Invoke($"?? [DEBUG] Título da janela: '{windowTitle}'");

                // NOVA ABORDAGEM: Usar DTE API do Visual Studio
                onLogReceived?.Invoke("");
                onLogReceived?.Invoke("?? [DTE API] Tentando conectar via DTE (Development Tools Extensibility)...");
                
                DTE2? dte = null;
                
                try
                {
                    // Tenta obter DTE do processo específico
                    dte = GetDTE(vsProcess.Id, onLogReceived);
                    
                    if (dte == null)
                    {
                        onLogReceived?.Invoke("?? [DTE API] Não foi possível conectar via DTE");
                        onLogReceived?.Invoke("?? [FALLBACK] Usando método alternativo com clipboard...");
                        return await SendViaClipboardFallback(prompt, mainWindowHandle, onLogReceived);
                    }

                    onLogReceived?.Invoke($"? [DTE API] Conectado ao Visual Studio via DTE!");
                    onLogReceived?.Invoke($"   Versão: {dte.Version}");
                    onLogReceived?.Invoke($"   Edição: {dte.Edition}");

                    // Tenta executar comando do Copilot Chat
                    onLogReceived?.Invoke("");
                    onLogReceived?.Invoke("?? [DTE API] Tentando abrir Copilot Chat via comando...");
                    
                    onLogReceived?.Invoke("");
                    onLogReceived?.Invoke("?? [DTE API] Tentando comandos conhecidos do Copilot...");
                    
                    // Lista expandida de possíveis comandos do Copilot
                    string[] copilotCommands = new[]
                    {
                        "View.GitHubCopilotChat",
                        "Tools.GitHubCopilotChat", 
                        "GitHub.Copilot.OpenChat",
                        "GitHub.Copilot.Chat.OpenChatWindow",
                        "GitHub.Copilot.ToggleChat",
                        "GitHub.Copilot.Chat.FocusChatWindow",
                        "GitHub.Copilot.ShowChatWindow",
                        "Window.GitHubCopilot",
                        "View.OtherWindows.GitHubCopilot"
                    };

                    bool commandExecuted = false;
                    string? lastError = null;
                    
                    foreach (var cmd in copilotCommands)
                    {
                        try
                        {
                            onLogReceived?.Invoke($"   Tentando comando: {cmd}");
                            
                            // Verifica se comando existe e está disponível
                            try
                            {
                                var command = dte.Commands.Item(cmd);
                                if (!command.IsAvailable)
                                {
                                    onLogReceived?.Invoke($"   ?? Comando existe mas não está disponível no momento");
                                    continue;
                                }
                            }
                            catch
                            {
                                onLogReceived?.Invoke($"   ?? Comando não encontrado no VS");
                                continue;
                            }
                            
                            // Tenta executar
                            dte.ExecuteCommand(cmd);
                            onLogReceived?.Invoke($"   ? Comando executado: {cmd}");
                            commandExecuted = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            lastError = ex.Message;
                            var errorType = ex.GetType().Name;
                            
                            if (ex.Message.Contains("E_FAIL"))
                            {
                                onLogReceived?.Invoke($"   ? Comando falhou (E_FAIL) - Copilot pode não estar instalado/ativado");
                            }
                            else if (ex.Message.Contains("not available"))
                            {
                                onLogReceived?.Invoke($"   ?? Comando não disponível");
                            }
                            else
                            {
                                onLogReceived?.Invoke($"   ? Erro: {ex.Message}");
                            }
                        }
                    }

                    if (!commandExecuted)
                    {
                        onLogReceived?.Invoke("");
                        onLogReceived?.Invoke("? [DTE API] Nenhum comando do Copilot funcionou");
                        onLogReceived?.Invoke("");
                        onLogReceived?.Invoke("?? [DIAGNÓSTICO] Possíveis causas:");
                        onLogReceived?.Invoke("   1. GitHub Copilot não está instalado no Visual Studio");
                        onLogReceived?.Invoke("   2. Copilot está instalado mas não ativado");
                        onLogReceived?.Invoke("   3. Você não está logado na conta GitHub com Copilot");
                        onLogReceived?.Invoke("   4. Extensão do Copilot precisa ser atualizada");
                        onLogReceived?.Invoke("");
                        onLogReceived?.Invoke("?? [SOLUÇÃO] Verifique:");
                        onLogReceived?.Invoke("   • Extensions > Manage Extensions > Busque 'GitHub Copilot'");
                        onLogReceived?.Invoke("   • View > Other Windows > GitHub Copilot Chat (menu manual)");
                        onLogReceived?.Invoke("   • Tools > Options > GitHub Copilot");
                        onLogReceived?.Invoke("");
                        onLogReceived?.Invoke($"   Último erro: {lastError}");
                        onLogReceived?.Invoke("");
                        onLogReceived?.Invoke("?? [FALLBACK] Tentando método alternativo com atalho de teclado...");
                        return await SendViaClipboardFallback(prompt, mainWindowHandle, onLogReceived);
                    }

                    onLogReceived?.Invoke("? [DTE API] Aguardando 2 segundos para Copilot Chat abrir...");
                    await Task.Delay(2000);

                    // Tenta enviar o prompt via automação
                    onLogReceived?.Invoke("?? [DTE API] Enviando prompt para Copilot Chat...");
                    
                    // Copia para clipboard como fallback
                    System.Windows.Clipboard.SetText(prompt);
                    onLogReceived?.Invoke("?? [DEBUG] Prompt copiado para clipboard");
                    
                    // Simula Ctrl+V e Enter
                    await Task.Delay(500);
                    System.Windows.Forms.SendKeys.SendWait("^v");
                    onLogReceived?.Invoke("?? [DEBUG] Ctrl+V enviado");
                    
                    await Task.Delay(1000);
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    onLogReceived?.Invoke("?? [DEBUG] Enter enviado");

                    onLogReceived?.Invoke("");
                    onLogReceived?.Invoke("? [DTE API] Sequência de comandos concluída com sucesso!");
                    return true;
                }
                finally
                {
                    // Libera COM object
                    if (dte != null)
                    {
                        Marshal.ReleaseComObject(dte);
                    }
                }
            }
            catch (Exception ex)
            {
                onLogReceived?.Invoke($"?? [DEBUG] EXCEÇÃO: {ex.GetType().Name}");
                onLogReceived?.Invoke($"   Mensagem: {ex.Message}");
                onLogReceived?.Invoke($"   StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private DTE2? GetDTE(int processId, Action<string>? onLogReceived = null)
        {
            try
            {
                onLogReceived?.Invoke($"?? [DTE] Procurando DTE para processo {processId}...");
                
                // Obtém a ROT (Running Object Table)
                IRunningObjectTable rot;
                IEnumMoniker enumMoniker;
                int retVal = GetRunningObjectTable(0, out rot);
                
                if (retVal != 0)
                {
                    onLogReceived?.Invoke("? [DTE] Falha ao obter Running Object Table");
                    return null;
                }

                rot.EnumRunning(out enumMoniker);
                enumMoniker.Reset();

                IntPtr fetched = IntPtr.Zero;
                IMoniker[] monikers = new IMoniker[1];
                
                int dteCount = 0;
                List<DTE2> allDtes = new List<DTE2>();

                onLogReceived?.Invoke("?? [DTE] Enumerando objetos na ROT...");

                while (enumMoniker.Next(1, monikers, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    
                    string displayName;
                    monikers[0].GetDisplayName(bindCtx, null, out displayName);

                    // Procura por Visual Studio DTE
                    if (displayName.StartsWith("!VisualStudio.DTE"))
                    {
                        dteCount++;
                        onLogReceived?.Invoke($"   Encontrado: {displayName}");
                        
                        try
                        {
                            object obj;
                            rot.GetObject(monikers[0], out obj);
                            
                            if (obj is DTE2 dte)
                            {
                                allDtes.Add(dte);
                                onLogReceived?.Invoke($"   ? DTE válido #{dteCount}");
                                onLogReceived?.Invoke($"      Versão: {dte.Version}");
                                onLogReceived?.Invoke($"      Nome: {dte.Name}");
                                
                                // Tenta obter informações da solution
                                try
                                {
                                    if (dte.Solution != null && !string.IsNullOrEmpty(dte.Solution.FullName))
                                    {
                                        onLogReceived?.Invoke($"      Solution: {System.IO.Path.GetFileName(dte.Solution.FullName)}");
                                    }
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            onLogReceived?.Invoke($"   ?? Erro ao obter DTE: {ex.Message}");
                        }
                    }

                    Marshal.ReleaseComObject(bindCtx);
                }

                onLogReceived?.Invoke($"?? [DTE] Total de DTEs encontrados: {allDtes.Count}");

                if (allDtes.Count == 0)
                {
                    onLogReceived?.Invoke("?? [DTE] Nenhum DTE encontrado na ROT");
                    onLogReceived?.Invoke("?? [DTE] Isso pode acontecer se:");
                    onLogReceived?.Invoke("   - VS ainda não registrou o DTE (muito cedo)");
                    onLogReceived?.Invoke("   - VS foi aberto por outro método");
                    onLogReceived?.Invoke("   - Permissões COM estão bloqueadas");
                    return null;
                }

                // Retorna o DTE mais recente (último na lista)
                var selectedDte = allDtes.Last();
                onLogReceived?.Invoke($"? [DTE] Selecionando DTE mais recente (#{allDtes.Count})");
                
                // Libera os DTEs não utilizados
                for (int i = 0; i < allDtes.Count - 1; i++)
                {
                    try
                    {
                        Marshal.ReleaseComObject(allDtes[i]);
                    }
                    catch { }
                }
                
                return selectedDte;
            }
            catch (Exception ex)
            {
                onLogReceived?.Invoke($"? [DTE] Erro ao buscar DTE: {ex.Message}");
                onLogReceived?.Invoke($"   Stack: {ex.StackTrace}");
                return null;
            }
        }

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        private async Task<bool> SendViaClipboardFallback(string prompt, IntPtr windowHandle, Action<string>? onLogReceived = null)
        {
            try
            {
                onLogReceived?.Invoke("?? [FALLBACK] Usando método de clipboard + SendKeys...");
                
                // Copia prompt para clipboard
                System.Windows.Clipboard.SetText(prompt);
                onLogReceived?.Invoke("? [FALLBACK] Prompt copiado para clipboard");
                
                await Task.Delay(500);
                
                // Tenta abrir Copilot Chat com Ctrl+/
                onLogReceived?.Invoke("?? [FALLBACK] Enviando Ctrl+/ ...");
                System.Windows.Forms.SendKeys.SendWait("^{/}");
                await Task.Delay(2000);
                
                // Cola o prompt
                onLogReceived?.Invoke("?? [FALLBACK] Enviando Ctrl+V ...");
                System.Windows.Forms.SendKeys.SendWait("^v");
                await Task.Delay(1000);
                
                // Envia
                onLogReceived?.Invoke("?? [FALLBACK] Enviando Enter ...");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                
                onLogReceived?.Invoke("? [FALLBACK] Sequência concluída");
                return true;
            }
            catch (Exception ex)
            {
                onLogReceived?.Invoke($"? [FALLBACK] Erro: {ex.Message}");
                return false;
            }
        }

        private string GetWindowTitle(IntPtr handle)
        {
            try
            {
                var length = GetWindowTextLength(handle);
                if (length == 0) return string.Empty;

                var builder = new StringBuilder(length + 1);
                GetWindowText(handle, builder, builder.Capacity);
                return builder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<bool> IsVisualStudioReadyAsync(int processId)
        {
            try
            {
                var process = SysProcess.GetProcessById(processId);
                
                // Verificação básica: processo existe e janela está disponível
                if (process.MainWindowHandle == IntPtr.Zero)
                    return false;

                if (!IsWindowVisible(process.MainWindowHandle))
                    return false;

                if (!process.Responding)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task SimulateKeyPress(IntPtr windowHandle, uint modifierKey, uint key, Action<string>? onLogReceived = null)
        {
            try
            {
                // Usa SendKeys do System.Windows.Forms (mais confiável para WPF)
                await Task.Run(() =>
                {
                    if (modifierKey == 0x11) // Ctrl
                    {
                        if (key == 0xBF) // /
                        {
                            onLogReceived?.Invoke("   Enviando: Ctrl + /");
                            System.Windows.Forms.SendKeys.SendWait("^{/}");
                        }
                        else if (key == 0x56) // V
                        {
                            onLogReceived?.Invoke("   Enviando: Ctrl + V");
                            System.Windows.Forms.SendKeys.SendWait("^v");
                        }
                    }
                    else if (key == 0x0D) // Enter
                    {
                        onLogReceived?.Invoke("   Enviando: Enter");
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    }
                });
                
                await Task.Delay(100);
                onLogReceived?.Invoke("   ? Tecla enviada");
            }
            catch (Exception ex)
            {
                onLogReceived?.Invoke($"   ? Erro ao simular tecla: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra o último processo do Visual Studio aberto
        /// </summary>
        public static void RegisterVisualStudioProcess(SysProcess process)
        {
            _lastVsProcess = process;
        }
    }
}
