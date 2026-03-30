# 🤖 Prompt de Execução — Melhorias Visuais e Funcionais

> Execute na raiz do projeto com o Claude Code CLI.

---

## Antes de Começar — Reescreva Este Prompt

**Antes de executar qualquer tarefa**, leia todo este prompt e o reescreva com suas próprias palavras, da forma que você achar melhor para entender e executar o que foi pedido. Reorganize, detalhe, simplifique ou expanda o que precisar — o objetivo é que você internalize as tarefas no formato que te permita trabalhar com mais precisão e qualidade. Só comece a executar após ter feito essa reescrita.

---

## Contexto Geral

Projeto web que precisa de melhorias visuais e funcionais urgentes para entrega hoje. Analise **todo o projeto completo** antes de começar — leia todos os componentes, rotas, estilos, hooks e configurações. Não pule etapas. Gaste os tokens necessários para entregar com qualidade.

---

## Tarefas Obrigatórias

### 1. 🎨 Melhorar o Dashboard no Tema Dark

O fundo do dashboard no dark mode está visual e interativamente fraco. Melhore:

- **Fundo e cards**: adicione gradientes sutis ou variações de tom entre os elementos — evite superfícies completamente planas e monocromáticas.
- **Estados de hover**: implemente transições suaves (`transition`) com feedback visual claro — leve elevação via `box-shadow`, variação de cor de borda, ou brilho/glow sutil. O hover não pode ser igual ao estado padrão.
- **Profundidade visual**: use camadas de cor (ex: fundo da página levemente diferente do fundo dos cards, que é diferente do fundo dos itens internos) para criar hierarquia visual.
- **Consistência**: todas as mudanças devem funcionar bem com o tema claro, sem quebrá-lo.
- Não altere dados, lógica ou estrutura — apenas o visual.

---

### 2. 🛠️ Adicionar Controle de Trial no Board de Admin

No painel administrativo, adicione a funcionalidade de ativar ou desativar o **trial/promoção** para cada cliente:

- Analise como o projeto modela usuários/clientes e planos — entenda onde e como o trial é (ou deveria ser) armazenado antes de implementar.
- Na listagem ou na página de detalhes do cliente dentro do admin, adicione um controle claro (toggle, botão ou switch) para ativar ou desativar o trial daquele cliente.
- Ao ativar, deve ser possível definir a duração do trial se fizer sentido no contexto (ex: número de dias), ou aplicar um período padrão que já exista no sistema.
- O estado atual do trial do cliente deve estar visível na interface — se está ativo, quando expira, ou se está desativado.
- A ação deve persistir no backend — crie ou ajuste o endpoint necessário para salvar essa informação.
- Adicione confirmação antes de aplicar a ação (ex: modal ou dialog de confirmação) para evitar cliques acidentais.
- Exiba feedback visual de sucesso ou erro após a operação (toast ou mensagem inline).
- Garanta que a lógica de trial no resto do sistema (ex: verificação de acesso do usuário) respeite o valor salvo pelo admin.

---

### 3. 💀 Skeletons de Carregamento

O projeto não possui telas de skeleton durante o carregamento de dados. Implemente skeletons em todas as telas e componentes que fazem requisições assíncronas:

- **Mapeie todos os pontos de loading** do projeto: listas, tabelas, cards, dashboards, páginas de perfil, detalhes — qualquer lugar que aguarde uma resposta do backend antes de renderizar.
- **Crie componentes de skeleton** que espelhem fielmente o layout do conteúdo real — mesmas proporções de blocos, colunas, linhas e espaçamentos. O skeleton deve parecer o "fantasma" do conteúdo que vai aparecer.
- **Animação lenta e natural**: use `animation-duration` entre **1.8s e 2.5s** no efeito de shimmer/pulse. Animações rápidas (abaixo de 1s) parecem nervosas — o objetivo é um brilho suave que vai e volta devagar, dando sensação de algo carregando organicamente.
- Implemente o efeito com CSS puro (`@keyframes` + `background: linear-gradient` em movimento) ou use a classe de skeleton já existente no projeto se houver (ex: Tailwind `animate-pulse`). Se usar `animate-pulse` do Tailwind, sobrescreva a duração padrão para algo mais lento via classe customizada.
- **Substitua spinners soltos** por skeletons onde o layout do conteúdo for previsível — spinner só deve ser usado para ações pontuais (ex: botão salvando), não para carregamento de telas inteiras.
- O skeleton deve desaparecer suavemente quando o conteúdo real chegar — use transição de opacidade (`opacity: 0` → `opacity: 1`) no conteúdo real ao montar, evitando troca brusca.
- Garanta que os skeletons funcionem bem nos temas claro e escuro — a cor base e o brilho do shimmer devem ser diferentes para cada tema.

---

### 4. 🔍 Análise Completa do Projeto + Melhorias Gerais

Esta é a tarefa mais ampla. Execute com atenção máxima e corrija tudo que encontrar.

#### 3.1 — Levantamento Inicial
- Leia todos os arquivos relevantes: componentes, rotas, hooks, serviços, modelos, configurações, estilos.
- Mapeie problemas: bugs, imports quebrados, rotas sem proteção, componentes inconsistentes, estados sem tratamento, etc.

#### 3.2 — Bugs e Crashes
- Corrija **todos os bugs encontrados** — não apenas liste, resolva.
- Priorize o que causa telas em branco, erros de console ou comportamento inesperado.

#### 3.3 — Problemas de Layout e Tipografia (PRIORIDADE ALTA)

Há problemas conhecidos de layout em modais. Aplique as correções abaixo e verifique se o mesmo padrão está quebrado em outros lugares:

- **Modal de informações de cartão de crédito**: o título da modal não está se comportando como título — ele aparece junto ou mal posicionado em relação ao campo de texto abaixo. Corrija para que o título fique claramente acima do conteúdo, com espaçamento e hierarquia tipográfica adequados (tamanho, peso, margem inferior).
- **Modal de login**: mesma situação — o título deve estar separado visualmente do formulário abaixo, com espaçamento correto e comportamento de heading real.
- Após corrigir esses dois, **varrer todas as outras modais e drawers** do projeto e aplicar o mesmo padrão consistente: título sempre acima, espaçamento adequado, hierarquia clara.
- Regra geral: títulos de modais devem ter `font-size` e `font-weight` maiores que o conteúdo, `margin-bottom` suficiente para separar do primeiro campo/parágrafo, e nunca estar colados ao conteúdo abaixo.

#### 3.4 — UX e Feedback Visual
- Adicione loading states onde estiverem faltando (botões que disparam requests, tabelas carregando, etc.).
- Adicione toasts/notificações de sucesso e erro onde o sistema já não tiver.
- Formulários sem validação: adicione ao menos validação básica de campos obrigatórios e formatos.
- Botões que não dão feedback ao usuário: desabilite durante o loading, mude o texto/ícone.

#### 3.5 — Segurança Básica
- Rotas protegidas que não verificam autenticação: corrija.
- Dados sensíveis expostos no frontend: remova.
- Inputs sem tratamento básico: sanitize onde óbvio.

#### 3.6 — Performance e Código
- Remova imports não utilizados.
- Corrija re-renders desnecessários óbvios em listas e componentes pesados.
- Adicione lazy loading nas rotas se o projeto usar React e ainda não tiver.

#### 3.7 — Consistência Visual Geral
- Padronize espaçamentos, tamanhos de fonte e cores que estejam inconsistentes entre telas similares.
- Componentes do mesmo tipo (cards, tabelas, badges) devem ter aparência uniforme em todo o projeto.

---

## Regras de Execução

- **Leia antes de agir**: analise o projeto completo antes de qualquer modificação.
- **Não quebre o que funciona**: avalie o impacto de cada mudança antes de aplicar.
- **Siga os padrões do projeto**: mesmas bibliotecas, convenções de nome, estrutura de pastas.
- **Commits atômicos** se usar git: um commit por tarefa principal.
- **Zero TODOs sem justificativa**: se não for possível resolver algo agora, deixe um comentário explicando o motivo.
- **Entrega funcionando**: ao final, sem erros críticos de console e todas as funcionalidades operacionais.

---

## Ordem de Execução Sugerida

1. Análise completa do projeto (4.1)
2. Correção de bugs encontrados (4.2)
3. Correção de layout das modais — cartão e login (4.3) ← prioridade
4. Melhorias visuais do Dashboard dark (Tarefa 1)
5. Skeletons de carregamento (Tarefa 3)
6. Controle de trial no Admin Board (Tarefa 2)
7. UX, feedback e validações (4.4)
8. Segurança básica (4.5)
9. Performance e consistência visual (4.6 e 4.7)

---
