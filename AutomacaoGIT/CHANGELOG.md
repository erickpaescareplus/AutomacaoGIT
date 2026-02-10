# ?? Changelog - Automação Git

## ?? Versão 2.0 - Melhorias de UI/UX e Novas Funcionalidades

### ? Novas Funcionalidades

#### ?? Suporte ao Projeto WEB
- **Adicionado**: Novo projeto "WEB - Credenciamento Prestador Cadastro Web"
- **URL**: `https://github.com/CareplusBR/credenciamento-prestador-cadastro-web`
- **Branches Feature**: 12 branches disponíveis para o projeto WEB
  - feature-B239447_PCadOrdemRedeCredenciada_Beta
  - feature-B253650_PreCad_Corpo_Clinico
  - feature-B218164_Cadastro_Prestador_Inclusao
  - feature-B253647_PreCad_CHs
  - feature-B239441_Historico_Prestadores_Beta
  - feature-B253637_PreCad_Dados_Gerais
  - feature-B253645_PreCad_Especialidades
  - feature-B252854_PreCad_Controle_Docs
  - feature-B253657_PreCad_Dados_De_Pagamnto
  - feature-B252858_PreCad_Qualificacoes
  - feature-B252857_PreCad_Divulgacao_Atendimento
  - feature-B252856_PreCad_Negociacao_Mat_Med

#### ?? Escolha de Editor
- **Adicionado**: Opção para escolher entre Visual Studio e Visual Studio Code
- **Radio Buttons**: Interface intuitiva para seleção do editor preferido
- **Visual Studio**: Abre arquivos .sln automaticamente (padrão)
- **VS Code**: Abre o diretório do projeto diretamente no VS Code
- **Requisito**: VS Code deve estar instalado e no PATH do sistema

### ?? Melhorias de Design e UI/UX

#### Visual Modernizado
- ? **Header personalizado** com gradiente e ícone temático
- ? **Cards brancos** com sombras suaves para profundidade
- ? **Layout de 2 colunas** no formulário para melhor aproveitamento do espaço
- ? **Ícones temáticos** para cada campo (??, ??, ??, ??, ???, ??, ??)
- ? **Background suave** (#F5F7FA) para não cansar a visão
- ? **Bordas arredondadas** em todos os elementos

#### Sistema de Cores Profissional
- ?? **Verde (#27AE60)** - Botão executar (ação principal)
- ?? **Vermelho (#E74C3C)** - Botão cancelar
- ? **Cinza (#95A5A6)** - Botão limpar logs
- ?? **Azul (#3498DB)** - Botão procurar pasta

#### Interatividade Aprimorada
- ? **Efeitos hover** em todos os botões com transição de cor
- ? **Efeitos de foco** nos campos de texto com bordas destacadas
- ? **Estados visuais** claros (enabled/disabled, pressed, hover)
- ? **Cursor hand** nos botões para indicar clicabilidade
- ? **Tooltips** informativos nos campos que necessitam

#### Área de Logs Melhorada
- ?? **Header destacado** com background escuro
- ?? **Contraste aprimorado** para melhor leitura
- ?? **Fonte monoespaçada** (Consolas) para logs técnicos
- ?? **Auto-scroll** para o final quando novos logs aparecem

### ?? Correções de Bugs

#### NullReferenceException no Carregamento
- **Problema**: Erro ao inicializar a janela devido a controles não carregados
- **Causa**: Evento `SelectionChanged` disparado durante `InitializeComponent()`
- **Solução**: Adicionada verificação de null para `TxtRepositoryUrl` e `CmbFeatureBranch`
- **Resultado**: Aplicação inicia sem erros

### ?? Melhorias Técnicas

#### Código
- ? Refatoração da lógica de detecção de projeto (CORE, BFF, WEB)
- ? Método assíncrono para abrir VS Code
- ? Tratamento de exceções melhorado
- ? Logs mais descritivos com emojis

#### Organização
- ? Espaçamento consistente entre elementos
- ? Tamanhos de fonte hierarquizados
- ? Padding e margins proporcionais
- ? Alinhamento perfeito de todos os elementos

### ?? Como Usar as Novas Funcionalidades

#### Selecionar Projeto WEB
1. Abra a aplicação
2. No campo "?? Projeto", selecione "WEB - Credenciamento Prestador Cadastro Web"
3. As branches feature serão carregadas automaticamente
4. Continue o processo normalmente

#### Escolher Editor
1. No campo "?? Editor para Abrir", selecione sua preferência:
   - **Visual Studio** (padrão): Ideal para projetos .NET
   - **Visual Studio Code**: Ideal para projetos web, mais leve
2. Após a execução bem-sucedida, o editor escolhido será aberto automaticamente

### ?? Requisitos

- **.NET 8.0** ou superior
- **Git** instalado e configurado
- **Visual Studio** (para opção VS) ou **Visual Studio Code** (para opção VS Code)
- Para VS Code: comando `code` deve estar no PATH do sistema

### ?? Dicas

- Use **Visual Studio** para projetos CORE e BFF (.NET)
- Use **Visual Studio Code** para projeto WEB (front-end)
- Verifique os logs para acompanhar o progresso da automação
- Utilize tooltips para obter informações sobre os campos

---

**Desenvolvido com ?? para agilizar o desenvolvimento**
