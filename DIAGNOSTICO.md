# Diagnóstico de Produto — UmbLink

> Gerado em: 2026-03-29

---

## 1. Simplicidade do Onboarding

**Status:** ✅ OK

**O que foi encontrado:**
O wizard em `Onboarding/Wizard.razor` possui 3 passos visuais (indicador de progresso exibe "Perfil → Template → Publicar"), mas internamente são 4 estados (`_step` 1 a 4, onde o passo 4 é a tela de sucesso). O fluxo real é:

1. **Perfil** — escolher tipo de perfil entre 6 opções por cartão visual (obrigatório)
2. **Template** — escolher estilo visual com filtro por categoria (obrigatório clicar em algo)
3. **Configurar** — preencher nome, slug (auto-gerado a partir do nome) e bio (opcional) + até 3 links (opcional) + preview ao vivo à direita (desktop) ou toggle para ver no mobile
4. **Sucesso** — URL gerada com botão de copiar e atalhos para editor/dashboard

Campos obrigatórios mínimos: apenas **nome** (≥2 chars) e **slug** (≥3 chars). O slug é auto-preenchido a partir do nome com conversão de acentos e sanitização, reduzindo a fricção. Links são opcionais no onboarding.

A página já é criada e publicada automaticamente no passo 3 — o usuário sai do onboarding com uma URL funcional.

**O que falta ou precisa melhorar:**
- O passo 1 (tipo de perfil) e o passo 2 (template) poderiam ser fundidos em um único passo, reduzindo o wizard de 3 para 2 passos visuais sem perder informação
- O campo de avatar está ausente no onboarding — o usuário precisa ir ao editor para adicionar foto, o que impacta a primeira impressão da página
- O passo 3 exibe 3 campos de link vazios por padrão; usuários que não querem adicionar links no momento podem se sentir pressionados a preencher
- Não há validação de disponibilidade do slug em tempo real no onboarding (diferente do editor que tem campo igual mas também sem debounce/check visível no wizard)

**Impacto:** Médio

---

## 2. Editor e Personalização

**Status:** ✅ OK

**O que foi encontrado:**
O editor em `Pages/Dashboard/Editor.razor` oferece:

- **Informações da página:** título, slug (editável), bio, avatar por URL
- **Links:** lista com reordenação drag-and-drop (SortableJS), toggle ativo/inativo, delete, modal de adição com "atalhos rápidos" (plataformas conhecidas via `LinkSuggestionsPanel`)
- **Aparência (painel colapsível):** 12 temas (4 free, 8 premium com lock para usuários Free), 4 estilos de botão, 2 color pickers (cor do botão, cor do texto), 3 formatos de avatar (circle, square, rounded), opções de espaçamento (compact/normal/relaxed), fundo sólido/gradiente/imagem com upload, fontes (título e link)
- **Preview ao vivo:** componente `PagePreview` renderizado em coluna lateral no desktop; em mobile há botão toggle para mostrar/ocultar preview
- **Toolbar:** botões "Ver", "Métricas", "Link", "Salvar" sempre visíveis; status publish/unpublish presente

**O que falta ou precisa melhorar:**
- O campo de avatar aceita apenas URL — upload de avatar direto no editor não está integrado à UI (o endpoint `/api/upload/avatar` existe, mas não há botão de upload na seção de informações do editor; o usuário teria que ir a Configurações de Conta)
- O campo "Endereço" (slug) no editor não valida disponibilidade em tempo real — o usuário só descobre conflito ao salvar
- Fundo com imagem requer que o usuário saiba que precisa clicar em "Aparência" (colapsível) e depois em "Fundo" e depois fazer upload — muitos cliques para uma feature importante
- Não há opção de preview mobile simulado dentro do editor (preview lateral usa max-width 480px, mas não tem "frame de celular" visual)

**Impacto:** Médio

---

## 3. Performance e Qualidade da Página Pública

**Status:** ⚠️ Parcial

**O que foi encontrado:**
A página pública em `Pages/Public/ProfilePage.razor` apresenta:

- **Meta tags presentes:** `<title>`, `<meta name="description">`, `og:title`, `og:description`, `og:image` (condicional se avatar existir), `og:type`
- **Layout mobile-first:** max-width 480px centralizado, py-4 px-3, `min-vh-100`, flex-column
- **Tracking automático:** view é registrado no `OnInitializedAsync` via `MetricsService.TrackViewAsync`
- **Branding removal:** implementado na linha 121 — verifica se dono tem plano Pro ou Business para suprimir o rodapé "Criado com UmbLink" (a TODO da linha 98 foi parcialmente resolvida, mas usa verificação de string no PlanName em vez do campo `AllowRemoveBranding` do `PlanLimit`)
- **Cache:** `PageService.GetBySlugAsync` usa cache com TTL 10min (key `page:slug:{slug}`)
- **Animações de hover nos links:** inline via `onmouseover`/`onmouseout` em JavaScript inline — funcional mas não ideal para performance

**O que falta ou precisa melhorar:**
- Ausência de `og:url` e `twitter:card` / `twitter:image` — compartilhamentos no Twitter/X não terão card rico
- Ausência de `<link rel="canonical">` — risco de duplicate content se o domínio for acessado por http e https
- A página é renderizada server-side via Blazor Server — qualquer latência no servidor aumenta o Time to First Byte (TTFB). Para uma página pública de link-in-bio acessada principalmente via celular em links de bio, um modelo SSG ou SSR puro seria mais performático
- `ProfilePage.razor` faz duas queries adicionais por render: `PageService.GetLinksAsync` e `SubSvc.GetAsync` (para checar branding) — a segunda não está cacheada e adiciona latência
- Sem `loading="lazy"` no `<img>` do avatar
- Animação de hover usa JavaScript inline (strings hardcoded no HTML); em mobile não há feedback de tap

**Impacto:** Médio

---

## 4. Métricas

**Status:** ⚠️ Parcial

**O que foi encontrado:**
A página `Pages/Dashboard/Metrics.razor` exibe:

- Cards de resumo: **Total de Visualizações**, **Total de Cliques**, **CTR Geral** (calculado em tempo real como clicks/views × 100)
- Gráfico de barras de visualizações diárias via Chart.js (`canvas id="views-chart"`)
- Tabela de performance por link: título, total de cliques, CTR individual
- Seletor de período: 7, 30 ou 90 dias (limitado pelo plano — Free fica restrito a 7 dias por `AnalyticsDays` do `PlanLimit`)

Tracking ocorre via:
- Views: `POST /api/track/view/{pageId}` (rate limited 30 req/min) e via `MetricsService.TrackViewAsync` direto no server-side render
- Clicks: `GET /r/{linkId}` → redirect rastreado + `POST /api/track/click/{linkId}`

**O que falta ou precisa melhorar:**
- Ausência de dado de **referrer** na UI — `PageView.Referrer` e `ClickEvent.Referrer` são coletados, mas não exibidos nas métricas do usuário (só Admin tem acesso via `AnalyticsRepository`)
- Sem **gráfico de cliques diários** — apenas visualizações diárias têm gráfico; cliques ficam apenas na tabela de links
- Sem métrica de **taxa de retorno** ou **visitantes únicos** — não há deduplicação por IP ou cookie
- Período máximo de 90 dias na UI (mesmo para Business que tem `AnalyticsDays = -1`)
- `ProcessExpiredTrialsAsync` não é chamado por job agendado (documentado em CLAUDE.md seção 15) — trials expirados não têm dados de analytics corretamente expirados

**Impacto:** Médio

---

## 5. Planos e Monetização

**Status:** ⚠️ Parcial

**O que foi encontrado:**
A página `Pages/Plans/Index.razor` apresenta:

- Toggle de período (Mensal / Trimestral -15% / Anual -25%) com slider visual animado
- 3 cards de plano (Free, Pro destacado como "Mais popular", Business) com features comparativas usando ícones check/x
- CTAs contextuais por estado atual: "Plano atual" (disabled), "Assinar", "Trial grátis (7 dias)", "Fazer downgrade"
- Modal de trial com coleta de dados de cartão (campos: número, nome, validade, CVV) — os dados são salvos via `PaymentRepo.SaveAsync` mas **não há gateway real**; o trial é ativado sem cobrança real
- Preços exibidos dinamicamente conforme período selecionado

**Diferenciação Free vs Pro:** clara na UI — Free mostra limitações explícitas com ícones X para domínio customizado e remover branding.

**O que falta ou precisa melhorar:**
- **Gateway de pagamento ausente** — `Checkout.razor` usa `Task.Delay(2000)` para simular processamento. O modal de trial coleta dados de cartão reais mas não os processa (o `PaymentRepo` apenas salva os últimos 4 dígitos localmente). Isso é um bloqueador crítico para receita real
- O Free plan lista apenas 5 features — não menciona a geração de perfil por IA (Groq), que é um diferencial forte disponível no onboarding
- Não há comparação visual de "o que Pro inclui a mais" em formato de tabela completa — a lista de features é muito resumida (5 itens por plano)
- O trial de 7 dias exige dados de cartão antes de iniciar — atrito alto para um trial "gratuito". Um trial sem cartão seria mais eficaz para conversão
- Trials expirados não são processados automaticamente (falta de scheduled job para `ProcessExpiredTrialsAsync`)
- Após cancelamento via "Fazer downgrade", o usuário é redirecionado para `/dashboard` sem feedback de confirmação do que foi cancelado (páginas/links suspensos)

**Impacto:** Alto

---

## 6. Qualidade Visual Geral

**Status:** ✅ OK

**O que foi encontrado:**

**Sistema de design (site.css):**
- CSS variables bem estruturadas cobrindo dois temas (light/dark) com tokens semânticos: `--bg-primary`, `--bg-card`, `--text-primary`, `--accent` (#557cf2), `--border`, `--shadow`, `--success`, `--warning`, `--danger`
- Dark mode com palette coerente (background #0a0e1a, cards #151c30, bordas com alpha do accent)
- Bootstrap 5.3.3 como base, com overrides via variáveis CSS (`--bs-*`) para integração limpa
- Fonte padrão: Inter (Google Fonts), com antialiasing ativado

**MainLayout.razor:**
- Navbar com blur glassmorphism (`--navbar-bg` com rgba + backdrop-filter no CSS)
- Menu hamburguer para mobile com dropdown inline responsivo
- Toggle de tema light/dark persistido via localStorage (via `umblink.getTheme`/`setTheme`)
- Footer sticky via flex-column + flex-grow-1 no main
- Links de navegação incluem: Dashboard, Planos, Meu Plano, Configurações, Admin (condicional), Logout

**O que falta ou precisa melhorar:**
- O avatar no editor aceita apenas URL — sem preview imediato do avatar no layout do editor antes de salvar (o `PagePreview` já reflete, mas o campo URL é pouco intuitivo comparado a um botão de upload)
- Ausência de animações de transição de página (Blazor Server não tem transição nativa entre rotas)
- A navbar em mobile mostra apenas o ícone hamburguer sem indicar qual página está ativa — sem estado "active" nos links de navegação
- Estilos inline extensivos nos componentes `.razor` (especialmente `Wizard.razor` e `Editor.razor`) dificultam manutenção e sobrescrevem variáveis CSS em vez de utilizá-las via classes utilitárias

**Impacto:** Baixo

---

## Resumo Executivo

| Critério | Status | Impacto |
|---|---|---|
| Onboarding | OK | Médio |
| Editor e Personalização | OK | Médio |
| Página Pública (SEO/Performance) | Parcial | Médio |
| Métricas | Parcial | Médio |
| Planos e Monetização | Parcial | **Alto** |
| Qualidade Visual | OK | Baixo |

**Prioridade imediata:** A ausência de gateway de pagamento real bloqueia toda receita. O trial coleta dados de cartão sem processar — além de não gerar receita, representa um risco de expectativa com o usuário. Este é o único bloqueador crítico para o produto operar comercialmente.

**Prioridades secundárias:**
1. Upload de avatar direto no editor (UX gap em feature core)
2. Validação de slug em tempo real no wizard e editor
3. Adicionar `og:url`, `twitter:card` e `<link rel="canonical">` na página pública
4. Scheduled job para `ProcessExpiredTrialsAsync`
5. Exibir dados de referrer nas métricas do usuário
