# UMBLINK — Sprint de Qualidade: UI, Planos, Fluxos, Editor e Revisão Geral

---

## Visão do Produto

Antes de qualquer coisa, leia e internalize este briefing. Ele define o que o produto é, o que precisa entregar e como avaliar se está no caminho certo. Use como filtro para cada decisão técnica e de UX nessa sessão.

### O que é o Umblink
Um criador de páginas de links para bio de redes sociais. O usuário monta de forma simples e rápida uma página personalizada que reúne todos os seus links importantes — site, redes sociais, WhatsApp, portfólio, loja — em uma única URL compartilhável. A ferramenta deve oferecer um editor intuitivo com opções de personalização visual e gerar uma página leve e otimizada para mobile.

### Problema que resolve
Quem divulga seu trabalho nas redes sociais só pode colocar um único link na bio, o que força a escolher entre site, WhatsApp, portfólio ou loja — perdendo audiência para tudo que ficou de fora. O Umblink resolve isso reunindo tudo em uma única URL.

### Inspirações de mercado
- **Linktree** — referência global com mais de 70 milhões de usuários. Criação rápida, analytics de cliques, integrações e templates personalizáveis.
- **Hopp by Wix** — alternativa mais robusta: além de links, permite vender produtos, captar leads e agendar serviços, funcionando como uma mini-site completa.

### Objetivo estratégico (Umbler)
Oferecer mais uma ferramenta de valor dentro do ecossistema Umbler, aumentando a superfície de contato com potenciais clientes e criando um ponto de entrada simples que pode evoluir para a adoção de outros produtos da plataforma.

### Critérios de qualidade inegociáveis
1. **Simplicidade radical no onboarding** — qualquer pessoa, sem conhecimento técnico, deve conseguir publicar sua página em poucos minutos
2. **Visual profissional e moderno** — as páginas geradas devem reforçar a percepção de qualidade associada à marca Umbler
3. **Mobile-first** — a página pública é acessada principalmente via redes sociais no celular; performance e layout mobile são prioridade
4. **Métricas básicas** — o usuário precisa ver quantas visualizações e cliques seus links receberam
5. **Porta de entrada no ecossistema** — o produto deve ter conexão natural com outros serviços Umbler para facilitar upsell e cross-sell

---

## Contexto Técnico

Projeto **Umblink** (.NET 8 / Blazor Server, EF Core, SQLite, Railway). Leia `CLAUDE.md` e `STYLE_GUIDE.md` antes de qualquer alteração. Mapeie a estrutura completa de pastas antes de iniciar qualquer tarefa. Compile (`dotnet build`) após cada tarefa concluída e corrija qualquer erro antes de avançar.

> **Regra geral:** sem código comentado, sem mocks (exceto pagamento), sem `window.confirm` nativo, sem tela em branco onde deveria haver conteúdo.

---

## Tarefa 0 — Diagnóstico de Aderência ao Produto

### Objetivo
Antes de iniciar qualquer correção, fazer um diagnóstico completo do produto atual comparando o que está implementado com o que o briefing exige. O resultado deve ser um relatório claro do que está OK, o que está incompleto e o que está faltando.

### O que analisar

**1. Simplicidade do onboarding**
- Quantos passos são necessários desde o cadastro até a primeira página publicada?
- Existe alguma fricção desnecessária (campos obrigatórios que poderiam ser opcionais, etapas que poderiam ser puladas)?
- Um usuário sem conhecimento técnico conseguiria concluir sem ajuda?

**2. Editor e personalização**
- O editor oferece opções suficientes de personalização visual?
- O preview em tempo real está funcionando?
- É possível adicionar, reordenar e remover links de forma intuitiva?

**3. Performance e qualidade da página pública**
- A página pública (`/p/{slug}`) é leve? Há assets desnecessários sendo carregados?
- O layout é funcional e legível em mobile (375px)?
- As meta tags (title, description, og:image) estão presentes para compartilhamento?

**4. Métricas**
- O sistema registra visualizações da página pública?
- O sistema registra cliques em links individuais?
- Essas métricas são exibidas de forma clara no dashboard?

**5. Planos e monetização**
- A diferenciação entre Free e Pro está clara na UI?
- Os bloqueios de features Pro estão corretos e consistentes?
- O fluxo de upgrade é acessível e compreensível?

**6. Qualidade visual geral**
- As páginas geradas pelo produto passam a percepção de qualidade profissional?
- O produto em si (editor, dashboard, home) transmite confiança e modernidade?

### Como entregar o diagnóstico
Antes de escrever código, gere um arquivo `DIAGNOSTICO.md` na raiz do projeto com o seguinte formato para cada critério:

```markdown
## [Nome do critério]
**Status:** ✅ OK | ⚠️ Parcial | ❌ Faltando
**O que foi encontrado:** descrição objetiva do estado atual
**O que falta ou precisa melhorar:** lista de pontos
**Impacto:** Alto / Médio / Baixo
```

Após gerar o diagnóstico, use-o como guia complementar às tarefas abaixo — se o diagnóstico revelar problemas críticos não cobertos pelas tarefas seguintes, corrija-os também.

### Critério de aceitação
O arquivo `DIAGNOSTICO.md` existe na raiz e cobre todos os 6 critérios acima com status, achados e impacto.

---

## Tarefa 1 — Remover Domínio Próprio de Todas as Telas

### Objetivo
Remover qualquer menção, campo, opção ou UI relacionada a domínio próprio (custom domain) do produto, pois essa funcionalidade não existe.

### O que localizar e remover
1. Campos de input para configuração de domínio personalizado
2. Seções de configuração/settings que mencionem domínio próprio
3. Itens de menu, cards de feature ou badges que listem "domínio próprio" como benefício de algum plano
4. Textos em páginas de pricing, onboarding ou comparativo de planos que citem essa feature
5. Qualquer lógica de backend (serviços, entidades, migrations) relacionada exclusivamente a custom domain

### Como remover
- Remover completamente, sem comentar
- Verificar se a entidade/coluna no banco precisa de migration de remoção ou se pode ser ignorada com segurança
- Após remover da UI, confirmar que não há referências órfãs

### Critério de aceitação
Nenhuma tela, card, tooltip, texto ou campo menciona "domínio próprio", "custom domain" ou equivalente em qualquer idioma.

---

## Tarefa 2 — Dashboard: Uniformizar Tamanho dos Cards

### Objetivo
Todos os cards do dashboard devem ter exatamente o mesmo tamanho, independentemente do conteúdo interno.

### O que corrigir
1. Identificar o grid/layout dos cards no dashboard e garantir que usem altura fixa ou `align-items: stretch` no container
2. Cards de métricas (total de links, visualizações, etc.) devem ter a mesma altura e largura
3. O conteúdo interno de cada card (ícone, número, label) deve ser alinhado verticalmente de forma consistente
4. Verificar comportamento em diferentes resoluções — os cards devem continuar uniformes em mobile e desktop
5. Se algum card tem conteúdo variável (ex: texto longo), usar `text-overflow: ellipsis` ou truncar — nunca deixar o card crescer

### CSS esperado (aplicar no container de cards)
```css
.dashboard-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  align-items: stretch;
  gap: 1rem;
}

.dashboard-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 120px; /* ajustar ao design existente */
}
```

### Critério de aceitação
Todos os cards do dashboard têm altura e largura idênticas em qualquer resolução. Nenhum card cresce ou encolhe por causa do conteúdo.

---

## Tarefa 3 — Planos e Templates: Corrigir Lógica de Acesso por Tier

### Objetivo
Corrigir a lógica que determina quais templates e opções de estilo estão disponíveis para cada plano, e aplicar isso corretamente na UI do onboarding e do editor.

### Regras de acesso a definir e implementar

| Feature | Free | Pro+ |
|---|---|---|
| Templates básicos (cores sólidas, layouts simples) | ✅ | ✅ |
| Templates mais elaborados / diferenciados | ❌ | ✅ |
| Gradiente customizável como background | ❌ | ✅ |
| Foto de background via URL | ❌ | ✅ |
| Upload de arquivo como background | ❌ | ✅ |

> **Regra de ouro:** templates simples são para todos. Apenas os estilos que oferecem diferencial visual real (gradientes customizados, fotos de fundo, layouts premium) ficam bloqueados no Pro+.

### O que corrigir

**No onboarding (step de escolha de estilo):**
1. Mapear todos os templates existentes e classificar cada um como `Free` ou `Pro` no banco ou em um enum/constante centralizada
2. Templates Free devem ser selecionáveis sem bloqueio — remover qualquer lógica que marque templates simples como Pro
3. Templates Pro devem exibir um badge "Pro" e, ao tentar selecionar, mostrar um modal/tooltip explicando o benefício — não travar silenciosamente nem avançar com o template aplicado
4. A seleção de um template Pro não deve persistir nem avançar o wizard para um usuário Free
5. Gradiente customizável e foto de fundo via URL devem aparecer como opções bloqueadas (com badge "Pro+") para usuários Free

**No editor de página:**
1. As mesmas regras acima se aplicam ao editor — usuários Free não podem ativar gradiente custom nem foto de fundo
2. Ao tentar ativar uma opção Pro no editor, exibir um CTA de upgrade — não silenciar nem ignorar

**No backend:**
1. Validar o plano do usuário no servidor antes de persistir qualquer configuração de tema — não confiar apenas na UI
2. Se um usuário Free tentar salvar um tema Pro via requisição direta, retornar erro 403 com mensagem clara

### Critério de aceitação
Um usuário Free consegue selecionar e aplicar qualquer template básico sem bloqueio. Ao tentar selecionar template Pro, gradiente ou foto de fundo, vê um modal de upgrade. Nenhuma opção Pro persiste no banco para usuários Free.

---

## Tarefa 4 — Logout: Redirecionar para Home

### Objetivo
Ao deslogar, o usuário deve ser redirecionado para a página inicial (`/`) e não para a tela de login.

### O que corrigir
1. Localizar o método/ação de logout (provavelmente em `AccountController`, `AuthService` ou equivalente)
2. Alterar o redirect pós-logout de `/login` (ou equivalente) para `/`
3. Verificar se há middleware ou configuração do Identity que sobrescreve esse redirect e corrigir na origem
4. Testar os dois cenários: logout via botão de menu e expiração de sessão — ambos devem levar para `/`

### Critério de aceitação
Ao clicar em "Sair", o usuário é redirecionado para a home pública (`/`) com a sessão encerrada. Nenhum redirect intermediário para `/login` ocorre.

---

## Tarefa 5 — Editor: Foto de Background via URL

### Objetivo
Permitir que usuários Pro+ configurem uma URL de imagem como background da sua página pública.

### O que implementar

**UI (Editor de página):**
1. Na seção de personalização de fundo, adicionar uma opção "Foto de fundo" (visível mas bloqueada para Free, com badge Pro+)
2. Ao selecionar essa opção (usuário Pro+), exibir um campo de input para URL da imagem
3. Ao inserir a URL, o preview da página deve atualizar em tempo real mostrando a imagem como background
4. Adicionar opções de overlay: intensidade de escurecimento/clareamento sobre a foto (slider de 0 a 100%) para garantir legibilidade do texto
5. Validar se a URL inserida é uma imagem válida (tentar carregar via `new Image()` no JS antes de salvar)

**Persistência:**
1. Salvar a URL da imagem e a intensidade do overlay na entidade de configuração visual do usuário (coluna JSON existente ou campo dedicado)
2. Validar no backend que o usuário possui plano Pro+ antes de persistir

**Página pública (`/p/{slug}`):**
1. Ao renderizar a página pública, aplicar a imagem como background via CSS inline ou classe gerada
2. O overlay deve ser aplicado como pseudo-elemento ou div com `background: rgba(0,0,0,{intensidade})` sobre a imagem
3. Garantir que o texto e os links fiquem legíveis sobre qualquer imagem

### Critério de aceitação
Um usuário Pro+ insere uma URL de imagem no editor, vê o preview em tempo real, salva e a imagem aparece como background na sua página pública com o overlay configurado.

---

## Tarefa 6 — Visual: Modernizar Hover e Efeitos Interativos

### Objetivo
Revisar e modernizar os efeitos de hover em todo o produto, especialmente no tema escuro e na home, para que a experiência seja sofisticada e coerente com o nível de polimento esperado de um produto SaaS moderno.

### Princípios a seguir
- Transições suaves: sempre `transition: all 0.2s ease` ou mais específico — nunca mudança abrupta
- Profundidade sem peso: usar `box-shadow` sutil, `scale(1.02)` ou brilho sutil — não borders grossas ou cores saturadas demais
- Consistência: o mesmo padrão de hover para elementos do mesmo tipo em todas as telas
- No tema escuro: evitar backgrounds de hover muito claros (quebra o tema) — preferir `rgba(255,255,255,0.06)` ou `rgba(255,255,255,0.10)` para superfícies
- Glass morphism quando aplicável: `backdrop-filter: blur(8px)` com `background: rgba(255,255,255,0.08)` para cards sobre fundos animados

### O que revisar e corrigir

**Botões:**
```css
/* Padrão esperado — ajustar às cores do design system existente */
.btn-primary {
  transition: transform 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
}
.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 20px rgba(var(--accent-rgb), 0.35);
}
.btn-primary:active {
  transform: translateY(0);
}
```

**Cards (tema escuro):**
```css
.card:hover {
  background: rgba(255, 255, 255, 0.07);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  transform: translateY(-2px);
  transition: all 0.25s ease;
}
```

**Links de navegação:**
- Hover deve ter uma transição de cor suave, não mudar abruptamente
- Adicionar um underline animado ou indicador de hover sutil

**Home:**
- Revisar todos os elementos interativos da home (CTAs, cards de features, links) e garantir que o hover seja consistente com os padrões acima
- O fundo animado da home não deve ser afetado pelo hover de elementos sobre ele

### Critério de aceitação
Qualquer elemento interativo em qualquer tela tem um hover suave, com transição e sem saltos visuais. O tema escuro não apresenta hovers com contraste excessivo ou quebra de paleta.

---

## Tarefa 7 — Responsividade: Cards e Grids Uniformes em Todas as Telas

### Objetivo
Garantir que todos os grids de cards no produto (dashboard, escolha de estilo/templates, pricing) mantenham uniformidade visual independentemente do conteúdo ou da resolução.

### Problemas a resolver

**Grid de templates (onboarding e editor):**
1. Cards de template não devem crescer ao ser selecionados — o estado selecionado deve ser indicado apenas por borda, brilho ou overlay, nunca por mudança de tamanho
2. Todos os cards de template devem ter `aspect-ratio` fixo (ex: `aspect-ratio: 3/4` ou `aspect-ratio: 1`) para manter proporção em qualquer grid
3. O grid deve quebrar corretamente em mobile — máximo 2 colunas em telas < 640px, 3-4 colunas em desktop

**Dashboard e outras listagens:**
1. Aplicar `align-items: stretch` em todos os grids de cards
2. Usar `min-height` nos cards para garantir altura mínima mesmo com conteúdo escasso
3. Em mobile (< 768px), cards de dashboard devem empilhar em 1 ou 2 colunas

**Regra CSS global a implementar:**
```css
/* Aplicar em todos os grids de cards do produto */
[class*="cards-grid"],
[class*="templates-grid"],
[class*="dashboard-grid"] {
  display: grid;
  align-items: stretch;
}

[class*="cards-grid"] > *,
[class*="templates-grid"] > *,
[class*="dashboard-grid"] > * {
  height: 100%;
}
```

**Verificar em:**
- `/dashboard`
- Onboarding step de templates
- Editor (seleção de tema)
- Página de pricing (cards de plano)

### Critério de aceitação
Nenhum card muda de tamanho ao ser selecionado, hover ou em qualquer estado. O grid é uniforme em desktop (1280px), tablet (768px) e mobile (375px).

---

## Tarefa 8 — Revisão Geral de Código: Frontend e Backend

### Objetivo
Fazer uma revisão técnica completa do codebase, identificando e corrigindo problemas de qualidade, segurança, performance e manutenibilidade.

### Frontend (Blazor / CSS)

**Identificar e corrigir:**
1. Componentes `.razor` com mais de 300 linhas — extrair subcomponentes
2. Lógica de negócio diretamente em componentes razor (deve estar em serviços)
3. CSS duplicado entre arquivos — consolidar em variáveis ou classes utilitárias
4. Chamadas a `StateHasChanged()` em excesso (causar re-renders desnecessários)
5. `@onclick` sem `async/await` quando a ação é assíncrona
6. Falta de `@key` em listas renderizadas com `@foreach`
7. Strings hardcoded que deveriam ser constantes ou recursos

**Padronizar:**
- Nomenclatura de classes CSS: usar um padrão consistente (BEM ou similar)
- Nomenclatura de componentes: PascalCase, sem prefixo desnecessário
- Parâmetros de componentes com `[Parameter]` sempre com valores default quando aplicável

### Backend (.NET / EF Core)

**Identificar e corrigir:**
1. Queries N+1 — usar `.Include()` e `.ThenInclude()` onde necessário
2. `DbContext` sendo reutilizado fora do escopo correto (Blazor Server tem ciclo de vida longo — usar factory pattern ou scoped context corretamente)
3. Endpoints ou serviços sem validação de autorização — checar todas as operações de escrita
4. Dados do usuário acessíveis sem checar se pertencem ao usuário autenticado (ex: editar página de outro usuário via ID)
5. Senhas, tokens ou dados sensíveis sendo logados
6. Migrations pendentes ou inconsistentes com o modelo atual

**Adicionar onde faltar:**
- Logging estruturado em operações críticas (login, criação de página, mudança de plano)
- Tratamento de exceções global (middleware de error handling) que retorne respostas consistentes

### Critério de aceitação
O projeto compila sem warnings relevantes. Nenhuma query N+1 identificada nas rotas principais. Todas as operações de escrita validam que o recurso pertence ao usuário autenticado. Nenhum dado sensível aparece em logs.

---

## Tarefa 9 — Editor de Página: Mais Opções e Experiência Aprimorada

### Objetivo
Enriquecer o editor de página com mais opções de personalização e melhorar a experiência de edição, tornando-o mais próximo de um editor visual profissional.

### Novas opções de personalização a implementar

**Tipografia:**
- Seletor de família de fonte para o nome/título da página (5-8 opções via Google Fonts)
- Opção de tamanho do nome (pequeno, médio, grande)

**Botões de link:**
- Seletor de estilo do botão: preenchido, outline, ghost, arredondado (pill), quadrado
- Opção de ícone no botão: sem ícone, ícone à esquerda, ícone à direita (usar conjunto de ícones já presente no projeto)
- Animação de hover do botão: nenhuma, deslizar, brilhar, escalar (aplicado via classe CSS)

**Layout da página:**
- Alinhamento do conteúdo: esquerda, centro, direita
- Espaçamento entre links: compacto, normal, espaçado

**Avatar/foto de perfil:**
- Forma do avatar: círculo, quadrado, quadrado arredondado
- Tamanho do avatar: pequeno, médio, grande

**Seção de bio:**
- Limite de caracteres exibido em tempo real (ex: "120/150")
- Toggle para mostrar/ocultar a bio na página pública

### Melhorias de experiência no editor

1. **Reorganização de links por drag-and-drop** — se já existe, garantir que funciona e dá feedback visual durante o drag; se não existe, implementar usando a API de Drag and Drop do HTML5 ou uma biblioteca leve compatível com Blazor
2. **Duplicar link** — botão de ação rápida para duplicar um link existente
3. **Toggle de visibilidade por link** — cada link deve ter um toggle para mostrar/ocultar sem precisar excluir
4. **Preview responsivo** — botões no painel de preview para simular visualização em mobile e desktop
5. **Contador de cliques por link** — exibir no editor quantos cliques cada link recebeu (se o dado já existe no banco)
6. **Auto-save** — salvar automaticamente as alterações após 2 segundos de inatividade (debounce), com indicador de status "Salvando..." / "Salvo"

### Critério de aceitação
O editor oferece pelo menos as opções de tipografia, estilo de botão, alinhamento e forma de avatar. Drag-and-drop de links funciona. Auto-save está ativo. O preview reflete todas as mudanças em tempo real.

---

## Tarefa 10 — Upload de Arquivo como Background (Pro+)

### Objetivo
Implementar upload de imagem como background de página para usuários Pro+, de forma que funcione corretamente em ambiente de produção (Railway/container efêmero).

### Problema do approach anterior
Salvar arquivos no filesystem local não funciona em ambientes container (Railway destrói o filesystem a cada deploy). A solução correta é armazenar a imagem em base64 no banco de dados ou usar um serviço de storage externo.

### Solução a implementar: Base64 no banco

Esta é a solução mais simples e confiável para o escopo atual do produto:

1. O usuário seleciona um arquivo de imagem (`.jpg`, `.png`, `.webp`)
2. O frontend lê o arquivo via `InputFile` e converte para base64
3. O base64 é enviado ao backend e salvo na coluna JSON de configuração visual do usuário
4. A página pública usa o base64 como `background-image: url('data:image/...;base64,...')`

**Limites e validações obrigatórias:**
- Tamanho máximo do arquivo: **2MB** — validar no frontend antes do upload e no backend antes de salvar
- Tipos permitidos: `image/jpeg`, `image/png`, `image/webp` — validar no backend pelo conteúdo, não apenas pela extensão
- Verificar que o plano do usuário é Pro+ antes de aceitar o upload
- Exibir barra de progresso ou spinner durante a conversão/envio
- Exibir mensagem de erro clara se o arquivo for muito grande ou tipo inválido

**UI no editor:**
1. A opção "Upload de imagem" deve estar dentro da seção de fundo, logo abaixo de "URL de imagem" (Tarefa 5)
2. Ambas as opções (URL e upload) são exclusivas — selecionar uma desmarca a outra
3. Após o upload, o preview atualiza imediatamente com a imagem
4. Botão para remover a imagem de fundo deve estar presente

**Limitação a comunicar:**
- Exibir uma nota discreta no editor: "Imagens salvas no perfil podem aumentar o tempo de carregamento da página." — para transparência com o usuário

### Critério de aceitação
Um usuário Pro+ consegue fazer upload de uma imagem (≤ 2MB), vê o preview imediato no editor, salva, e a imagem aparece corretamente como background na página pública — inclusive após redeploy do servidor.

---

## Ordem de Execução Recomendada

```
Tarefa 0  →  Tarefa 4  →  Tarefa 1  →  Tarefa 3  →  Tarefa 2  →  Tarefa 7
    →  Tarefa 6  →  Tarefa 5  →  Tarefa 10  →  Tarefa 9  →  Tarefa 8
```

**Lógica da ordem:**
- **Tarefa 0** (diagnóstico) abre a sessão — nenhuma linha de código antes de entender o estado real do produto
- **Tarefa 4** (logout) é a mais simples e isolada — vitória rápida antes de entrar nas partes complexas
- **Tarefas 1 e 3** limpam e corrigem regras de negócio antes de qualquer trabalho visual
- **Tarefas 2 e 7** são ajustes de layout — rápidas e de alto impacto visual
- **Tarefa 6** (hover/visual) depois do layout estar correto
- **Tarefas 5 e 10** (background via URL e upload) juntas pois compartilham a mesma seção do editor
- **Tarefa 9** (editor expandido) por cima de uma base já sólida
- **Tarefa 8** (revisão de código) por último — visão geral do que ficou, limpeza final e checagem de aderência ao briefing