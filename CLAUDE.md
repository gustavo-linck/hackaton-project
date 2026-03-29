# CLAUDE.md — UmbLink System Context
> Gerado em: 2026-03-29
> Stack: .NET 8 / Blazor Server / EF Core / SQLite / Docker / Railway

---

## 1. Estrutura de projetos

- `src/UmbLink.Web` — Blazor Server entry point: UI, minimal API endpoints, middleware, DI registration
- `src/UmbLink.Application` — Business logic: services, interfaces, DTOs, requests, models, background services
- `src/UmbLink.Infrastructure` — Data layer: EF Core, entities, repositories, Identity, migrations, seed
- `tests/UmbLink.UnitTests` — xUnit + Moq + FluentAssertions; currently only `PlanLimitServiceTests.cs`

**Dependency**: Web → Application + Infrastructure; Application → Infrastructure (not strict Clean Architecture)

---

## 2. Domínio — Entidades principais

### AppUser (Identity)
- Herda: `IdentityUser<Guid>`
- Campos extras: `Name` (MaxLength 50), `AvatarUrl?`, `Role` (UserRole enum, stored as string), `IsActive`, `CreatedAt`
- Navigations: `Subscription?`, `Pages[]`, `TrialUsages[]`, `AuditLogs[]`
- Roles: `Admin`, `User` (seeded via Identity)

### Page
- `Id` (Guid), `UserId` (Guid FK→AppUser), `Slug` (MaxLength 30, unique index), `Title` (MaxLength 60)
- `Bio?` (MaxLength 200), `AvatarUrl?` (MaxLength 2048)
- `Status` (PageStatus enum → string: `Draft`, `Published`, `Suspended`)
- `ThemeConfig` (string JSON, see ThemeConfig schema below)
- Index: `Slug` (unique), `UserId`
- Navigations: `User`, `Links[]`, `Views[]`, `CustomDomain?`

### Link
- `Id` (Guid), `PageId` (Guid FK→Page), `Title` (MaxLength 80), `Url` (MaxLength 2048)
- `IconName?`, `IsActive`, `Order` (int, 1-based), `CreatedAt`
- Navigations: `Page`, `Clicks[]`

### Subscription
- `Id` (Guid), `UserId` (Guid 1:1 FK→AppUser), `PlanId` (int FK→Plan), `PlanPriceId?`
- `Status` (SubscriptionStatus enum → string: `Free`, `Trial`, `Active`, `Cancelled`, `Expired`)
- `BillingPeriod?` (enum → string: `Monthly`, `Quarterly`, `Annual`)
- `TrialStartedAt?`, `TrialEndsAt?`, `CurrentPeriodStart?`, `CurrentPeriodEnd?`

### Plan
- `Id` (int auto), `Name` (string: "Free"/"Pro"/"Business"), `IsActive`
- Navigations: `Limit` (1:1 PlanLimit), `Prices[]`, `Subscriptions[]`, `TrialUsages[]`

### PlanLimit
- `PlanId` (1:1 FK→Plan), `MaxPages`, `MaxLinksPerPage`, `AnalyticsDays` — **-1 = unlimited**
- `AllowReferrer`, `AllowCustomDomain`, `AllowRemoveBranding` (bool)
- `ThemeCount`, `FontCount` — **-1 = all**
- **Free**: 1 page, 3 links, 7d, no referrer/custom domain/branding, 2 themes, 2 fonts
- **Pro**: 3 pages, 5 links, 365d, referrer+custom domain, all themes/fonts
- **Business**: unlimited everything + remove branding

### PlanPrice
- `PlanId`, `BillingPeriod`, `PricePerMonth` (decimal 10,2), `TotalCharged` (10,2), `DiscountPercent`
- Pro: R$19/mês, R$16/mês (tri), R$14/mês (anual)
- Business: R$75/mês, R$64/mês (tri), R$56/mês (anual)

### TrialUsage
- `UserId` + `PlanId` = unique constraint (prevent re-trial same plan)
- `Status` (TrialStatus: `Active`, `Expired`), `StartedAt`

### ClickEvent
- `LinkId` (FK), `Timestamp`, `UserAgentSummary?` (truncated to 150 chars), `Referrer?`
- Index: `(LinkId, Timestamp)`

### PageView
- `PageId` (FK), `Timestamp`, `Referrer?`
- Index: `(PageId, Timestamp)`

### CustomDomain
- `PageId` (FK→Page), `Domain` (unique index), `CnameTarget` ("cname.umblink.com")
- `Status` (DomainStatus: `Pending`, `Active`, `Failed`)

### AuditLog
- `UserId` (FK), `Action` (string e.g. "page.create"), `Metadata?` (JSON serialized), `CreatedAt`

---

## 3. DbContext

- Arquivo: `src/UmbLink.Infrastructure/Data/AppDbContext.cs`
- Herda: `IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>`
- DbSets: `Plans`, `PlanLimits`, `PlanPrices`, `Subscriptions`, `TrialUsages`, `Pages`, `Links`, `ClickEvents`, `PageViews`, `CustomDomains`, `AuditLogs`
- Configurações especiais (Fluent API em `OnModelCreating`):
  - Todos os enums → string via `HasConversion<string>()`
  - `PlanPrice.PricePerMonth` / `TotalCharged` → `HasPrecision(10, 2)`
  - Índices compostos: `ClickEvent(LinkId, Timestamp)`, `PageView(PageId, Timestamp)`
  - Unique: `Page.Slug`, `CustomDomain.Domain`, `TrialUsage(UserId, PlanId)`
- Connection string key: `ConnectionStrings:Default` (padrão: `Data Source=umblink.db`)
- Provider: SQLite (dev e prod atual; sem Postgres configurado)
- **Auto-migrate + seed no startup** em `Program.cs` via `db.Database.Migrate()` + `DbSeeder.SeedAsync()`

---

## 4. Repositórios (Infrastructure layer)

| Interface | Implementação | Responsabilidade |
|-----------|--------------|------------------|
| `IPageRepository` | `PageRepository` | CRUD Page, slug check, counts, published lookup |
| `ILinkRepository` | `LinkRepository` | CRUD Link, reorder (max order), user-owned checks |
| `ISubscriptionRepository` | `SubscriptionRepository` | Sub CRUD, trial usage, plan/price lookups |
| `IAnalyticsRepository` | `AnalyticsRepository` | AddClick/View, aggregated metrics, global counts |
| `IAuditLogRepository` | `AuditLogRepository` | Append-only audit log |
| `IPlanLimitRepository` | `PlanLimitRepository` | PlanLimit lookups by userId/planId, page/link counts |

---

## 5. Serviços — Responsabilidades

| Serviço | Responsabilidade | Dependências-chave |
|---------|-----------------|-------------------|
| `PageService` | CRUD de páginas, slug availability, cache invalidation | `IPageRepository`, `ILinkRepository`, `IPlanLimitService`, `IAuditService`, `ICacheService` |
| `LinkService` | CRUD de links, reorder, toggle active, SSRF URL validation | `ILinkRepository`, `IPageRepository`, `IPlanLimitService`, `ICacheService` |
| `PlanLimitService` | Verificar limites de plano (pages/links/features), retornar `Result<bool>` | `IPlanLimitRepository` |
| `SubscriptionService` | Start trial, activate plan, cancel (downgrade + suspend excess), process expired trials | `ISubscriptionRepository`, `IPageRepository`, `ILinkRepository`, `IAuditService`, `ICacheService` |
| `MetricsService` | Track views/clicks via background queue (non-blocking), aggregate metrics | `IBackgroundTaskQueue`, `IPageRepository`, `IAnalyticsRepository` |
| `AdminService` | User list/search, suspend/reactivate, change plan, global stats | `ISubscriptionRepository`, `IAnalyticsRepository`, `IAuditService`, `UserManager<AppUser>` |
| `AuditService` | Append audit entries with JSON metadata | `IAuditLogRepository` |
| `GroqService` | AI profile generation (title/bio/slug) via Groq API (llama-3.3-70b) | `HttpClient`, `IConfiguration` |

### Regras de negócio não-óbvias
- **Slug regex**: `^[a-z0-9-]+$`, 3–30 chars (validated by FluentValidation + service double-check)
- **URL SSRF protection** em `LinkService.ValidateUrl`: bloqueia localhost, 127.x.x.x, 10.x, 192.168.x, 172.16-31.x, credenciais embutidas; só permite `https`, `http`, `mailto`
- **Redirect warning**: links para domínios não-conhecidos redirecionam via `/redirect-warning?url=...` (lista em `RedirectKnownDomains.cs`)
- **Downgrade on cancel**: `SubscriptionService.CancelAsync` suspende páginas excedentes e desativa links excedentes automaticamente
- **Trial lock**: `TrialUsage` com unique `(UserId, PlanId)` impede re-trial; trial dura 7 dias
- **Metrics off-thread**: `MetricsService` enfileira tasks no `BackgroundTaskQueue` (Channel unbounded), processado por `MetricsBackgroundService`
- **PlanLimit fallback**: se userId não tem subscription, usa plano Free com limites hardcoded
- **Feature flags** via `Feature` enum: `CustomDomain`, `RemoveBranding`, `Referrer`, `AdvancedThemes`, `AdvancedFonts`, `AdvancedColors`
- **Avatar upload**: resize para 400×400 WebP (center crop), max 2MB, magic bytes validation (JPEG/PNG/WebP), salvo em `wwwroot/uploads/avatars/{userId}_{timestamp}.webp`
- **Background image upload**: endpoint `/api/upload/background?pageId=`, requer `pageId` pertencer ao usuário autenticado

---

## 6. Injeção de dependência (Program.cs)

```
AddRazorComponents().AddInteractiveServerComponents()
AddDbContext<AppDbContext> (SQLite, conn string "Default")
AddIdentity<AppUser, IdentityRole<Guid>> (RequireDigit, Length>=8, no RequiredNonAlpha, no RequireConfirmedAccount)
AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomUserClaimsPrincipalFactory>
AddCascadingAuthenticationState
AddAuthentication().AddGoogle (só se Google:ClientId não vazio)
AddAuthorization → policy "AdminOnly" (RequireRole("Admin"))
AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>
AddHostedService<MetricsBackgroundService>
Repositories: AddScoped (todos 6)
Services: AddScoped (todos 7 serviços de application)
Cache: Redis (ConnectionStrings:Redis) → RedisCacheService; senão IMemoryCache → MemoryCacheService (ambos Singleton)
AddValidatorsFromAssemblyContaining<CreatePageRequestValidator>
AddRateLimiter "tracking" → FixedWindow 30 req/min, queue 0
AddHttpContextAccessor, AddHttpClient, AddHttpClient<GroqService>
```

**Middleware pipeline (ordem):**
1. ExceptionHandler / HSTS (prod)
2. UseHttpsRedirection
3. UseStaticFiles
4. UseRateLimiter
5. UseAuthentication
6. UseAuthorization
7. UseAntiforgery
8. Custom middleware: bloqueia `/admin` retornando 403 se não autenticado ou não Admin role

---

## 7. Páginas e rotas Blazor

| Rota | Arquivo | Layout | Auth | Propósito |
|------|---------|--------|------|-----------|
| `/` | `Pages/Index.razor` | PublicLayout | – | Landing page pública, hero interativo |
| `/auth/login` | `Pages/Auth/Login.razor` | PublicLayout | – | Login email+senha, Google OAuth opcional |
| `/auth/register` | `Pages/Auth/Register.razor` | PublicLayout | – | Cadastro nouveau utilisateur |
| `/auth/forgot-password` | `Pages/Auth/ForgotPassword.razor` | PublicLayout | – | Solicitar reset de senha |
| `/auth/reset-password` | `Pages/Auth/ResetPassword.razor` | PublicLayout | – | Redefinir senha via token |
| `/onboarding` | `Pages/Onboarding/Wizard.razor` | PublicLayout | ✓ | Wizard 4 passos: perfil → template → AI/manual → publicar |
| `/dashboard` | `Pages/Dashboard/Index.razor` | MainLayout | ✓ | Lista de páginas do usuário |
| `/editor/{pageId:guid}` | `Pages/Dashboard/Editor.razor` | MainLayout | ✓ | Editor de links + tema + preview |
| `/dashboard/metrics/{pageId:guid}` | `Pages/Dashboard/Metrics.razor` | MainLayout | ✓ | Métricas de visualizações/cliques (Chart.js) |
| `/editor/{pageId:guid}/domain` | `Pages/Dashboard/CustomDomain.razor` | MainLayout | ✓ | Slug personalizado (sem DNS/verification real) |
| `/plans` | `Pages/Plans/Index.razor` | MainLayout | – | Listagem de planos e preços |
| `/plans/checkout` | `Pages/Plans/Checkout.razor` | – | ✓ | Checkout **simulado** (sem gateway real) |
| `/my-plan` ou `/plans/my` | `Pages/Plans/MyPlan.razor` | MainLayout | ✓ | Plano atual + uso + cancelamento |
| `/admin` | `Pages/Admin/Index.razor` | AdminLayout | AdminOnly | Stats gerais (usuários, páginas, cliques, receita) |
| `/admin/users` | `Pages/Admin/Users.razor` | AdminLayout | AdminOnly | Lista e gerencia usuários |
| `/p/{slug}` | `Pages/Public/ProfilePage.razor` | PublicLayout | – | Página pública do link-in-bio |
| `/redirect-warning` | `Pages/Public/RedirectWarning.razor` | PublicLayout | – | Aviso antes de redirecionar para URL desconhecida |

### Minimal API endpoints (Program.cs)
- `POST /auth/do-login` — login por form, redireciona para /dashboard
- `POST /auth/do-register` — registro, cria subscription Free, redireciona /onboarding
- `POST /auth/logout` — sign out
- `GET /auth/google-login` → `GET /auth/google-callback` — OAuth Google
- `POST /api/track/view/{pageId:guid}` — tracking de visualização (rate limited)
- `POST /api/track/click/{linkId:guid}` — tracking de clique (rate limited)
- `GET /r/{linkId:guid}` — redirect rastreado (known domain direto; desconhecido → /redirect-warning)
- `POST /api/upload/avatar` — upload de avatar (400×400 WebP, 2MB)
- `POST /api/upload/background?pageId=` — upload de imagem de fundo

### Componentes compartilhados
- `ConfirmModal.razor` — modal de confirmação genérico com `EventCallback<bool>`
- `ToastContainer.razor` — notificações toast
- `PlanLimitGuard.razor` — bloqueia UI quando limite atingido, exibe `UpgradeModal`
- `UpgradeModal.razor` — modal de upgrade de plano
- `PagePreview.razor` — preview ao vivo do link-in-bio no editor
- `PlanBadge.razor` — badge colorido pelo nome do plano
- `TrialBanner.razor` — banner de trial expirando (mostra dias restantes)
- `UsageMeter.razor` — barra de progresso de uso (páginas/links)
- `LoadingSpinner.razor` — spinner genérico com param `Loading`
- `LinkSuggestionsPanel.razor` — atalhos de plataformas (Instagram, TikTok, etc.)
- `RedirectToLogin.razor` — redireciona para /auth/login se não autenticado

---

## 8. ThemeConfig JSON schema

```json
{
  "themeId": "minimal-light",
  "bgColor": "#ffffff",
  "buttonStyle": "outline|rounded|pill|flat",
  "buttonColor": "#111111",
  "buttonTextColor": "#ffffff",
  "textColor": "#111111",
  "titleFont": "inter",
  "linkFont": "inter",
  "spacing": "compact|normal|relaxed",
  "avatarShape": "circle|square|rounded",
  "bgImageUrl": "/uploads/backgrounds/..."
}
```

12 temas definidos em `ThemeDefinitions.cs`: 4 free (minimal-light, dark-pro, pastel-dream, earth-tone), 8 premium.
6 templates em `Templates.All`: creator, dev-portfolio, business, artist, musician, minimalist.

---

## 9. Autenticação e autorização

- Provider: ASP.NET Core Identity (cookie-based), Identity com `AppUser`/`IdentityRole<Guid>`
- Google OAuth: ativo somente se `Google:ClientId` configurado (opcional)
- `CustomUserClaimsPrincipalFactory`: adiciona claims customizados ao principal
- `ClaimsPrincipalExtensions.GetUserId()`: extrai Guid do `ClaimTypes.NameIdentifier`
- Política padrão: sem auth global (páginas públicas acessíveis); `@attribute [Authorize]` nas páginas protegidas
- Política `AdminOnly`: `RequireRole("Admin")`
- Middleware extra: `/admin/*` retorna 403 se não Admin (dupla proteção além do [Authorize])
- Roles: `Admin`, `User`
- Lockout: ativado no `PasswordSignInAsync` (lockoutOnFailure: true)

---

## 10. Cache

- Tipo: `IMemoryCache` (default) ou Redis (`StackExchangeRedis`) se `ConnectionStrings:Redis` estiver definido
- Implementações: `MemoryCacheService` / `RedisCacheService` (ambos `ICacheService`, Singleton)
- **Limitação IMemoryCache**: `RemoveByPrefixAsync` é no-op (IMemoryCache não suporta prefix delete)
- Cache keys (em `CacheKeys.cs`):
  - `page:slug:{slug}` — PageDto publicada, TTL 10 min
  - `page:links:{pageId}` — `List<LinkDto>`, TTL 10 min
  - `user:sub:{userId}` — SubscriptionDto, TTL 5 min
- Invalidação: explícita nos services após mutações

---

## 11. Configuração e variáveis de ambiente

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=umblink.db",
    "Redis": "(opcional)"
  },
  "Google": {
    "ClientId": "(opcional para OAuth)",
    "ClientSecret": "(opcional para OAuth)"
  },
  "Groq": {
    "ApiKey": "(opcional para geração de perfil por IA)"
  }
}
```

### Variáveis obrigatórias em produção (Railway)
- `ConnectionStrings__Default` — connection string do SQLite (caminho no volume)
- `ASPNETCORE_URLS` — Railway injeta automaticamente (porta 8080 no Dockerfile)
- `Google__ClientId` + `Google__ClientSecret` — opcional (OAuth Google)
- `ConnectionStrings__Redis` — opcional (ativar Redis cache)
- `Groq__ApiKey` — opcional (ativar geração de perfil por IA)

---

## 12. Deploy e infra

- Plataforma: Railway (Dockerfile)
- Build: multi-stage Docker (`mcr.microsoft.com/dotnet/sdk:8.0` → `aspnet:8.0`)
- Porta exposta: `8080`
- Banco em prod: SQLite no volume montado (caminho configurado via env var)
- Migrações: **aplicadas automaticamente no startup** (`db.Database.Migrate()`)
- Seed: **automático no startup** via `DbSeeder.SeedAsync()` (idempotente: `if (db.Plans.Any()) return`)
- Entrypoint: `dotnet UmbLink.Web.dll`

---

## 13. Features — Estado atual

### ✅ Implementadas e funcionais
- Cadastro/Login email+senha com lockout
- Login Google OAuth (condicional por config)
- Onboarding wizard 4-passos (perfil type → template → AI/manual profile → preview/publicar)
- CRUD completo de páginas (create, update, delete, publish/unpublish)
- CRUD completo de links com reorder (drag via SortableJS), toggle active
- Editor de tema visual (12 temas, buttonStyle, spacing, avatarShape, cores customizadas)
- Upload de avatar (400×400 WebP, resize automático, validação magic bytes)
- Upload de imagem de fundo de página
- Pré-visualização ao vivo no editor (componente `PagePreview`)
- Página pública `/p/{slug}` com tracking de visualização
- Redirect rastreado `/r/{linkId}` com aviso para domínios desconhecidos
- Métricas: visualizações diárias, cliques por link, CTR, Chart.js
- Sistema de planos (Free/Pro/Business) com limites aplicados em tempo real
- Trial de 7 dias (por plano, não reutilizável)
- Cancelamento de plano com downgrade automático (suspensão de páginas/links)
- Painel Admin: stats globais, gestão de usuários, suspend/reactivate, change plan
- Audit log (ações: page.create/delete, subscription.*, admin.*)
- Rate limiting em endpoints de tracking (30 req/min)
- SSRF protection nas URLs de links
- Campo de slug personalizado com validação de disponibilidade em tempo real
- Geração de perfil por IA (Groq llama-3.3-70b) no onboarding

### 🔶 Parcialmente implementadas
- **Checkout/Pagamento** (`/plans/checkout`): UI de cartão implementada, mas `Confirm()` usa `Task.Delay(2000)` simulando processamento — **sem gateway de pagamento real**
- **Custom Domain** (`/editor/{id}/domain`): UI de slug personalizado funcional, mas custom domain real (DNS/CNAME verification) não está implementado — `CnameTarget` = "cname.umblink.com" hardcoded, sem verificação DNS
- **Remove Branding**: `AllowRemoveBranding` existe em PlanLimit e Business plan permite, mas `_removeBranding` em `ProfilePage.razor` tem `// TODO: verificar com subscriptiondto`
- **ForgotPassword/ResetPassword**: páginas existem mas fluxo real de email não verificado (Identity token presente, mas smtp não configurado)

### ❌ Não implementadas / ausentes
- Gateway de pagamento real (Stripe, PagSeguro, etc.)
- Verificação DNS para domínios customizados
- Envio de e-mail (confirmação de conta, reset de senha)
- Webhook de renovação/expiração de subscription
- `ProcessExpiredTrialsAsync` existe em `SubscriptionService` mas não há scheduled job que chame periodicamente (MetricsBackgroundService só processa analytics queue)

---

## 14. Fluxos end-to-end funcionais

### Cadastro e onboarding
1. GET `/auth/register` → POST `/auth/do-register` → cria AppUser + Subscription Free + role "User"
2. Redireciona `/onboarding` → wizard: escolhe perfil type → template → preenche title/bio/slug (ou usa IA Groq) → cria Page via `PageService.CreateAsync` + adiciona links demo via `LinkService.AddAsync` → publica → redireciona `/dashboard`

### Criar e publicar página
1. Dashboard → "Nova página" → form → `PageService.CreateAsync` (verifica limite, slug único, aplica template ThemeConfig)
2. `/editor/{pageId}` → editar title/bio/avatar/tema/links → salvar → `PageService.UpdateAsync` + `LinkService.*`
3. Publish toggle → `PageService.PublishAsync` → muda status `Draft`↔`Published` → invalida cache

### Visualização pública e tracking
1. GET `/p/{slug}` → `PageService.GetBySlugAsync` (cache 10min) → renderiza ProfilePage
2. JS `fetch('/api/track/view/{pageId}')` na montagem do componente → `MetricsService.TrackViewAsync` → enfileira no `BackgroundTaskQueue`
3. Clique em link → GET `/r/{linkId}` → verifica known domain → redireciona (ou aviso) → `MetricsService.TrackClickAsync`

### Upgrade de plano
1. `/plans` → escolhe plano → "Testar grátis" → `SubscriptionService.StartTrialAsync` (checa TrialUsage, 7 dias)
2. ou "Assinar" → `/plans/checkout` → preenche cartão → `SubscriptionService.ActivatePlanAsync` (simula pagamento)

---

## 15. Dívidas técnicas e TODOs

- `ProfilePage.razor:86` — `_removeBranding; // TODO: verificar com subscriptiondto` — campo não lido da subscription
- `Checkout.razor` — `Task.Delay(2000)` simula pagamento; sem integração real
- `MemoryCacheService.RemoveByPrefixAsync` — no-op, documentado em comentário
- `SubscriptionService.ProcessExpiredTrialsAsync` — método existe, mas **não é chamado por nenhum scheduled job**; trials expirados não são processados automaticamente
- Nenhum email SMTP configurado (ForgotPassword/ResetPassword páginas existem, fluxo incompleto)
- `AdminStatsDto.SimulatedRevenue` — receita calculada como soma de preços mensais, não receita real
- Sem testes para PageService, LinkService, SubscriptionService, GroqService (apenas PlanLimitServiceTests)

---

## 16. Seed data (desenvolvimento/demo)

**Credenciais demo** (senha padrão: `Demo@1234`):
- `admin@umblink.com` — role Admin, plan Business
- `joao@exemplo.com` — role User, plan Pro (trial)
- `maria@exemplo.com` — role User, plan Free
- `carlos@exemplo.com` — role User, plan Free

Seed é idempotente: `if (db.Plans.Any()) return` — roda apenas na primeira migração.

---

## 17. Convenções do projeto

- **Nomenclatura**: PascalCase para classes/métodos/propriedades; camelCase para parâmetros/locais; prefixo `_` para campos privados em `.razor`
- **Primary constructors** em todos os services: `public class FooService(IFooRepo repo, ICacheService cache)`
- **Result<T>**: padrão de retorno em services — `Result<T>.Ok(value)`, `Result<T>.Fail("msg")`, `Result<T>.LimitExceeded(err)` com `LimitExceededError`
- **Validação**: FluentValidation para requests (validators em `Requests/`) + validação duplicada no service para segurança
- **Enums**: stringly-typed no banco via `HasConversion<string>()`
- **Blazor state**: local por componente (campos `_privados`); sem store global
- **CSS/estilo**: Bootstrap 5.3.3 CDN + Bootstrap Icons 1.11.3 CDN + `site.css` custom + estilos inline; CSS variables (`var(--bg-primary)`, `var(--accent)`, `var(--border)` etc.) para theming light/dark
- **JavaScript**: `wwwroot/js/app.js`, usado via `IJSRuntime` no Editor e Index (SortableJS, Chart.js, gradiente interativo do hero)
- **Namespaces**: `UmbLink.<Layer>.<Feature>`

---

## 18. Comandos essenciais

```bash
# Build
dotnet build

# Run (dev)
dotnet run --project src/UmbLink.Web/UmbLink.Web.csproj

# Testes
dotnet test

# Nova migration
dotnet ef migrations add <Nome> --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj

# Aplicar migrations
dotnet ef database update --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj
```

### Adicionar nova entidade
1. Criar `src/UmbLink.Infrastructure/Data/Entities/MinhaEntidade.cs`
2. Adicionar DbSet em `AppDbContext`
3. Configurar em `OnModelCreating` se necessário (índices, conversões)
4. `dotnet ef migrations add AddMinhaEntidade ...`
5. Criar interface `IMinhaEntidadeRepository` em `Infrastructure/Repositories/`
6. Implementar `MinhaEntidadeRepository`
7. Criar serviço em `Application/Services/`, interface em `Application/Interfaces/`
8. Registrar no `Program.cs` (`AddScoped`)

### Adicionar nova página Blazor
1. Criar `src/UmbLink.Web/Pages/<Area>/MinhaPage.razor`
2. Adicionar `@page "/rota"` e `@layout <Layout>`
3. Se requer auth: `@attribute [Authorize]`
4. Injetar serviços com `@inject`
5. Adicionar link de navegação em `MainLayout.razor` ou `AdminLayout.razor` se necessário

---

## 19. NuGet packages relevantes

| Pacote | Projeto | Uso |
|--------|---------|-----|
| `Microsoft.EntityFrameworkCore.Sqlite` | Infrastructure | Provider SQLite |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Infrastructure | Identity + EF |
| `FluentValidation.AspNetCore` | Web | Validação de requests |
| `Microsoft.AspNetCore.Authentication.Google` | Web | OAuth Google |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Web | Redis cache opcional |
| `SixLabors.ImageSharp` | Web | Resize/crop avatar e background |
| `xunit` + `Moq` + `FluentAssertions` | UnitTests | Testes |
