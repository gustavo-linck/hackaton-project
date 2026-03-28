# 1. Visão Geral do Projeto
UmbLink é uma plataforma de “link na bio” onde usuários criam páginas públicas com links, personalização visual, métricas de acesso e planos de assinatura (Free, Pro, Business). O sistema inclui autenticação (email/senha e Google), painel do usuário, área administrativa e rastreamento de visualizações/cliques.

**Stack confirmada:**
- **.NET 8 / C#** (`net8.0` em todos os `.csproj`)
- **Blazor Server** (confirmado por `AddInteractiveServerComponents()`, `AddInteractiveServerRenderMode()` e script `_framework/blazor.server.js` em `App.razor`)
- **EF Core + Migrations** (`AppDbContext`, pasta `src/UmbLink.Infrastructure/Migrations`, `Database.Migrate()` no startup)
- **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite` no `UmbLink.Infrastructure.csproj` e `UseSqlite(...)` em `Program.cs`)
- **Bootstrap utilities/components** (CDN `bootstrap@5.3.3` em `App.razor`, uso extenso de classes utilitárias)

# 2. Arquitetura e Estrutura de Pastas
## Padrão arquitetural identificado
Arquitetura em camadas com separação por projetos (`Web`, `Application`, `Infrastructure`) + testes (`UnitTests`), com UI em Blazor e endpoints HTTP mínimos no `Program.cs`.

⚠️ **Inconsistência encontrada:** o projeto se aproxima de Clean Architecture, mas `UmbLink.Application` referencia `UmbLink.Infrastructure` diretamente (dependência invertida para o padrão Clean estrito).

## Projetos/assemblies da solution
- `src/UmbLink.Web/UmbLink.Web.csproj`
- `src/UmbLink.Application/UmbLink.Application.csproj`
- `src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj`
- `tests/UmbLink.UnitTests/UmbLink.UnitTests.csproj`

## Estrutura real mapeada
- `src/UmbLink.Web`
  - `Pages/` (`Auth`, `Dashboard`, `Plans`, `Admin`, `Public`, `Onboarding`)
  - `Components/` (ex.: `ConfirmModal.razor`, `ToastContainer.razor`, `PlanLimitGuard.razor`)
  - `Layouts/` (`MainLayout.razor`, `AdminLayout.razor`, `PublicLayout.razor`)
  - `Program.cs`, `App.razor`, `Routes.razor`, `_Imports.razor`
  - `wwwroot/js/app.js`, `wwwroot/css/...`
- `src/UmbLink.Application`
  - `Services/`, `Interfaces/`, `DTOs/`, `Requests/`, `Models/`, `BackgroundServices/`
- `src/UmbLink.Infrastructure`
  - `Data/AppDbContext.cs`
  - `Data/Entities/...`
  - `Identity/...`
  - `Migrations/...`
  - `Seed/DbSeeder.cs`
- `tests/UmbLink.UnitTests`
  - `PlanLimitServiceTests.cs`

## Separação de camadas
- **Models/DTOs/Requests:** `src/UmbLink.Application/Models`, `DTOs`, `Requests`
- **Services:** `src/UmbLink.Application/Services`
- **Repositório/acesso a dados:** via `AppDbContext` diretamente nos services (não há camada repository explícita)
- **Pages/Components:** `src/UmbLink.Web/Pages` e `src/UmbLink.Web/Components`
- **Data/DbContext/Entities/Migrations:** `src/UmbLink.Infrastructure/Data`, `Entities`, `Migrations`

# 3. Comandos Essenciais
Não foram encontrados `Makefile`, scripts `.sh` ou `README.md` de automação no repositório analisado.

- **Build:** `dotnet build`
- **Run (projeto web):** `dotnet run --project src/UmbLink.Web/UmbLink.Web.csproj`
- **Migrations (EF Core):**
  - `dotnet ef migrations add <Nome> --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj`
  - `dotnet ef database update --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj`
- **Testes:** `dotnet test`
- **Publish:** `[a definir]`

# 4. Padrões de Código — C# / .NET
- **Versão/recursos de linguagem em uso:**
  - `.NET 8` com `Nullable` e `ImplicitUsings` habilitados
  - `record` para DTOs (`PageDto`, `LinkDto`, `ThemeDefinition`)
  - **Primary constructors** em services (ex.: `public class LinkService(AppDbContext db, IPlanLimitService limits)`)
  - expressões `switch`, `collection expressions` (`[]`) e raw string literals (`"""..."""`)
- **Nomenclatura praticada:**
  - `PascalCase` para classes/métodos/propriedades
  - `camelCase` para parâmetros/variáveis locais
  - prefixo `_` para campos privados em componentes `.razor`
- **Namespaces:** padrão `UmbLink.<Camada>.<Feature>`
- **Async/await:** predominante em serviços e ciclo de vida Blazor (`OnInitializedAsync`, operações de dados, etc.)
- **Injeção de dependência:** centralizada em `src/UmbLink.Web/Program.cs` com `AddScoped`, `AddSingleton`, `AddHostedService`
- **Validação:** FluentValidation (`CreatePageRequestValidator`, `UpdatePageRequestValidator`, `CreateLinkRequestValidator`, etc.) registrado via `AddValidatorsFromAssemblyContaining<...>()`

# 5. Padrões de Código — Blazor
- **Tipo:** Blazor Server (justificado por configuração de renderização interativa server e `blazor.server.js`)
- **Organização dos `.razor`:** por área funcional em `Pages/*` + componentes reutilizáveis em `Components/*` + layouts em `Layouts/*`
- **Comunicação entre componentes:**
  - `EventCallback`/`EventCallback<T>` (ex.: `ConfirmModal`)
  - `@bind-Visible` em modais
  - `CascadingParameter` para `AuthenticationState`
  - serviços injetados via `@inject` / `[Inject]`
- **Gerenciamento de estado:** estado local por componente (campos privados) + persistência via serviços da camada `Application`; sem store global formal
- **Roteamento:** `@page` por arquivo com `Routes.razor` usando `Router` + `AuthorizeRouteView`; layouts por rota (`@layout`)
- **JavaScript interop:**
  - Implementações em `src/UmbLink.Web/wwwroot/js/app.js`
  - Uso em páginas como `Pages/Dashboard/Editor.razor` e `Pages/Dashboard/Index.razor` (`IJSRuntime`)

# 6. Entity Framework Core
- **DbContext:** `src/UmbLink.Infrastructure/Data/AppDbContext.cs` (`AppDbContext`)
- **Mapeamento:** predominante via **Fluent API** em `OnModelCreating` (índices únicos, relacionamentos, conversões de enum para string, precisão decimal)
- **Entidades:** `src/UmbLink.Infrastructure/Data/Entities/*` com nomes em singular (`Page`, `Link`, `Subscription`, etc.)
- **Migrations:** `src/UmbLink.Infrastructure/Migrations`
- **Seed/dados iniciais:** `src/UmbLink.Infrastructure/Seed/DbSeeder.cs` com planos, limites, preços, usuários demo, páginas, links e métricas simuladas

# 7. Bootstrap — Regras de Estilo
- Bootstrap em uso confirmado por CDN no `App.razor` (`5.3.3`) + `bootstrap-icons` (`1.11.3`)
- Padrões recorrentes observados: uso intenso de `container`, `row/col`, `card`, `btn`, `badge`, `alert`, utilitários (`d-flex`, `gap-*`, `py-*`, etc.)

⚠️ **Inconsistência encontrada:** não é “exclusivamente Bootstrap utilities/components”. Há:
- CSS custom em `src/UmbLink.Web/wwwroot/css/site.css`
- múltiplos estilos inline em páginas/componentes `.razor`

Tema visual:
- sem sistema central de tema global (light/dark) via variáveis CSS
- personalização de tema por página via `ThemeConfig` (JSON) interpretado nos componentes

# 8. Decisões Técnicas e Contexto
- **SQLite:** escolhido por simplicidade de setup local e integração direta já configurada no `Program.cs` + pacote EF SQLite
- **Blazor Server:** escolhido para UI interativa com renderização no servidor e integração direta com DI/Identity/EF no mesmo host
- **Integrações externas presentes:**
  - Google OAuth (`Microsoft.AspNetCore.Authentication.Google`)
  - ASP.NET Core Identity
  - bibliotecas front-end via CDN: Bootstrap, Bootstrap Icons, SortableJS, Chart.js
- **Bibliotecas NuGet relevantes:**
  - `FluentValidation`, `FluentValidation.AspNetCore`
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`
  - testes: `xunit`, `Moq`, `FluentAssertions`, `coverlet.collector`

⚠️ **Inconsistência encontrada:** fluxo de checkout em `Pages/Plans/Checkout.razor` simula pagamento (`Task.Delay`) e não integra gateway real.

# 9. O Que Evitar Neste Projeto
- Não introduzir SQL raw sem necessidade; padrão atual usa EF Core LINQ + Fluent API
- Não quebrar a separação em camadas existente (`Web`/`Application`/`Infrastructure`)
- Evitar duplicar lógica de tema/estilo entre componentes (`PagePreview` e `ProfilePage` já têm parsing semelhante)
- Evitar adicionar lógica de negócio diretamente em páginas `.razor` quando já existe service correspondente na camada `Application`
- Evitar endpoints sem rate limit para tracking (padrão atual protege `/api/track/*`)

⚠️ **Inconsistência encontrada:** coexistem estilos inline e `site.css`; se a diretriz for Bootstrap puro, padronizar e remover customizações fora do padrão.
⚠️ **Inconsistência encontrada:** sem comando/documentação de `publish` versionado no repositório (`[a definir]`).
