# ?? MUFASA - Ferramenta de Automação Git

> **M**odernização **U**nificada para **F**acilitar **A**utomação de **S**oftware **A**vançado

## ?? Índice

- [Visão Geral](#-visão-geral)
- [Funcionalidades](#-funcionalidades)
- [Requisitos](#-requisitos)
- [Instalação](#-instalação)
- [Como Usar](#-como-usar)
- [Gerador de Prompt](#-gerador-de-prompt)
- [Arquitetura](#-arquitetura)
- [Troubleshooting](#-troubleshooting)
- [Changelog](#-changelog)

---

## ?? Visão Geral

**MUFASA** é uma ferramenta desktop desenvolvida em .NET 8 (WPF) que automatiza operações Git e facilita o desenvolvimento com geração inteligente de prompts para GitHub Copilot.

### ?? Principais Benefícios

- ? **Automação Completa**: Clone, pull, checkout e criação de branches automatizados
- ?? **Geração de Prompts**: Templates CORE e BFF para desenvolvimento padronizado
- ?? **Interface Intuitiva**: UI moderna e responsiva com feedback em tempo real
- ?? **Integração IDE**: Abertura automática no Visual Studio ou VS Code
- ?? **Logs Detalhados**: Acompanhamento completo de todas as operações

---

## ? Funcionalidades

### 1. Automação Git

#### Operações Suportadas
- ? **Clone**: Clonagem automática de repositórios
- ? **Pull**: Atualização de repositórios existentes
- ? **Checkout**: Troca entre branches existentes
- ? **Create Branch**: Criação de novas branches a partir de feature branches
- ? **Fetch**: Busca de atualizações remotas

#### Projetos Pré-configurados
- **CORE**: Credenciamento Prestador Cadastro API
- **BFF**: Credenciamento Prestador Cadastro BFF API
- **WEB**: Credenciamento Prestador Cadastro Web

### 2. Gerador de Prompt

#### Templates Disponíveis

##### Template CORE (7 Etapas)
1. Criação do Mapeamento Response/Domain
2. Criação do Response
3. Criação da Interface e Services
4. Criação Domain (Domain + Interface Repository)
5. Apontar o Serviço no Módulo
6. Adicionando a Query SQL no Repository
7. Criar o Controller

##### Template BFF (5 Etapas)
1. Criação do Controller
2. Criação dos Services
3. Criação do Response Model
4. Interface API Service
5. Implementação API Service

#### Campos Dinâmicos
- **Sistema de Colunas**: Adicione/remova campos dinamicamente
- **Tipos Suportados**: int, string, bool, decimal, DateTime, Guid, long, double
- **Validação Automática**: Campos obrigatórios validados antes da geração
- **Template Condicional**: Campo "Nome da Tabela" apenas para CORE

---

## ?? Requisitos

### Sistema
- **OS**: Windows 10/11
- **.NET**: .NET 8 Runtime
- **Git**: Git instalado e no PATH do sistema
- **IDE** (opcional): Visual Studio 2022 ou VS Code

---

## ?? Instalação

```bash
# Clone o repositório
git clone https://github.com/erickpaescareplus/AutomacaoGIT.git
cd AutomacaoGIT

# Compile o projeto
dotnet build

# Execute
dotnet run --project AutomacaoGIT
```

---

## ?? Como Usar

### Quick Start

1. **Configure o Repositório**
   - Selecione o projeto (CORE/BFF/WEB)
   - Defina o caminho local (ex: `C:\Projetos`)
   - Escolha a branch feature base
   - Informe o nome da WIT

2. **Gerador de Prompt** (Opcional)
   - ? Marque "Habilitar Gerador de Prompt"
   - Selecione o template (CORE ou BFF)
   - Preencha os campos obrigatórios
   - Adicione colunas com "? Adicionar Coluna"

3. **Execute**
   - Clique em "?? Executar Automação"
   - Acompanhe os logs
   - Visual Studio abrirá automaticamente

### Exemplo Prático

#### Criar Endpoint GET para Consultar Cidades (CORE)

**Configuração**:
```
?? Template: CORE
?? Controller: ProviderRegister
??? Tabela: TB_PROVIDER_CITY
?? Endpoint: Get
?? Nome: City
? Método: GetCity
?? Colunas:
   - ID_CIDADE: int
   - NOME_CIDADE: string
   - COD_ESTADO: int
   - ATIVO: bool
```

**Resultado**:
- Arquivo gerado: `PROMPT_ProviderRegister_City_20240115_143022.md`
- Visual Studio abre com o prompt
- Pressione `Ctrl + /` e use: `@workspace Execute as instruções deste documento`

---

## ?? Gerador de Prompt

### Variáveis Substituídas

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `{nomeController}` | Nome do Controller | ProviderRegister |
| `{tipoEndpoint}` | Tipo HTTP | Get, Post, Put, Delete |
| `{nomeEndpoint}` | Nome do Endpoint | City |
| `{nomeMetodo}` | Nome do Método | GetCity |
| `{colunas}` | Colunas formatadas | ID:int; NOME:string |
| `{tabela}` | Nome da Tabela | TB_PROVIDER_CITY |

### Formato de Saída

```markdown
## ETAPA 1 - CRIAÇÃO DO MAPEAMENTO

**Caminho:** `building-blocks/Application/ProviderRegister/Mappings/ProviderProfile.cs`

**Exemplo:**
```csharp
CreateMap<CityResponse, City>().ReverseMap();
```
---
```

---

## ??? Arquitetura

### Estrutura do Projeto

```
AutomacaoGIT/
??? Models/
?   ??? DTOs/
?   ?   ??? GitAutomationRequest.cs
?   ?   ??? GitAutomationResult.cs
?   ?   ??? PromptGeneratorRequest.cs
?   ??? Enums/
?   ?   ??? GitOperationStatus.cs
?   ??? Templates/
?       ??? PromptTemplate.cs
??? Services/
?   ??? Interfaces/
?   ?   ??? IGitAutomationService.cs
?   ?   ??? IVisualStudioLauncher.cs
?   ?   ??? IPromptGeneratorService.cs
?   ??? Implementations/
?       ??? GitAutomationService.cs
?       ??? VisualStudioLauncher.cs
?       ??? PromptGeneratorService.cs
??? Helpers/
?   ??? ProcessHelper.cs
??? MainWindow.xaml
??? MainWindow.xaml.cs
```

### Componentes Principais

- **GitAutomationService**: Operações Git (clone, pull, branch)
- **PromptGeneratorService**: Geração de prompts com templates
- **VisualStudioLauncher**: Integração com IDEs
- **ProcessHelper**: Execução de comandos do sistema

### Fluxo de Execução

```
Usuário ? MainWindow ? GitAutomationService ? ProcessHelper
                    ?
              PromptGeneratorService ? Arquivo .md
                    ?
              VisualStudioLauncher ? IDE
```

---

## ?? Troubleshooting

### Git não encontrado
**Solução**: Verifique se Git está no PATH: `git --version`

### Visual Studio não abre
**Solução**: Confirme que existe arquivo .sln no projeto

### Campos de coluna não aparecem
**Solução**: Marque "Habilitar Gerador de Prompt" e role para baixo

### Erro de validação
**Solução**: Verifique se todos os campos obrigatórios estão preenchidos
- Para CORE: Nome da Tabela é obrigatório
- Pelo menos uma coluna deve ser adicionada

---

## ?? Dicas de Uso

### Boas Práticas

1. **Nomenclatura**
   - Controllers: `PascalCase` (ex: ProviderRegister)
   - Endpoints: Singular, `PascalCase` (ex: City)
   - Métodos: Verbo + Nome (ex: GetCity)
   - Tabelas: Prefixo TB_ (ex: TB_PROVIDER_CITY)

2. **Colunas**
   ```
   ? Bom: ID_CIDADE:int; NOME_CIDADE:string
   ? Ruim: id:int; nome:string
   ```

3. **Feature Branches**
   - Sempre trabalhe a partir de uma feature branch atualizada
   - Use nomenclatura clara para WITs

---

## ?? Changelog

### Versão 2.0.0 (Atual)

#### ? Novas Funcionalidades
- Gerador de Prompt com templates CORE e BFF
- Campos dinâmicos para colunas
- Sistema de adicionar/remover colunas
- Campo de Nome da Tabela (CORE)
- Validação condicional por template

#### ?? Melhorias
- UI/UX completamente redesenhada
- Campos organizados lado a lado
- ComboBox para tipos de dados
- Visibilidade condicional de campos
- Logs mais detalhados

#### ??? Removido
- Prompt manual (substituído por gerador)
- CopilotChatService (funcionalidade integrada)

### Versão 1.0.0
- Clone de repositórios
- Pull e Fetch
- Gerenciamento de branches
- Integração com Visual Studio

---

## ?? Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## ?? Licença

Este projeto é de uso interno da CarePlus.

---

## ?? Equipe

- **Desenvolvimento**: Erick Paes
- **Organização**: CarePlus

---

## ?? Links

- **Repositório**: https://github.com/erickpaescareplus/AutomacaoGIT
- **Issues**: https://github.com/erickpaescareplus/AutomacaoGIT/issues

---

<div align="center">

**MUFASA** - Automatizando o Desenvolvimento com Inteligência

?? Desenvolvido com ?? pela equipe CarePlus

</div>
