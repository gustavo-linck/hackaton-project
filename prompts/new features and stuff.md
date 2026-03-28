# Prompt de Melhorias — Temas, Templates, Fluxo e Visual

## Contexto

Este documento descreve um conjunto de melhorias voltadas à experiência visual, personalização, onboarding e refinamento da interface da plataforma. As tarefas envolvem desde novos recursos de customização até ajustes de UX em telas existentes.

---

## 1. Novos Temas de Personalização

Expandir o catálogo de temas disponíveis na tela de personalização da página.

**Diretrizes:**
- Adicionar ao menos **8 novos temas** variados, contemplando estilos como: minimalista, neon/dark, pastel, terra/orgânico, monocromático, retro, glassmorphism e high-contrast
- Cada tema deve definir um conjunto completo de tokens: cor de fundo, cor de texto, cor de destaque, cor de botões, estilo de borda e família tipográfica (quando aplicável)
- Os temas devem ser exibidos na interface como **cards de preview** clicáveis, mostrando uma miniatura visual de como a página ficará
- O tema selecionado deve refletir imediatamente no preview em tempo real

---

## 2. Gradiente Customizável

Substituir o gradiente fixo atual por um editor de gradiente flexível.

**Comportamento esperado:**
- Permitir que o usuário escolha **duas ou mais cores** para compor o gradiente
- Oferecer opções de **direção** (horizontal, vertical, diagonal, radial)
- Exibir **presets de gradiente** como ponto de partida (ex: pôr do sol, oceano, aurora, etc.), mas permitindo edição livre a partir deles
- O gradiente customizado deve ser aplicado ao fundo da página e refletido em tempo real no preview
- Armazenar o gradiente como configuração da página (ex: array de cores + direção + tipo)

---

## 3. Sistema de Templates

Criar um sistema de templates que sirva de ponto de partida e inspiração para o usuário.

**Estrutura de um template:**
- Nome e descrição curta
- Thumbnail de preview
- Configuração completa pré-definida: tema, gradiente, seções ativas, textos de exemplo, disposição dos blocos

**Templates sugeridos para a versão inicial (mínimo 6):**
- **Creator** — voltado para influenciadores, com foto de destaque, links de redes sociais e botão de contato
- **Dev / Portfolio** — para desenvolvedores, com links para GitHub, projetos e blog
- **Business** — para pequenos negócios, com nome, descrição, endereço e link para WhatsApp
- **Artista** — galeria visual, link para portfólio e redes
- **Músico** — links para plataformas de streaming, agenda de shows e contato
- **Minimalista** — apenas nome, bio curta e links essenciais, visual limpo

**Diretrizes técnicas:**
- Templates armazenados como objetos de configuração (JSON) no sistema, não como páginas reais
- Ao aplicar um template, clonar a configuração para a página do usuário e permitir edição livre a partir daí

---

## 4. Escolha de Template na Criação de Nova Página

Ao iniciar a criação de uma nova página, apresentar os templates disponíveis para que o usuário não comece do zero.

**Fluxo esperado:**
1. Usuário clica em "Nova página"
2. Abre uma tela/modal de seleção de template com os cards disponíveis
3. Cada card exibe: nome, thumbnail e uma breve descrição do perfil de uso
4. Usuário pode selecionar um template ou optar por "Começar em branco"
5. Ao confirmar, a página é criada com a configuração do template aplicada e o usuário é direcionado para a tela de edição

**Requisitos:**
- A tela de seleção deve ter busca ou filtro por categoria (ex: criador, negócio, portfólio)
- O template selecionado deve ter um estado de destaque visual claro (borda, check, escala)
- Deve ser possível visualizar o template em tamanho maior antes de confirmar (ex: clique no card abre preview expandido)

---

## 5. Correção do Accordion na Tela de Edição

O accordion na tela de edição não gera overflow corretamente ao ser expandido, cortando o conteúdo.

**Solução esperada:**
- O painel esquerdo (edição) deve ter `overflow-y: auto` e crescer verticalmente conforme os accordions são abertos, sem cortar conteúdo
- O painel direito (preview em tempo real) deve ser **completamente independente do scroll** do painel esquerdo:
  - Usar `position: sticky` com `top: 0` ou layout com `height: 100vh` + `overflow: hidden` no painel direito
  - O preview não deve se mover, independentemente de quanto o usuário role o painel de edição
- Testar com todos os accordions abertos simultaneamente para garantir que nenhuma informação fique inacessível
- Garantir que o comportamento se mantenha correto em diferentes resoluções de tela

---

## 6. Melhoria do Fluxo de Criação de Conta e de Página

Redesenhar o fluxo de criação de conta e de primeira página para ser mais guiado, moderno e animado.

**Princípios do novo fluxo:**
- **Progressivo:** dividir em etapas curtas, sem sobrecarregar o usuário com muitos campos de uma vez
- **Contextualizado:** em cada etapa, mostrar uma prévia ou ilustração do que está sendo configurado
- **Animado:** usar transições suaves entre etapas (ex: slide, fade, scale — nada abrupto)
- **Orientado ao template:** assim que a conta for criada, conduzir o usuário diretamente para a escolha de um template e criação da primeira página

**Etapas sugeridas do onboarding:**
1. **Criar conta** — nome, e-mail, senha (ou OAuth) — layout limpo, uma coisa por vez
2. **Qual é o seu perfil?** — seleção visual de perfil (creator, negócio, dev, artista, etc.) para personalizar a sugestão de templates
3. **Escolha um template** — tela de seleção descrita no item 4
4. **Dê um nome à sua página** — campo de slug + preview do link
5. **Pronto!** — tela de sucesso animada com CTA para abrir o editor

**Requisitos técnicos:**
- Barra de progresso ou indicador de etapa visível durante todo o fluxo
- Possibilidade de voltar para a etapa anterior sem perder dados
- Animações implementadas com Framer Motion (ou equivalente já utilizado no projeto)
- Fluxo funcional tanto em desktop quanto em mobile

---

## 7. Suavização da Tela de Planos

A seleção de periodicidade (mensal/anual) na tela de planos está visualmente agressiva e quadrada. Refinar o componente.

**Ajustes esperados:**
- O toggle/switch de periodicidade deve ter **bordas arredondadas** (`border-radius` generoso, estilo pill)
- A animação de troca entre mensal e anual deve ser **suave e fluida** (ex: transição com `transition: all 0.3s ease`, sem saltos bruscos)
- O indicador de seleção ativo (background deslizante) deve usar uma animação de `transform: translateX` em vez de troca abrupta de classes
- Revisar os cards de plano: arredondar cantos, suavizar sombras e refinar o estado de hover e selecionado
- A mudança de preço exibida ao trocar a periodicidade deve ter uma animação de fade ou counter suave, não uma troca instantânea de texto

---

## 8. Novo Efeito de Fundo na Home

Substituir o efeito de fundo atual da home por um **gradiente interativo que responde ao movimento do mouse**.

**Comportamento esperado:**
- O gradiente de fundo se move e se reposiciona suavemente conforme o cursor do usuário se move pela tela
- Implementado via `mousemove` event listener: capturar posição X e Y do mouse e usar para ajustar os pontos de origem do gradiente (ex: `radial-gradient` com posição dinâmica) ou rotacionar/transladar um gradiente de fundo
- O movimento deve ser **fluido e levemente atrasado** em relação ao cursor (usar lerp — linear interpolation — para suavizar o tracking)
- Em dispositivos touch/mobile, usar um efeito estático ou uma animação automática suave em loop (ex: keyframe de rotação lenta do gradiente), já que não há `hover`
- O efeito não deve impactar a performance: usar `requestAnimationFrame`, evitar recálculos desnecessários e não bloquear a thread principal

**Referências de implementação:**
- Técnica de `radial-gradient` com `background-position` dinâmico via CSS custom properties atualizadas por JS
- Ou uso de `mesh gradient` animado via WebGL/canvas de forma leve (apenas se já houver dependência equivalente no projeto)

---

## Ordem de Prioridade Sugerida

| # | Tarefa | Impacto UX | Complexidade |
|---|--------|------------|-------------|
| 1 | Correção do accordion + scroll da edição | Alto | Baixo |
| 2 | Templates + seleção na criação de página | Alto | Alto |
| 3 | Fluxo de onboarding guiado e animado | Alto | Alto |
| 4 | Novos temas de personalização | Médio | Médio |
| 5 | Gradiente customizável | Médio | Médio |
| 6 | Suavização da tela de planos | Médio | Baixo |
| 7 | Efeito de fundo interativo na home | Médio | Médio |