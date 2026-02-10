# ?? GUIA RÁPIDO DE INÍCIO

## ? Como Usar em 3 Passos

### 1?? Execute a Aplicação
- Compile e execute o projeto (F5 no Visual Studio)
- A interface WIT será exibida

### 2?? Preencha os Dados
```
URL do Repositório:      https://dev.azure.com/sua-org/projeto/_git/repo
Caminho Local Base:      C:\Projetos
Nome da Pasta:           MeuProjeto
Branch (opcional):       feature/nova-feature
```

### 3?? Execute!
- Clique em "?? Executar Automação"
- Aguarde os logs aparecerem
- O Visual Studio abrirá automaticamente com a solution

---

## ?? Cenários Comuns

### Cenário 1: Clone Inicial
**Situação:** Primeira vez trabalhando no projeto

**Configuração:**
- URL: `https://github.com/empresa/projeto.git`
- Caminho: `C:\Projetos`
- Pasta: `ProjetoXYZ`
- Branch: *(deixar vazio para usar default)*

**O que acontece:**
1. ? Clona o repositório
2. ? Busca a solution (.sln)
3. ? Abre no Visual Studio

---

### Cenário 2: Nova Feature Branch
**Situação:** Iniciar desenvolvimento de nova funcionalidade

**Configuração:**
- URL: `https://dev.azure.com/org/proj/_git/api`
- Caminho: `C:\Dev`
- Pasta: `MinhaAPI`
- Branch: `feature/login-jwt`
- Prompt IA: `Implementar autenticação JWT com refresh token`

**O que acontece:**
1. ? Atualiza repositório existente (git pull)
2. ? Cria branch `feature/login-jwt`
3. ? Salva prompt em `.ai-prompt.txt`
4. ? Abre no Visual Studio

---

### Cenário 3: Code Review
**Situação:** Revisar PR de colega

**Configuração:**
- URL: `https://github.com/empresa/backend.git`
- Caminho: `C:\Reviews`
- Pasta: `Backend-PR-1234`
- Branch: `feature/fix-bug-payment`

**O que acontece:**
1. ? Clona ou atualiza repositório
2. ? Faz checkout da branch do PR
3. ? Abre no Visual Studio para revisão

---

### Cenário 4: Atualização Rápida
**Situação:** Sincronizar com última versão da main

**Configuração:**
- URL: `https://dev.azure.com/org/proj/_git/web`
- Caminho: `C:\Projetos`
- Pasta: `WebApp`
- Branch: `main`

**O que acontece:**
1. ? git fetch --all
2. ? git checkout main
3. ? git pull
4. ? Abre no Visual Studio atualizado

---

## ?? Validações do Sistema

Antes de executar, o sistema valida:
- ? Git instalado no sistema
- ? Campos obrigatórios preenchidos
- ? Caminho base válido (cria se não existir)

---

## ?? Entendendo os Logs

### Tipos de Mensagens

```
[HH:mm:ss] Mensagem normal           ? Informação
[HH:mm:ss] ? Sucesso                ? Operação bem-sucedida
[HH:mm:ss] ? Aviso                  ? Atenção necessária
[HH:mm:ss] ERRO: Descrição          ? Falha na operação
```

### Exemplo de Log Completo
```
[14:30:15] === INICIANDO AUTOMAÇÃO GIT ===
[14:30:15] Repositório: https://github.com/empresa/projeto.git
[14:30:15] Caminho local: C:\Projetos\ProjetoXYZ
[14:30:15] ? Git encontrado no sistema
[14:30:16] Clonando repositório...
[14:30:45] ? Repositório clonado com sucesso
[14:30:45] Gerenciando branch: develop
[14:30:45] Criando nova branch: develop
[14:30:46] ? Branch criada e checkout realizado: develop
[14:30:46] === AUTOMAÇÃO CONCLUÍDA COM SUCESSO EM 31.24s ===
[14:30:46] Solution encontrada: C:\Projetos\ProjetoXYZ\ProjetoXYZ.sln
[14:30:46] Abrindo Visual Studio...
[14:30:47] ? Visual Studio aberto com sucesso!
```

---

## ? Solução de Problemas

### Problema: "Git não está instalado"
**Solução:**
1. Baixe Git: https://git-scm.com/download/win
2. Instale com opção "Add to PATH"
3. Reinicie a aplicação

### Problema: "Erro ao clonar repositório"
**Possíveis causas:**
- URL incorreta
- Sem acesso ao repositório
- Credenciais não configuradas

**Solução:**
```bash
# Configure credenciais do Git
git config --global user.name "Seu Nome"
git config --global user.email "email@empresa.com"

# Para Azure DevOps, use Personal Access Token (PAT)
# Windows Credential Manager salvará automaticamente
```

### Problema: "Visual Studio não abre"
**Possíveis causas:**
- Visual Studio não instalado
- Nenhuma solution (.sln) no projeto

**Solução:**
- Verifique se há arquivo .sln no projeto
- Abra manualmente: File > Open > Project/Solution

### Problema: "Alterações locais detectadas"
**O que significa:**
- Você tem modificações não commitadas

**Ação:**
- Faça commit/stash das alterações antes
- Ou ignore e continue (pull pode falhar)

---

## ?? Recursos Avançados

### Uso de Prompts de IA

O campo "Prompt para IA" salva um arquivo `.ai-prompt.txt` na raiz do projeto.

**Casos de uso:**
- Documentar objetivo da feature
- Instruções para agente de IA
- Checklist de desenvolvimento

**Exemplo:**
```
Implementar API de Pagamentos:
- Endpoint POST /api/payments
- Integração com Stripe
- Validação de cartão
- Tratamento de erros
- Testes unitários
```

---

## ?? Produtividade

### Atalhos Recomendados

Crie um arquivo de configuração rápida:

**repos-config.json**
```json
{
  "projetos": [
    {
      "nome": "Backend API",
      "url": "https://dev.azure.com/org/proj/_git/backend",
      "caminho": "C:\\Projetos",
      "pasta": "Backend"
    },
    {
      "nome": "Frontend Web",
      "url": "https://dev.azure.com/org/proj/_git/frontend",
      "caminho": "C:\\Projetos",
      "pasta": "Frontend"
    }
  ]
}
```

---

## ?? Suporte

**Problemas ou dúvidas?**
- Verifique os logs na área de execução
- Consulte o README.md completo
- Contate o time de DevOps

---

**Última Atualização:** 2024
**Versão:** 1.0
