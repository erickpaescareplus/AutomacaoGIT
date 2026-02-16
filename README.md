# ?? Mufasa Automações

<div align="center">

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=for-the-badge&logo=windows)
![GitHub](https://img.shields.io/badge/GitHub-API-181717?style=for-the-badge&logo=github)

**Sistema de Automação de Repositórios Git e Geração de Prompts para GitHub Copilot**

Desenvolvido pela **Squad de Credenciamento** - **CarePlus**

</div>

---

## ?? Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Configuração Inicial](#-configuração-inicial)
- [Guia de Uso](#-guia-de-uso)
  - [?? Gerenciador de Branches](#-gerenciador-de-branches)
  - [? Gerador de Prompt](#-gerador-de-prompt)
- [Estrutura de Templates](#-estrutura-de-templates)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Troubleshooting](#-troubleshooting)
- [Equipe](#-equipe)
- [Licença](#-licença)

---

## ?? Sobre o Projeto

O **Mufasa Automações** é uma ferramenta desenvolvida pela Squad de Credenciamento da CarePlus para automatizar e agilizar o fluxo de trabalho dos desenvolvedores. O nome "Mufasa" foi inspirado no personagem do Rei Leão, simbolizando liderança, força e sabedoria.

### Objetivos Principais:

1. **Automatizar operações Git** (clone, branch, checkout)
2. **Integração com GitHub** (branches remotas, tokens de autenticação)
3. **Gerar prompts estruturados** para GitHub Copilot baseados em templates
4. **Agilizar o desenvolvimento** de endpoints em projetos .NET (CORE API e BFF)
5. **Reduzir erros humanos** através de processos padronizados

---

## ? Funcionalidades

### ?? Gerenciador de Branches

- ? Clone automático de repositórios Git
- ? Listagem dinâmica de branches `feature-*` do GitHub
- ? Criação automática de branches WIT (Work Item)
- ? Checkout para branch específica
- ? Abertura automática no Visual Studio ou VS Code
- ? Suporte a repositórios privados (via token GitHub)
- ? Modo "Apenas Clonar" (sem criar branch WIT)

### ? Gerador de Prompt

- ? Geração de prompts estruturados para GitHub Copilot
- ? Templates JSON configuráveis (CORE API e BFF)
- ? Suporte a múltiplos tipos de endpoints (GET, POST, PUT, DELETE)
- ? Mapeamento automático de colunas/propriedades
- ? Substituição dinâmica de variáveis
- ? Exportação em formato Markdown
- ? Checklist de implementação incluído

### ?? Testes Automatizados (Em Breve)

Funcionalidade planejada para futuras versões.

---

## ?? Pré-requisitos

Antes de começar, certifique-se de ter instalado:

### Obrigatório:

- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** ou superior
- **Git** (versão 2.30 ou superior)
- **Visual Studio 2022** (qualquer edição) OU **Visual Studio Code**

### Opcional (mas recomendado):

- **GitHub Personal Access Token** (para repositórios privados da CarePlus)
- **Visual Studio 2022** com workload ".NET desktop development"

### Verificar Instalações:

```powershell
# Verificar .NET
dotnet --version

# Verificar Git
git --version

# Verificar VS Code (se instalado)
code --version
```

---

## ?? Instalação

### Opção 1: Download do Executável (Recomendado)

1. Acesse a [página de Releases](https://github.com/erickpaescareplus/AutomacaoGIT/releases)
2. Baixe a versão mais recente (`AutomacaoGIT-v1.x.x.zip`)
3. Extraia o arquivo ZIP em uma pasta de sua preferência (ex: `C:\Ferramentas\Mufasa`)
4. Execute `AutomacaoGIT.exe`

### Opção 2: Compilar do Código Fonte

```powershell
# 1. Clone o repositório
git clone https://github.com/erickpaescareplus/AutomacaoGIT.git
cd AutomacaoGIT

# 2. Restaurar dependências
dotnet restore

# 3. Compilar o projeto
dotnet build --configuration Release

# 4. Executar a aplicação
dotnet run --project AutomacaoGIT
```

---

## ?? Configuração Inicial

### 1. Configurar Caminho Local Base

Na primeira execução, configure o diretório onde os repositórios serão clonados:

- **Padrão**: `C:\Projetos`
- **Personalizado**: Clique no botão `?? Procurar` e selecione o diretório desejado

### 2. Configurar Token do GitHub (Obrigatório para Repos Privados)

#### Por que preciso de um token?

Os repositórios da CarePlus são **privados** e exigem autenticação. O token permite que o Mufasa:
- Liste as branches remotas (`feature-*`)
- Clone repositórios privados
- Acesse a API do GitHub sem limite de taxa

#### Como criar o token:

1. Acesse: [https://github.com/settings/tokens](https://github.com/settings/tokens)
2. Clique em **"Generate new token (classic)"**
3. Configure:
   - **Note**: `Mufasa Automações - CarePlus`
   - **Expiration**: `90 days` (ou mais)
   - **Scopes**:
     - ? `repo` (acesso completo a repositórios privados)
     - ? `read:org` (ler dados da organização)

4. Clique em **"Generate token"**
5. **COPIE O TOKEN IMEDIATAMENTE** (não será exibido novamente)

#### Configurar SSO (Single Sign-On) - IMPORTANTE!

?? **Este passo é OBRIGATÓRIO para acessar repositórios da CarePlus:**

1. Após gerar o token, role até a seção **"Configure SSO"**
2. Localize a organização **"CareplusBR"**
3. Clique em **"Authorize"**
4. Confirme a autorização

#### Adicionar o token no Mufasa:

1. Abra o **Mufasa Automações**
2. Clique no botão **"?? Token"** (na seção de Branch Feature Base)
3. Cole o token gerado
4. Clique em **"Salvar"**

? **Pronto!** Agora você tem acesso aos repositórios privados da CarePlus.

---

## ?? Guia de Uso

### ?? Gerenciador de Branches

Esta funcionalidade automatiza o processo de clonagem e criação de branches para desenvolvimento.

#### Passo a Passo:

**1. Selecione um Projeto**

Escolha no dropdown:
- `CORE - Credenciamento Prestador Cadastro API`
- `BFF - Credenciamento Prestador Cadastro BFF API`
- `WEB - Credenciamento Prestador Cadastro Web`

A URL do repositório será preenchida automaticamente.

**2. Configure o Caminho Local**

- **Padrão**: `C:\Projetos`
- Altere se necessário usando o botão `?? Procurar`

**3. Aguarde o Carregamento das Branches**

Após selecionar o projeto, o Mufasa buscará automaticamente as branches `feature-*` do GitHub.

**4. Selecione a Branch Feature Base**

Escolha a branch feature que servirá de base para o desenvolvimento:
- Exemplo: `feature-credenciamento-v2`

**5. Informe o Nome da WIT**

Digite o identificador da Work Item (WIT):
- Formato: `WIT-12345` ou apenas `12345`

**Ou marque "Apenas Clonar"** se não quiser criar uma branch WIT.

**6. Escolha o Editor**

Selecione qual IDE abrir após a clonagem:
- ?? **Visual Studio** (abre o arquivo `.sln`)
- ? **Visual Studio Code** (abre o diretório do projeto)

**7. Execute a Automação**

Clique em **"?? Executar Automação"**

#### O que o Mufasa faz automaticamente:

1. ? Clona o repositório (se não existir)
2. ? Faz checkout para a branch feature selecionada
3. ? Cria uma nova branch WIT a partir da feature (se solicitado)
4. ? Abre o projeto no editor escolhido
5. ? Exibe logs detalhados de cada etapa

#### Logs de Exemplo:

```
=== INICIANDO EXECUÇÃO ===
??? Branch WIT: WIT-12345
?? Branch base: feature-credenciamento-v2

? Git encontrado no sistema
?? Clonando repositório...
? Repositório clonado com sucesso
? Checkout realizado para feature base: feature-credenciamento-v2
? Branch criada e checkout realizado: WIT-12345
Solution encontrada: C:\Projetos\credenciamento-prestador-cadastro-api\CarePlus.sln
Abrindo Visual Studio...
? Visual Studio aberto com sucesso!

=== SUCESSO! ===
```

---

### ? Gerador de Prompt

Gera prompts estruturados para o GitHub Copilot, facilitando a implementação de endpoints seguindo os padrões da CarePlus.

#### Passo a Passo:

**1. Selecione o Tipo de Projeto**

- ?? **CORE - API de Cadastro**
- ? **BFF - API Gateway**
- ? **WEB - Interface Web** (Em Breve)

**2. Preencha os Campos Obrigatórios**

| Campo | Descrição | Exemplo |
|-------|-----------|---------|
| **Nome do Controller** | Nome do controller sem "Controller" | `ProviderRegister` |
| **Nome da Tabela** | Nome da tabela no banco (apenas CORE) | `TB_PROVIDER_CITY` |
| **Tipo de Endpoint** | Método HTTP | `Get` / `Post` / `Put` / `Delete` |
| **Nome do Endpoint** | Nome do endpoint na rota | `City` |
| **Nome do Método** | Nome do método no código | `GetCity` |

**3. Configure as Colunas/Propriedades**

Adicione as colunas da tabela ou propriedades do DTO:

- **Nome**: `ID_CIDADE` (formato banco de dados)
- **Tipo**: `int`, `string`, `DateTime`, etc.

Clique em **"? Adicionar Coluna"** para adicionar mais campos.

**4. Gere o Prompt**

Clique em **"? Gerar Prompt"**

**5. Salve o Arquivo**

Clique em **"?? Salvar Prompt em Arquivo"** e escolha o local e formato:
- `COPILOT-PROMPT.txt` (texto)
- `COPILOT-PROMPT.md` (markdown)

#### Exemplo de Prompt Gerado:

```markdown
# ?? Prompt para GitHub Copilot - Implementação de Endpoint

## ?? Informações do Endpoint

- **Controller:** ProviderRegister
- **Tabela:** TB_PROVIDER_CITY
- **Tipo de Endpoint:** Get
- **Nome do Endpoint:** City
- **Nome do Método:** GetCity
- **Colunas/Propriedades:**
  - `int IdCidade`
  - `string NomeCidade`

---

## ?? Etapas de Implementação

### ETAPA 1 - CRIAÇÃO DO MAPEAMENTO DO RESPONSE/DOMAIN

**Descrição:** Inserir novo mapeamento conforme abaixo:
**Caminho:** `building-blocks/Application/ProviderRegister/Mappings/ProviderProfile.cs`

**Exemplo de implementação:**
```csharp
CreateMap<CityResponse, City>().ReverseMap();
```

### ETAPA 2 - CRIAÇÃO DO RESPONSE
...
```

---

## ?? Estrutura de Templates

Os templates de prompt estão localizados em:

```
AutomacaoGIT/
??? templatesGeradorPrompt/
    ??? netCore8-CQRS-API-Sandbox012026.json    (Template para CORE API)
    ??? netCore8-CQRS-BFF-Sandbox012026.json    (Template para BFF)
```

### Variáveis Disponíveis nos Templates:

| Variável | Descrição |
|----------|-----------|
| `{nomeController}` | Nome do controller |
| `{tabela}` | Nome da tabela no banco |
| `{tipoEndpoint}` | Tipo do endpoint (Get/Post/Put/Delete) |
| `{nomeEndpoint}` | Nome do endpoint |
| `{nomeMetodo}` | Nome do método |
| `{colunas}` | Lista formatada de colunas |

### Personalizar Templates:

Você pode editar os arquivos JSON para adaptar às suas necessidades. O formato esperado é:

```json
{
  "steps": [
    {
      "titulo": "ETAPA 1 - TÍTULO DA ETAPA",
      "descricao": "Descrição do que fazer",
      "caminho": "path/para/{nomeController}/arquivo.cs",
      "exemplo": "Código de exemplo com {variáveis}"
    }
  ]
}
```

---

## ??? Tecnologias Utilizadas

### Framework e Linguagem:
- **.NET 8.0** - Framework principal
- **C# 12** - Linguagem de programação
- **WPF (Windows Presentation Foundation)** - Interface gráfica

### Bibliotecas e APIs:
- **System.Text.Json** - Serialização/deserialização JSON
- **EnvDTE** - Automação do Visual Studio
- **GitHub API** - Integração com repositórios remotos
- **System.Diagnostics** - Execução de processos (Git, VS Code)

### Padrões e Arquitetura:
- **MVVM** (Model-View-ViewModel) - Padrão de interface
- **Dependency Injection** - Injeção de dependências
- **Service Layer** - Camada de serviços
- **DTO Pattern** - Objetos de transferência de dados

---

## ?? Troubleshooting

### Problema: "Template JSON inválido ou vazio"

**Causa**: Arquivos de template não foram copiados para o diretório de saída.

**Solução**:
1. Feche e reabra a aplicação
2. Verifique se os arquivos `.json` existem em `templatesGeradorPrompt/`
3. Recompile o projeto se necessário

---

### Problema: "Repositório privado" ou "Token inválido"

**Causa**: Token GitHub não configurado ou inválido.

**Solução**:
1. Clique no botão **"?? Token"**
2. Verifique se o token tem o scope `repo`
3. **IMPORTANTE**: Configure SSO para a organização CareplusBR
4. Salve o token novamente

---

### Problema: "Acesso negado - Configure SSO"

**Causa**: SSO não autorizado para a organização CareplusBR.

**Solução**:
1. Acesse: [https://github.com/settings/tokens](https://github.com/settings/tokens)
2. Localize seu token
3. Clique em **"Configure SSO"**
4. Autorize a organização **"CareplusBR"**
5. No Mufasa, clique em **"?? Token"** e salve novamente

---

### Problema: "O comando 'code' não foi encontrado"

**Causa**: VS Code não está no PATH do sistema.

**Solução**:
1. Durante a instalação do VS Code, marque: **"Adicionar ao PATH"**
2. Ou adicione manualmente:
   ```
   C:\Users\[seu-usuario]\AppData\Local\Programs\Microsoft VS Code\bin
   ```
3. Reinicie o Mufasa após adicionar ao PATH

---

### Problema: "Nenhuma solution (.sln) encontrada"

**Causa**: O repositório não possui arquivo `.sln` na raiz.

**Solução**:
- Use **Visual Studio Code** ao invés de Visual Studio
- Ou localize manualmente o arquivo `.sln` e abra

---

### Problema: Erro ao clonar repositório

**Possíveis Causas e Soluções**:

1. **Git não instalado**:
   - Instale o Git: [https://git-scm.com/downloads](https://git-scm.com/downloads)
   - Reinicie o computador após a instalação

2. **Sem permissão no diretório**:
   - Escolha um diretório onde você tem permissão de escrita
   - Exemplo: `C:\Projetos` ou `C:\Users\[seu-usuario]\Projetos`

3. **Repositório já existe**:
   - O Mufasa detecta automaticamente e faz checkout/pull
   - Ou delete a pasta manualmente e clone novamente

---

## ?? Equipe

### Desenvolvido por:

**Squad de Credenciamento - CarePlus**

### Contribuidores:

- **Erick Paes** - [@erickpaescareplus](https://github.com/erickpaescareplus)
- **Squad de Credenciamento** - Colaboradores e testers

### Agradecimentos Especiais:

- **GitHub Copilot** - Por auxiliar no desenvolvimento
- **Equipe de Tecnologia CarePlus** - Pelo suporte e feedback
- **Comunidade .NET** - Pelas bibliotecas e ferramentas open source

---

## ?? Licença

Este projeto é de propriedade da **CarePlus** e é destinado para uso interno pela equipe de desenvolvimento.

**Uso Restrito**: Não distribuir ou modificar sem autorização.

---

## ?? Suporte

### Encontrou um bug?

Abra uma **Issue** no GitHub: [Reportar Bug](https://github.com/erickpaescareplus/AutomacaoGIT/issues)

### Sugestões de Melhorias?

Envie um **Pull Request** ou abra uma **Issue** com a tag `enhancement`.

### Contato Direto:

- **E-mail**: [erick.paes@careplus.com.br](mailto:erick.paes@careplus.com.br)
- **Teams**: Erick Paes (Squad Credenciamento)

---

<div align="center">

**Feito com ?? pela Squad de Credenciamento - CarePlus**

?? **Mufasa Automações** - *Liderando o caminho da automação!*

</div>
