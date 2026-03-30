# 🤖 Prompt de Execução — Claude Code CLI

> Execute este arquivo com: `claude < PROMPT_CLAUDE_CODE.md` ou cole o conteúdo direto no CLI.

---

## Contexto Geral

Você está trabalhando em um projeto web que precisa de uma série de melhorias e correções antes de ser entregue hoje. Analise **todo o projeto** antes de começar — estrutura de pastas, arquivos de configuração, componentes, rotas, banco de dados, etc. Não pule etapas. Gaste os tokens necessários para entregar tudo funcionando.

---

## Tarefas Obrigatórias

### 1. 🚫 Remover Menções de Domínio Próprio

- Faça uma busca em **todos os arquivos** do projeto por referências a domínio próprio (ex: campos, labels, inputs, configurações, textos de UI, tooltips, placeholders, comentários, variáveis de ambiente e qualquer lógica) relacionadas à funcionalidade de "domínio próprio" ou "custom domain".
- **Remova completamente** qualquer tela, seção, campo, rota, componente ou lógica relacionada a essa funcionalidade — pois ela não existe no sistema.
- Garanta que não fique nenhum link quebrado, erro de rota ou referência morta após a remoção.
- Se houver menu ou navegação apontando para essa funcionalidade, remova o item do menu também.

---

### 2. 🎨 Melhorar o Dashboard (Tema Dark)

- O fundo do dashboard no tema escuro (dark mode) está com visual enjoativo — cores planas, sem profundidade, hover sem feedback visual adequado.
- Melhore o visual do dashboard no tema dark com:
  - Gradientes sutis ou variações de tom no fundo dos cards/painéis.
  - Estados de hover com transições suaves (ex: `transition`, leve elevação com `box-shadow`, mudança de borda ou brilho sutil).
  - Certifique-se de que as cores de hover não sejam idênticas ao fundo — crie contraste suficiente para dar sensação de interatividade.
  - Mantenha consistência com o tema claro se existir.
  - Não altere a lógica ou os dados exibidos, apenas o visual.

---

### 3. 📸 Upload de Foto no Avatar (Página de Edição de Perfil)

- Na página de edição de perfil/conta do usuário, **falta a funcionalidade de upload de foto de avatar**.
- Implemente:
  - Um clique no avatar atual que abre o seletor de arquivo (`input[type="file"]`).
  - Aceite apenas imagens (`image/*`), preferencialmente com limite de tamanho (ex: 5MB).
  - Mostre preview da imagem selecionada antes de salvar.
  - Faça o upload para o backend (analise como o projeto já lida com uploads ou storage — S3, local, Supabase, etc. — e siga o mesmo padrão).
  - Salve a URL/referência no perfil do usuário no banco de dados.
  - Exiba a nova foto imediatamente após o salvamento, sem precisar recarregar a página.
  - Trate erros (arquivo muito grande, formato inválido, falha no upload).

---

### 4. 🛠️ Melhorar o Board de Admin

- O painel administrativo está com poucas funcionalidades. Analise o que já existe e adicione o que fizer sentido para o contexto do projeto, incluindo (mas não limitado a):
  - **Métricas e estatísticas**: total de usuários, usuários ativos, crescimento recente, etc.
  - **Tabela de usuários**: listagem com busca, filtros, paginação, e ações rápidas (ativar/desativar, ver perfil, etc.).
  - **Logs de atividade recente**: ações importantes realizadas no sistema.
  - **Gráficos simples**: use a biblioteca de gráficos já presente no projeto (ou instale uma leve como `recharts` ou `chart.js`) para mostrar tendências.
  - **Cards de resumo** no topo com números-chave.
  - **Ações rápidas de moderação** se aplicável ao contexto.
- Conecte tudo ao backend real — não use dados mockados/estáticos, a menos que seja absolutamente impossível no momento. Se usar mock, deixe um comentário claro `// TODO: conectar ao endpoint real`.

---

### 5. 📄 Criar README.md

- Crie (ou sobrescreva) o arquivo `README.md` na raiz do projeto.
- O README deve conter:
  - **Nome e descrição** do projeto.
  - **Propósito**: para que serve, quem usa.
  - **Funcionalidades principais**: descreva em linguagem natural o que o sistema faz — sem mostrar código.
  - **Tecnologias utilizadas**: liste as principais (framework, banco de dados, autenticação, etc.).
  - **Como rodar localmente**: passo a passo em texto (instalar dependências, configurar variáveis de ambiente, rodar o servidor).
  - **Estrutura geral do projeto**: explique as pastas principais em prosa, sem árvore de código.
  - **Funcionalidades do painel admin**: descreva o que o admin pode fazer.
  - **Observações e limitações conhecidas** se houver.
- Tom: claro, profissional, em português.

---

### 7. 💳 Validação de Cartão de Crédito

- Localize todos os formulários/telas onde o usuário informa dados de cartão de crédito.
- Implemente validação no **frontend** (antes de qualquer envio ao backend) para:
  - **Número do cartão**: aceitar apenas dígitos, aplicar máscara `XXXX XXXX XXXX XXXX`, validar com o algoritmo de Luhn para detectar números obviamente inválidos.
  - **Nome no cartão**: apenas letras e espaços, mínimo de 2 palavras.
  - **Data de validade**: formato `MM/AA`, verificar se o mês é entre 01–12, e se a data não está no passado.
  - **CVV/CVC**: apenas dígitos, 3 dígitos para a maioria dos cartões (4 para Amex se aplicável).
  - Detectar e exibir a **bandeira do cartão** (Visa, Mastercard, Elo, etc.) com base nos primeiros dígitos, se possível.
- Exiba mensagens de erro **inline** em cada campo, no momento em que o usuário sair do campo (blur) ou tentar submeter.
- **Bloqueie o envio do formulário** enquanto qualquer campo for inválido.
- Nunca armazene o CVV — se o projeto salvar dados de cartão, garanta que o CVV não persiste em nenhum lugar (log, banco, estado global).
- Use uma biblioteca já presente no projeto se houver (ex: `react-hook-form`, `yup`, `zod`). Caso não haja, implemente a validação de forma simples e direta sem adicionar dependências desnecessárias.

---

### 8. ⚙️ Configurações do Usuário — Restrições e Campos Bloqueados

- Na página de configurações/perfil do usuário, identifique campos que **não devem ser editáveis** e trate isso corretamente na interface:
  - **E-mail**: não deve ser alterável diretamente pelo usuário (questão de segurança e integridade de autenticação). O campo deve estar visível, mas bloqueado (`disabled` ou `readOnly`), com uma explicação clara do motivo (ex: *"Para alterar seu e-mail, entre em contato com o suporte."* ou *"O e-mail não pode ser alterado após o cadastro."*).
  - Verifique se há outros campos na mesma situação (ex: CPF, data de criação da conta, ID de usuário) e aplique o mesmo padrão de exibição bloqueada.
- Garanta que, mesmo que alguém tente forçar a edição via DevTools ou requisição direta, o **backend rejeite** alterações nesses campos — adicione a proteção na camada de API/serviço se ainda não existir.
- Se o projeto tiver autenticação via provedor externo (Google, GitHub, etc.), exiba uma mensagem informando que o e-mail é gerenciado pelo provedor e não pode ser alterado aqui.
- Para os campos **que podem ser editados**, garanta que:
  - Haja validação adequada (nome não vazio, telefone com formato correto, etc.).
  - Exista feedback visual de sucesso após salvar (toast, mensagem inline, etc.).
  - O botão de salvar só fique ativo se houver alguma alteração real nos dados.

---

### 6. 🔍 Análise Completa do Projeto + Melhorias Gerais

Esta é a tarefa mais importante e abrangente. Execute com atenção máxima:

#### 6.1 — Análise Estrutural
- Leia todos os arquivos relevantes: rotas, componentes, hooks, serviços, modelos de dados, configurações.
- Identifique problemas reais: erros, inconsistências, funcionalidades quebradas, imports faltando, variáveis não usadas, rotas sem tratamento de erro, etc.

#### 6.2 — Correções de Bugs
- Corrija **qualquer bug encontrado** — não apenas liste, resolva.
- Priorize problemas que causam crashes, telas em branco ou comportamento inesperado.

#### 6.3 — UX e Interface
- Identifique telas ou fluxos confusos, botões sem feedback, formulários sem validação, estados de loading ausentes.
- Adicione loading states, mensagens de erro amigáveis, toasts de sucesso onde faltar.
- Garanta que formulários tenham validação básica no frontend.

#### 6.4 — Segurança Básica
- Verifique se rotas protegidas realmente verificam autenticação.
- Certifique-se que dados sensíveis não estão expostos no frontend.
- Se houver inputs sem sanitização óbvia, corrija.

#### 6.5 — Performance Simples
- Remova imports não utilizados.
- Evite re-renders desnecessários óbvios (ex: funções criadas dentro do render sem `useCallback`/`useMemo` quando impactam listas grandes).
- Adicione lazy loading em rotas se o projeto usar React e ainda não tiver.

#### 6.6 — Consistência Visual
- Padronize espaçamentos, tamanhos de fonte e cores que estejam inconsistentes.
- Garanta que componentes similares tenham aparência similar.

---

## Regras de Execução

- **Analise antes de agir**: leia o projeto completo antes de começar qualquer modificação.
- **Não quebre o que funciona**: teste mentalmente o impacto de cada mudança.
- **Siga os padrões existentes**: use as mesmas bibliotecas, convenções de nomenclatura e estrutura de pastas já presentes no projeto.
- **Commits atômicos** se usar git: um commit por tarefa principal.
- **Não deixe TODO sem resolver** a menos que seja genuinamente impossível agora — e nesse caso, deixe um comentário explicativo.
- **Entrega funcionando**: ao final, o projeto deve rodar sem erros de console críticos e todas as funcionalidades listadas devem estar operacionais.

---

## Ordem de Execução Sugerida

1. Análise completa do projeto (Tarefa 6.1)
2. Remoção de domínio próprio (Tarefa 1)
3. Correções de bugs encontrados (Tarefa 6.2)
4. Upload de avatar (Tarefa 3)
5. Configurações do usuário — campos bloqueados (Tarefa 8)
6. Validação de cartão de crédito (Tarefa 7)
7. Melhorias no Admin Board (Tarefa 4)
8. Melhorias visuais do Dashboard dark (Tarefa 2)
9. Melhorias de UX/segurança/performance (Tarefas 6.3–6.6)
10. Criação do README.md (Tarefa 5)

---