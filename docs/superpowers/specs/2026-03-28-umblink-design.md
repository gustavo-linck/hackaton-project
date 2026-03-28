# UmbLink — Design Spec
**Data:** 2026-03-28
**Contexto:** Hackathon Umbler — sistema avaliado por jurados via link público

---

## 1. Objetivo do Produto

UmbLink é um criador de páginas de links para bio de redes sociais, operando em modelo freemium (Free / Pro / Business). O produto resolve o problema do "link único na bio" e serve como porta de entrada ao ecossistema Umbler, gerando receita recorrente via planos pagos com múltiplas periodicidades (mensal, trimestral, anual).

---

## 2. Contexto de Negócio

- **Produto:** Umbler — ecossistema de ferramentas para criadores e negócios digitais
- **Modelo de receita:** Freemium com planos pagos (Pro e Business) em ciclos mensal, trimestral e anual
- **Objetivo estratégico:** Porta de entrada no ecossistema Umbler → upsell/cross-sell para outros produtos
- **Avaliação:** Jurados acessam via link público, sem instalação, sem cadastro para ver páginas públicas

---

## 3. Usuários e Personas

| Perfil | Descrição |
|---|---|
| **Free** | Criador iniciante, quer montar uma página rápida sem custo |
| **Pro** | Criador ativo, precisa de personalização, múltiplas páginas e métricas detalhadas |
| **Business** | Agência ou profissional, precisa de branding próprio e domínio customizado |
| **Admin Umbler** | Equipe interna, gerencia usuários, monitora métricas globais |

---

## 4. Modelo de Planos

### 4.1 Limites por Plano

| Funcionalidade | Free | Pro | Business |
|---|---|---|---|
| Páginas de links | 1 | 3 | ilimitado |
| Links por página | 3 | 5 | ilimitado |
| Personalização visual | básica | completa | completa |
| Temas disponíveis | 2 básicos | todos | todos |
| Fontes disponíveis | 2 | todas | todas |
| Métricas | 7 dias | 1 ano | total (sem limite) |
| Origens de acesso (referrer) | não | sim | sim |
| Domínio customizado | não | sim | sim |
| Remoção do branding Umbler | não | não | sim |
| Suporte | comunidade | e-mail | prioritário |

### 4.2 Preços e Periodicidades

| Plano | Mensal | Trimestral (~15% off) | Anual (~25% off) |
|---|---|---|---|
| Free | R$ 0 | — | — |
| Pro | R$ 19/mês | R$ 16/mês | R$ 14/mês |
| Business | R$ 75/mês | R$ 64/mês | R$ 56/mês |

A tela de upgrade exibe toggle **Mensal / Trimestral / Anual** com badge de economia destacado.

### 4.3 Modelo de Dados de Preços

```
Plan: Id, Name (Free/Pro/Business), IsActive
PlanLimit: PlanId, MaxPages, MaxLinksPerPage, AnalyticsDays (-1 = ilimitado),
           AllowReferrer, AllowCustomDomain, AllowRemoveBranding,
           ThemeCount (-1 = todos), FontCount (-1 = todas)
PlanPrice: Id, PlanId, BillingPeriod (monthly/quarterly/annual),
           PricePerMonth, TotalCharged, DiscountPercent
```

### 4.4 Trial

- Pro e Business oferecem trial gratuito de 7 dias (informar cartão, cobrança apenas no 7º dia)
- Trial só pode ser iniciado **uma vez por plano por usuário** — enforçado no banco via constraint
- No 7º dia: banner de aviso que trial encerra em 24h
- Ao encerrar sem conversão: downgrade para Free, conteúdo preservado mas bloqueado
- O período de billing (mensal/trimestral/anual) é escolhido no momento do trial e mantido na conversão

### 4.5 Downgrade

- Ao cancelar plano pago e voltar para Free:
  - Páginas excedentes: desativadas (não excluídas)
  - Links excedentes: inativos (não excluídos)
  - Usuário vê aviso claro + opção de escolher quais manter
  - Dados **nunca** deletados silenciosamente

---

## 5. Domínio — Entidades e Relacionamentos

### Entidades

```
User
  Id (Guid)
  Email (unique)
  PasswordHash
  Name
  AvatarUrl
  Role (User / Admin)
  IsActive
  CreatedAt

Plan
  Id, Name, IsActive

PlanLimit (1:1 com Plan)
  PlanId, MaxPages, MaxLinksPerPage, AnalyticsDays,
  AllowReferrer, AllowCustomDomain, AllowRemoveBranding,
  ThemeCount, FontCount

PlanPrice (N:1 com Plan)
  Id, PlanId, BillingPeriod (monthly/quarterly/annual),
  PricePerMonth (decimal), TotalCharged (decimal), DiscountPercent (int)

Subscription (1:1 com User)
  Id, UserId, PlanId, PlanPriceId (nullable — null = Free),
  Status (free / trial / active / cancelled / expired),
  BillingPeriod (monthly/quarterly/annual),
  TrialStartedAt (nullable), TrialEndsAt (nullable),
  CurrentPeriodStart (nullable), CurrentPeriodEnd (nullable),
  CreatedAt, UpdatedAt

TrialUsage (previne duplo trial)
  Id, UserId, PlanId, StartedAt, Status (active/expired/converted)
  UNIQUE (UserId, PlanId)

Page
  Id, UserId, Slug (unique, 3-30 chars, alphanum + hífens),
  Title, Bio, AvatarUrl, Status (draft/published/suspended),
  ThemeConfig (JSON), CreatedAt, UpdatedAt

Link
  Id, PageId, Title (max 50), Url, IconName, IsActive, Order, CreatedAt

ClickEvent
  Id, LinkId, Timestamp, UserAgentSummary, Referrer (nullable)
  — sem IP completo (LGPD)

PageView
  Id, PageId, Timestamp, Referrer (nullable)
  — sem IP completo (LGPD)

CustomDomain
  Id, PageId, Domain, CnameTarget, Status (pending/active/failed)

AuditLog
  Id, UserId, Action (string), Metadata (JSON), CreatedAt
```

### Fluxos de Estado

```
Subscription.Status:  free → trial → active → cancelled → expired
Page.Status:          draft → published ↔ suspended
Link.IsActive:        true ↔ false
TrialUsage.Status:    active → expired | converted
```

---

## 6. Funcionalidades

### Deve Ter (escopo obrigatório)

- Autenticação: cadastro/login e-mail+senha, OAuth2 Google, recuperação de senha
- Editor de página com split-view (editor + preview de celular) em tempo real
- Drag-and-drop para reordenar links
- Personalização visual com temas, cores, fontes (limitada por plano)
- Página pública em `/{slug}` — sem login, mobile-first, leve
- Métricas: acessos, cliques, taxa de clique, referrer (por plano), gráfico temporal
- Gestão de planos: tela "Meu Plano", upgrade, formulário de pagamento mockado, histórico de faturas simulado, cancelamento
- Tela de planos com toggle mensal/trimestral/anual e comparativo visual
- Trial de 7 dias com billing period selecionado
- Domínio customizado (campo + instruções CNAME — sem validação DNS real)
- Painel admin: lista usuários, métricas globais, alterar plano, suspender/reativar
- Branding Umbler no rodapé (removível no Business)
- Seed de demo completo (jurado vê algo funcional ao abrir sem cadastrar)

### Pode Ter (agrega valor real)

- QR Code da página pública (download PNG) — valor imediato para divulgação offline
- Link de WhatsApp pré-configurado (ícone + prefixo `https://wa.me/`) — caso de uso muito comum
- Ordenação de links por cliques no dashboard — insight de otimização rápido
- Preview de como a página aparece no Google/WhatsApp (OG tags) — confiança do usuário

### Fora do Escopo

- Venda de produtos / agendamento (escopo Hopp/Wix)
- Processamento de pagamento real
- Validação DNS real para domínio customizado
- Envio de e-mail transacional real
- Internacionalização (i18n)

---

## 7. UX/UI

### 7.1 Jornadas

**Onboarding (< 3 min):**
Cadastro → criar página (slug, título, avatar) → adicionar até 3 links → publicar → ver página pública

**Edição:**
Dashboard → selecionar página → editor split-view → alterações salvas automaticamente (debounce 800ms) ou botão "Salvar"

**Upgrade:**
Atingiu limite → PlanLimitGuard exibe cadeado + mensagem humana + CTA → UpgradeModal com toggle de período → selecionar plano → formulário de pagamento → confirmação → plano ativo imediatamente

**Downgrade:**
Meu Plano → Cancelar → ConfirmModal com lista do que será bloqueado → confirmar → Subscription.Status = cancelled → página de aviso com seleção dos itens a manter

**Métricas:**
Dashboard → aba Métricas → seletor de período (conforme plano) → gráfico + tabela de links

**Admin:**
/admin → lista de usuários com filtros → detalhe do usuário → alterar plano / suspender

### 7.2 Layout do Editor

```
┌─────────────────────────────┬─────────────────────┐
│ EDITOR (esquerda)           │ PREVIEW (direita)   │
│ - Título, bio, avatar       │ [frame de celular]  │
│ - Lista de links            │ Página renderizada  │
│ - Drag handles              │ em tempo real       │
│ - Botão Adicionar Link      │                     │
│ - Aba Aparência             │ Toggle Mobile/Desktop│
└─────────────────────────────┴─────────────────────┘
```

### 7.3 Componentes Blazor Reutilizáveis

| Componente | Propósito |
|---|---|
| `PlanLimitGuard` | Envolve ações bloqueadas por plano; exibe cadeado + CTA |
| `PlanBadge` | Badge colorido com nome do plano atual |
| `UpgradeModal` | Modal com comparativo de planos + toggle de período + CTA |
| `TrialBanner` | Banner persistente durante trial; urgente quando < 24h |
| `UsageMeter` | Barra de progresso (ex: "2 de 3 links usados") |
| `ConfirmModal` | Modal de confirmação para ações destrutivas |
| `ToastService` | Notificações temporárias (sucesso, erro, aviso) |
| `LoadingSpinner` | Estado de carregamento reutilizável |
| `LinkCard` | Card de link com drag handle, toggle, editar, excluir |

### 7.4 Estados de UI Obrigatórios

Toda tela/componente deve implementar: **vazio, carregando, erro, sucesso, sem permissão, limite atingido.**

- **Limite atingido**: nunca mensagem técnica — exibir o que está bloqueado + por quê + CTA de upgrade contextual
- **Erro**: mensagem em linguagem humana (nunca stack trace)
- **Ações destrutivas**: sempre `ConfirmModal` com aviso de consequência

### 7.5 Padrão de Feedback

- **Toast**: operações rápidas (salvar, copiar link, ativar/desativar link)
- **Modal de confirmação**: ações destrutivas (excluir, cancelar plano, troca de slug)
- **Inline alert**: erros de formulário
- **Banner**: trial ativo, downgrade pendente, aviso de slug

---

## 8. Sistema de Temas e Personalização Visual

### 8.1 Temas Disponíveis (mínimo 4)

| ID | Nome | Disponível em |
|---|---|---|
| `minimal-light` | Minimal Light | Free, Pro, Business |
| `dark-pro` | Dark Pro | Free, Pro, Business |
| `gradient-sunset` | Gradient Sunset | Pro, Business |
| `glass-morphism` | Glass | Pro, Business |

### 8.2 ThemeConfig (JSON na Page)

```json
{
  "themeId": "minimal-light",
  "bgColor": "#ffffff",
  "bgGradient": null,
  "bgImageUrl": null,
  "buttonStyle": "rounded",
  "buttonColor": "#000000",
  "buttonTextColor": "#ffffff",
  "textColor": "#111111",
  "titleFont": "inter",
  "linkFont": "inter",
  "spacing": "normal",
  "avatarShape": "circle"
}
```

### 8.3 Enforcement de Limites Visuais

- Campos de personalização Pro+ são renderizados via `PlanLimitGuard`
- Ao tentar editar campo bloqueado: cadeado visual + tooltip "Disponível no Pro"
- Temas bloqueados: exibidos no seletor com overlay de cadeado (não ocultados)

---

## 9. Enforcement de Limites de Plano

**Centralizado em `PlanLimitService`** — único ponto de verificação na camada Application.

```csharp
// Único ponto de entrada para qualquer verificação de limite
IPlanLimitService.CanAddPage(userId) → Result<bool, LimitExceededError>
IPlanLimitService.CanAddLink(pageId) → Result<bool, LimitExceededError>
IPlanLimitService.CanUseFeature(userId, Feature.CustomDomain) → Result<bool, LimitExceededError>
```

- Application Services chamam `PlanLimitService` antes de qualquer mutação
- `LimitExceededError` carrega: `FeatureName`, `CurrentPlan`, `RequiredPlan`, `UpgradeUrl`
- UI interpreta `LimitExceededError` e renderiza `PlanLimitGuard` com dados corretos
- Limites são lidos do banco (`PlanLimit` table) — **nunca hardcoded**

---

## 10. Arquitetura

### 10.1 Abordagem Escolhida: Clean Architecture Simplificada (3 camadas)

```
UmbLink.sln
├── src/
│   ├── UmbLink.Web/              ← Blazor Server (UI, componentes, páginas)
│   ├── UmbLink.Application/      ← Services, Interfaces, DTOs, Validações
│   └── UmbLink.Infrastructure/   ← EF Core, Identity, Migrations, Background Services
├── tests/
│   ├── UmbLink.UnitTests/
│   └── UmbLink.IntegrationTests/
├── Dockerfile
└── docker-compose.yml
```

**Direção das dependências:**
```
Web → Application → Infrastructure
         ↑
    (interfaces definidas em Application, implementadas em Infrastructure)
```

### 10.2 Por que Blazor Server

- Preview em tempo real via SignalR — sem round-trip HTTP para cada keystroke
- Deploy em container único (Web + Application + Infrastructure num único processo)
- Sem problemas de CORS, sem bundle WASM
- Desvantagem (estado em memória por conexão) aceitável para hackathon e early-stage

### 10.3 Por que SQLite

- Zero configuração, deploy em volume persistido no Railway
- Suficiente para demo e primeiros meses de uso real
- Ponto de extensão: migration para PostgreSQL é troca de provider no EF Core

### 10.4 Por que Railway

- Suporte nativo a Docker, volumes persistidos, URL pública automática
- Sem cold start — serviço sempre ativo, crítico para a experiência do jurado
- Deploy via `railway up` ou push no GitHub
- **Custo:** ~US$5/mês (Hobby plan) — Railway não tem tier gratuito permanente; o crédito de $5 inicial é one-time por 30 dias
- **Alternativa gratuita — Render:** free tier real (750h/mês), suporte a Docker e SQLite, mas serviços dormem após 15min de inatividade → cold start de ~30-60s no primeiro acesso do jurado. Aceitável somente se configurar um "keep alive" (ping periódico). Dado o contexto de avaliação por jurados, Railway é preferível pelo custo de ~US$5.

### 10.5 Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Entidades | PascalCase | `User`, `Page`, `ClickEvent` |
| Services | `{Nome}Service` | `PageService`, `PlanLimitService` |
| Interfaces | `I{Nome}` | `IPageService` |
| DTOs | `{Nome}Dto` / `Create{Nome}Request` | `PageDto`, `CreateLinkRequest` |
| Blazor pages | PascalCase em `/Pages/` | `Editor.razor`, `Plans.razor` |
| Componentes | PascalCase em `/Components/` | `PlanLimitGuard.razor` |

### 10.6 Validação

**FluentValidation** para DTOs de entrada — justificativa: validação separada da entidade, testável isoladamente, mensagens de erro customizáveis em português.

### 10.7 Tratamento de Erros

- Application Services retornam `Result<T, Error>` (OneOf ou record próprio)
- Blazor components fazem `match` no resultado e exibem estado de UI correto
- Exceções não tratadas → middleware de erro genérico → página de erro amigável
- Logging via `ILogger<T>` com structured logging (sem dados sensíveis)

### 10.8 Registro Assíncrono de Métricas

```
Página pública recebe clique
  → Endpoint público POST /api/track (fire-and-forget)
  → Enfileira em IBackgroundTaskQueue
  → MetricsBackgroundService processa fila e persiste ClickEvent/PageView
  → Response da página pública nunca bloqueada
```

---

## 11. Segurança

- **Autenticação:** ASP.NET Identity + Cookie + OAuth2 Google
- **Admin isolado por arquitetura:** área `/admin` mapeada em `MapRazorPages`/`MapBlazorHub` com policy `AdminOnly` no middleware — não apenas ocultação de UI
- **Rate limiting:** endpoint `/api/track` — 30 req/min por IP via `RateLimiter` do .NET 8
- **XSS:** URLs de links validadas por regex (`^https?://`) antes de salvar; Blazor escapa HTML por padrão
- **CSRF:** Cookie auth do ASP.NET Core inclui proteção CSRF por padrão em Blazor Server
- **Dados de cartão:** jamais armazenados — nem os mockados. Formulário é somente UI.
- **LGPD:** ClickEvent e PageView armazenam `UserAgentSummary` (primeiros 150 chars) e `Referrer` — sem IP
- **Secrets:** User Secrets em dev, variáveis de ambiente em prod — nunca no código
- **AuditLog:** criação de conta, upgrade/downgrade, troca de slug, suspensão pelo admin

---

## 12. Boilerplate — Checklist de Validação

O boilerplate deve compilar e rodar antes de qualquer feature. Checklist mínimo:

- [ ] `dotnet build` sem erros
- [ ] `dotnet ef migrations add Initial` sem erros
- [ ] `dotnet ef database update` cria banco com todas as tabelas
- [ ] Seed executa: planos, limites, preços, usuários, páginas, links, métricas
- [ ] App sobe em `https://localhost:5001`
- [ ] Página pública `/{slug}` retorna 200 sem login
- [ ] Login com e-mail funciona
- [ ] MainLayout e NavMenu renderizam sem erro
- [ ] TrialBanner e PlanLimitGuard compilam sem erro
- [ ] Admin inacessível para usuário comum (retorna 403, não 404)

---

## 13. Seed de Demo

**Usuários pré-criados para o jurado:**

| Usuário | Plano | Slug | Conteúdo |
|---|---|---|---|
| `admin@umblink.com` | Admin | — | Acesso ao painel admin |
| `joao@exemplo.com` | Business | `joao-silva` | 8 links, tema Glass, métricas 30 dias |
| `maria@exemplo.com` | Pro (trial) | `maria-criativa` | 5 links, tema Sunset, métricas 7 dias |
| `carlos@exemplo.com` | Free | `carlos-dev` | 3 links, tema Minimal, métricas básicas |

Senha padrão de todos: `Demo@1234`

Métricas simuladas: geradas retroativamente para os últimos 30 dias com distribuição realista (pico nos dias úteis).

---

## 14. Deploy — Setup

### Plataforma Escolhida

**Railway (~US$5/mês — Hobby plan).** Não existe tier gratuito permanente: o crédito inicial de $5 expira em 30 dias. Railway é a escolha recomendada pois não tem cold start — crítico para a experiência do jurado.

**Alternativa gratuita — Render:** free tier real (750h/mês), suporte Docker + SQLite volume, mas serviços dormem após 15min de inatividade → cold start de ~30–60s no primeiro acesso. Viável somente com um keep-alive externo (ex: UptimeRobot pingando a URL a cada 5min).

---

### Dockerfile (multistage)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/UmbLink.Web/UmbLink.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UmbLink.Web.dll"]
```

### Variáveis de Ambiente

| Variável | Descrição |
|---|---|
| `ConnectionStrings__Default` | `Data Source=/data/umblink.db` |
| `Google__ClientId` | Client ID do OAuth Google |
| `Google__ClientSecret` | Client Secret do OAuth Google |
| `App__BaseUrl` | URL pública gerada pelo Railway |

### Volume Railway

```
/data → volume persistido (SQLite)
```

### OAuth Google

Callback URL a registrar no Google Console:
`{APP_BASE_URL}/signin-google`

---

## 15. Como o Jurado Acessa

1. Abrir o link no navegador (sem instalação)
2. Páginas públicas visíveis sem login: `/{slug}` — ex: `/joao-silva`
3. Para explorar o editor: criar conta ou usar `joao@exemplo.com` / `Demo@1234`
4. Para o painel admin: `admin@umblink.com` / `Demo@1234` → `/admin`

---

## 16. Pontos de Extensão

| Feature futura | Onde conectar |
|---|---|
| Gateway de pagamento real | `IPaymentService` (interface em Application, implementação em Infrastructure) |
| E-mail transacional real | `IEmailService` (substituir implementação mock por SendGrid/SMTP) |
| Domínio customizado real | `IDnsVerificationService` (implementação atual retorna sempre `pending`) |
| Integração produtos Umbler | `IUmblerIdentityService` — SSO via OAuth externo |
| Armazenamento de avatar | `IFileStorageService` — troca local → S3/R2 sem alterar camada Application |

---

## 17. Riscos e Mitigações

| Risco | Mitigação |
|---|---|
| SQLite lock em escrita concorrente | Métricas assíncronas via fila; WAL mode no SQLite |
| Blazor Server: memória por conexão | Acceptable no hackathon; documentado como ponto de migração |
| OAuth Google: callback URL muda com redeploy | `App__BaseUrl` como variável de ambiente; URL registrada no Google Console |
| Seed pesado atrasa startup | Seed verifica `if (!db.Plans.Any())` antes de inserir |
| Trial duplo por race condition | UNIQUE constraint em `TrialUsage(UserId, PlanId)` |

---

## 18. Assunções Documentadas

1. Pro = R$19/mês, Business = R$49/mês (valores sugeridos pelo prompt)
2. Desconto trimestral ~15%, anual ~25% (valores de mercado típicos)
3. "E-mail de confirmação simulado" = modal/toast com detalhes — sem envio real
4. Cada `Page` tem seu próprio `Slug` único (não o username do usuário)
5. Filtro de métricas (7 dias / 3 meses / etc.) é de **visualização** — dados sempre retidos
6. OAuth Google funcional em produção; em dev pode ser desabilitado via feature flag
7. "Sem IP completo" = não armazenar nenhum octeto do IP — apenas timestamp + referrer + user_agent

---

## 19. Ambiguidades Resolvidas

| Ambiguidade | Decisão |
|---|---|
| Um usuário tem múltiplas páginas com slugs diferentes? | Sim — cada Page tem seu slug. Free = max 1 página. |
| Período de métricas é filtro ou retenção? | Filtro de visualização — dados sempre retidos |
| Trial escolhe período de billing no início ou na conversão? | No início — mantido na conversão |
| Formulário de pagamento: campos reais mas não processados | Nunca armazenar dados de cartão, nem os mockados |

---

*Spec gerada e revisada em 2026-03-28. Aprovada para implementação.*
