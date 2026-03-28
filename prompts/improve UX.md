# Prompt de Melhorias — Plataforma de Páginas

## Contexto

Este documento descreve um conjunto de melhorias a serem implementadas na plataforma. As tarefas estão organizadas por área de impacto. Implemente cada item com atenção aos detalhes de UX descritos.

---

## 1. Performance — Cache com Redis

Avalie os endpoints e queries mais frequentes da aplicação e implemente uma camada de cache utilizando **Redis**.

**Diretrizes:**
- Identifique as consultas ao banco de dados que se repetem com frequência e que retornam dados raramente alterados (ex: dados de página publicada, configurações do usuário, etc.)
- Defina TTLs (time-to-live) adequados para cada tipo de dado cacheado
- Garanta que o cache seja invalidado corretamente sempre que os dados forem atualizados (ex: ao salvar uma página, limpar o cache correspondente)
- Não aplique cache em dados sensíveis ou que exijam consistência em tempo real

---

## 2. Tela de Edição de Página — Layout e Scroll

A tela de edição apresenta dois problemas de layout que precisam ser corrigidos.

### 2.1 Ausência de scroll vertical

Atualmente a tela não possui scroll, fazendo com que informações fora da viewport fiquem inacessíveis.

**Solução esperada:**
- Adicionar scroll **apenas no painel esquerdo** (formulário/campos de edição)
- O painel direito (preview em tempo real) deve permanecer **fixo**, sem scroll próprio, ocupando sempre a altura visível da tela
- O layout deve ser estruturado com `height: 100vh` no container principal, e `overflow-y: auto` restrito à coluna esquerda

### 2.2 Preview em tempo real no Mobile

No mobile, o preview em tempo real não é exibido, deixando o usuário sem feedback visual durante a edição.

**Solução esperada:**
- Adicionar um **botão flutuante fixo** na tela (ex: canto inferior direito), visível apenas em viewports mobile
- Ao clicar no botão, abrir uma **modal em tela cheia ou drawer** exibindo o preview da página no estado atual
- O usuário pode fechar a modal, realizar ajustes nos campos e reabri-la para conferir o resultado
- O botão deve seguir o scroll do usuário (posição `fixed`)
- Sugestão de ícone: olho (`eye`) ou dispositivo móvel, com label "Visualizar"

---

## 3. Customização de Link da Página

Remover a lógica de domínio próprio por usuário. No lugar, permitir que o usuário defina o **slug personalizado** da sua página.

**Comportamento esperado:**
- O link final da página seguirá o formato: `umblink.com/p/{slug-escolhido}`
- Na tela de edição/configuração da página, exibir um campo de texto para o usuário digitar o slug desejado
- Validações necessárias:
  - Apenas caracteres permitidos em URLs (letras, números, hífens)
  - Slug único — verificar disponibilidade em tempo real (ex: debounce + consulta à API)
  - Comprimento mínimo e máximo definidos (sugestão: 3–60 caracteres)
- Exibir preview do link completo abaixo do campo à medida que o usuário digita
- Em caso de slug já ocupado, exibir mensagem clara de indisponibilidade

---

## 4. Correções de Tema Escuro (Dark Mode)

Existem inconsistências visuais no tema escuro em diversas telas da plataforma. Revisar e corrigir os seguintes pontos:

**Itens identificados:**
- **Banner/mensagem de trial:** a cor de fundo ou texto está muito clara em relação ao restante do layout no tema escuro — ajustar para manter contraste e coerência visual
- **Textos com cor escura sobre fundo escuro:** identificar todos os elementos que utilizam cores de texto fixas (hardcoded, ex: `text-gray-900`, `color: #111`) e substituir por tokens/variáveis que respeitem o tema ativo
- Realizar uma varredura em **todas as páginas** da aplicação no tema escuro, corrigindo qualquer elemento com contraste insuficiente ou cor incompatível

**Critério de aceitação:**
- Nenhum texto deve ser ilegível no tema escuro
- Todos os componentes devem utilizar variáveis de cor do tema (ex: via CSS variables, Tailwind dark mode classes ou equivalente do design system adotado)

---

## 5. Responsividade Mobile — Revisão Geral

O layout mobile está quebrando em diversas telas. Fazer uma revisão completa da versão mobile da plataforma.

**Escopo da revisão:**
- Percorrer **todas as telas** da aplicação em viewport mobile (sugestão: testar em 375px e 390px de largura)
- Identificar e corrigir: elementos cortados, overflow horizontal indesejado, textos sobrepostos, botões inacessíveis, formulários mal dimensionados e modais que ultrapassam a tela
- Garantir que todas as interações principais sejam acessíveis por toque
- Revisar espaçamentos, tamanhos de fonte e hierarquia visual para telas pequenas
- Após correções, validar também em tablets (768px) para evitar regressões

---

## Ordem de Prioridade Sugerida

| # | Tarefa | Impacto | Complexidade |
|---|--------|---------|-------------|
| 1 | Responsividade mobile geral | Alto | Médio |
| 2 | Scroll e preview mobile na edição | Alto | Médio |
| 3 | Correções de tema escuro | Médio | Baixo |
| 4 | Customização de slug do link | Médio | Médio |
| 5 | Cache com Redis | Médio | Alto |