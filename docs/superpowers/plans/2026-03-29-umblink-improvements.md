# UmbLink Improvements — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement 15 product improvements across infrastructure, new features, admin enhancements, UX polish, and CSS fixes.

**Architecture:** Ordered by dependency: infra entities first (UserActivityLog, PaymentMethod), then bugfixes (AI, avatar), then new features (onboarding links, trial card, account settings), then visual enhancements (dashboard, admin, header), then pure CSS fixes.

**Tech Stack:** .NET 8, Blazor Server, EF Core + SQLite, Bootstrap 5.3, CSS variables, GroqService (Groq API), SixLabors.ImageSharp

---

## File Map

| File | Action | Purpose |
|------|--------|---------|
| `src/UmbLink.Infrastructure/Data/Entities/UserActivityLog.cs` | Create | New entity for user activity logging |
| `src/UmbLink.Infrastructure/Data/Entities/PaymentMethod.cs` | Create | Mock payment method for trial flow |
| `src/UmbLink.Infrastructure/Identity/AppUser.cs` | Modify | Add navigation to UserActivityLog + PaymentMethod |
| `src/UmbLink.Infrastructure/Data/AppDbContext.cs` | Modify | Add DbSets + model config |
| `src/UmbLink.Infrastructure/Repositories/IUserActivityLogRepository.cs` | Create | Interface |
| `src/UmbLink.Infrastructure/Repositories/UserActivityLogRepository.cs` | Create | Implementation |
| `src/UmbLink.Infrastructure/Repositories/IPaymentMethodRepository.cs` | Create | Interface |
| `src/UmbLink.Infrastructure/Repositories/PaymentMethodRepository.cs` | Create | Implementation |
| `src/UmbLink.Application/Interfaces/IUserActivityLogService.cs` | Create | Service interface |
| `src/UmbLink.Application/Services/UserActivityLogService.cs` | Create | Service implementation |
| `src/UmbLink.Application/DTOs/UserActivityLogDto.cs` | Create | DTO for logs |
| `src/UmbLink.Application/DTOs/AdminDto.cs` | Modify | Add richer stats + user detail DTO |
| `src/UmbLink.Application/Interfaces/IAdminService.cs` | Modify | Add GetUserDetailAsync, SendNotificationAsync |
| `src/UmbLink.Application/Services/AdminService.cs` | Modify | Implement new methods |
| `src/UmbLink.Web/Program.cs` | Modify | Register new repos + services |
| `src/UmbLink.Web/Pages/Onboarding/Wizard.razor` | Modify | Add step 3.5 for links |
| `src/UmbLink.Web/Pages/Plans/Index.razor` | Modify | Trial requires card modal |
| `src/UmbLink.Web/Pages/Dashboard/Index.razor` | Modify | Enrich with metrics cards + activity |
| `src/UmbLink.Web/Pages/Admin/Index.razor` | Modify | Richer stats dashboard |
| `src/UmbLink.Web/Pages/Admin/Users.razor` | Modify | User actions (plan, notify, logs) |
| `src/UmbLink.Web/Pages/Auth/Login.razor` | Modify | Labels above inputs |
| `src/UmbLink.Web/Pages/Auth/Register.razor` | Modify | Labels above inputs |
| `src/UmbLink.Web/Pages/Index.razor` | Modify | Hero text color in light theme |
| `src/UmbLink.Web/Pages/AccountSettings.razor` | Create | New /account/settings page |
| `src/UmbLink.Web/Layouts/MainLayout.razor` | Modify | Footer flex fix + Admin link |
| `src/UmbLink.Web/wwwroot/css/site.css` | Modify | Modal fixed size + hero text + footer |
| `src/UmbLink.Application/Models/ThemeDefinitions.cs` | Modify | Fix background thumbnail URLs |

---

## Task 1: UserActivityLog Entity + Repository + Service

**Files:**
- Create: `src/UmbLink.Infrastructure/Data/Entities/UserActivityLog.cs`
- Create: `src/UmbLink.Infrastructure/Repositories/IUserActivityLogRepository.cs`
- Create: `src/UmbLink.Infrastructure/Repositories/UserActivityLogRepository.cs`
- Create: `src/UmbLink.Application/Interfaces/IUserActivityLogService.cs`
- Create: `src/UmbLink.Application/Services/UserActivityLogService.cs`
- Create: `src/UmbLink.Application/DTOs/UserActivityLogDto.cs`
- Modify: `src/UmbLink.Infrastructure/Identity/AppUser.cs`
- Modify: `src/UmbLink.Infrastructure/Data/AppDbContext.cs`

- [ ] **Step 1: Criar entidade UserActivityLog**

```csharp
// src/UmbLink.Infrastructure/Data/Entities/UserActivityLog.cs
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class UserActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
}
```

- [ ] **Step 2: Adicionar navigation em AppUser**

Em `src/UmbLink.Infrastructure/Identity/AppUser.cs`, adicionar logo após `AuditLogs`:
```csharp
public ICollection<UserActivityLog> ActivityLogs { get; set; } = [];
```

- [ ] **Step 3: Registrar DbSet e configurar no AppDbContext**

Em `src/UmbLink.Infrastructure/Data/AppDbContext.cs`, adicionar após `AuditLogs`:
```csharp
public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
```

E em `OnModelCreating`, adicionar índice por `(UserId, CreatedAt)`:
```csharp
builder.Entity<UserActivityLog>()
    .HasIndex(l => new { l.UserId, l.CreatedAt });
```

- [ ] **Step 4: Criar interface do repositório**

```csharp
// src/UmbLink.Infrastructure/Repositories/IUserActivityLogRepository.cs
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IUserActivityLogRepository
{
    Task AddAsync(UserActivityLog log);
    Task<List<UserActivityLog>> GetByUserIdAsync(Guid userId, int take = 50);
}
```

- [ ] **Step 5: Implementar repositório**

```csharp
// src/UmbLink.Infrastructure/Repositories/UserActivityLogRepository.cs
using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class UserActivityLogRepository(AppDbContext db) : IUserActivityLogRepository
{
    public async Task AddAsync(UserActivityLog log)
    {
        db.UserActivityLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<UserActivityLog>> GetByUserIdAsync(Guid userId, int take = 50)
    {
        return await db.UserActivityLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync();
    }
}
```

- [ ] **Step 6: Criar DTO**

```csharp
// src/UmbLink.Application/DTOs/UserActivityLogDto.cs
namespace UmbLink.Application.DTOs;

public record UserActivityLogDto(
    Guid Id,
    string Action,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt
);
```

- [ ] **Step 7: Criar interface do serviço**

```csharp
// src/UmbLink.Application/Interfaces/IUserActivityLogService.cs
using UmbLink.Application.DTOs;

namespace UmbLink.Application.Interfaces;

public interface IUserActivityLogService
{
    Task LogAsync(Guid userId, string action, string? details = null, string? ipAddress = null);
    Task<List<UserActivityLogDto>> GetLogsAsync(Guid userId, int take = 50);
}
```

- [ ] **Step 8: Implementar serviço**

```csharp
// src/UmbLink.Application/Services/UserActivityLogService.cs
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class UserActivityLogService(IUserActivityLogRepository repo) : IUserActivityLogService
{
    public async Task LogAsync(Guid userId, string action, string? details = null, string? ipAddress = null)
    {
        await repo.AddAsync(new UserActivityLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress
        });
    }

    public async Task<List<UserActivityLogDto>> GetLogsAsync(Guid userId, int take = 50)
    {
        var logs = await repo.GetByUserIdAsync(userId, take);
        return logs.Select(l => new UserActivityLogDto(l.Id, l.Action, l.Details, l.IpAddress, l.CreatedAt)).ToList();
    }
}
```

- [ ] **Step 9: Registrar no Program.cs**

Em `src/UmbLink.Web/Program.cs`, após `AddScoped<IAuditLogRepository, AuditLogRepository>()`:
```csharp
builder.Services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
```

E após `AddScoped<IAuditService, AuditService>()`:
```csharp
builder.Services.AddScoped<IUserActivityLogService, UserActivityLogService>();
```

- [ ] **Step 10: Gerar migration**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
dotnet ef migrations add AddUserActivityLog --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj
```

Expected: migration file criada em `src/UmbLink.Infrastructure/Migrations/`

- [ ] **Step 11: Build para verificar**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 12: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Infrastructure/Data/Entities/UserActivityLog.cs
git add src/UmbLink.Infrastructure/Identity/AppUser.cs
git add src/UmbLink.Infrastructure/Data/AppDbContext.cs
git add src/UmbLink.Infrastructure/Repositories/IUserActivityLogRepository.cs
git add src/UmbLink.Infrastructure/Repositories/UserActivityLogRepository.cs
git add src/UmbLink.Application/DTOs/UserActivityLogDto.cs
git add src/UmbLink.Application/Interfaces/IUserActivityLogService.cs
git add src/UmbLink.Application/Services/UserActivityLogService.cs
git add src/UmbLink.Web/Program.cs
git add src/UmbLink.Infrastructure/Migrations/
git commit -m "feat: add UserActivityLog entity, repository and service"
```

---

## Task 2: PaymentMethod Entity (para Item 9 — Trial com cartão)

**Files:**
- Create: `src/UmbLink.Infrastructure/Data/Entities/PaymentMethod.cs`
- Create: `src/UmbLink.Infrastructure/Repositories/IPaymentMethodRepository.cs`
- Create: `src/UmbLink.Infrastructure/Repositories/PaymentMethodRepository.cs`
- Modify: `src/UmbLink.Infrastructure/Identity/AppUser.cs`
- Modify: `src/UmbLink.Infrastructure/Data/AppDbContext.cs`

- [ ] **Step 1: Criar entidade PaymentMethod**

```csharp
// src/UmbLink.Infrastructure/Data/Entities/PaymentMethod.cs
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CardLastFour { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Brand { get; set; } = "Visa";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
}
```

- [ ] **Step 2: Adicionar navigation em AppUser**

Em `src/UmbLink.Infrastructure/Identity/AppUser.cs`, adicionar após `ActivityLogs`:
```csharp
public PaymentMethod? PaymentMethod { get; set; }
```

- [ ] **Step 3: Registrar no AppDbContext**

Em `AppDbContext.cs`, adicionar DbSet após `UserActivityLogs`:
```csharp
public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
```

Em `OnModelCreating`, configurar a relação 1:1:
```csharp
builder.Entity<AppUser>()
    .HasOne(u => u.PaymentMethod)
    .WithOne(p => p.User)
    .HasForeignKey<PaymentMethod>(p => p.UserId);
```

- [ ] **Step 4: Criar repositório**

```csharp
// src/UmbLink.Infrastructure/Repositories/IPaymentMethodRepository.cs
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByUserIdAsync(Guid userId);
    Task SaveAsync(PaymentMethod method);
}
```

```csharp
// src/UmbLink.Infrastructure/Repositories/PaymentMethodRepository.cs
using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class PaymentMethodRepository(AppDbContext db) : IPaymentMethodRepository
{
    public async Task<PaymentMethod?> GetByUserIdAsync(Guid userId)
        => await db.PaymentMethods.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task SaveAsync(PaymentMethod method)
    {
        var existing = await GetByUserIdAsync(method.UserId);
        if (existing is null)
            db.PaymentMethods.Add(method);
        else
        {
            existing.CardLastFour = method.CardLastFour;
            existing.CardHolder = method.CardHolder;
            existing.ExpiryMonth = method.ExpiryMonth;
            existing.ExpiryYear = method.ExpiryYear;
            existing.Brand = method.Brand;
        }
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 5: Registrar no Program.cs**

Após `AddScoped<IUserActivityLogRepository, UserActivityLogRepository>()`:
```csharp
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
```

- [ ] **Step 6: Gerar migration**

```bash
dotnet ef migrations add AddPaymentMethod --project src/UmbLink.Infrastructure/UmbLink.Infrastructure.csproj --startup-project src/UmbLink.Web/UmbLink.Web.csproj
```

- [ ] **Step 7: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Infrastructure/Data/Entities/PaymentMethod.cs
git add src/UmbLink.Infrastructure/Identity/AppUser.cs
git add src/UmbLink.Infrastructure/Data/AppDbContext.cs
git add src/UmbLink.Infrastructure/Repositories/IPaymentMethodRepository.cs
git add src/UmbLink.Infrastructure/Repositories/PaymentMethodRepository.cs
git add src/UmbLink.Web/Program.cs
git add src/UmbLink.Infrastructure/Migrations/
git commit -m "feat: add PaymentMethod entity for mock trial card flow"
```

---

## Task 3: Item 4 — Corrigir IA no onboarding (diagnóstico + logs)

**Files:**
- Modify: `src/UmbLink.Application/Services/GroqService.cs`
- Modify: `src/UmbLink.Web/Program.cs` (endpoint /api/ai/generate-profile)

O `GroqService` já tem logging básico, mas o endpoint retorna 503 sem detalhes. O problema mais comum é `appsettings.Development.json` sem a chave `Groq:ApiKey`, ou silenciamento de erros de rede.

- [ ] **Step 1: Melhorar logging no GroqService**

Substituir o método `GenerateProfileAsync` em `src/UmbLink.Application/Services/GroqService.cs`:

```csharp
public async Task<GenerateProfileResponse?> GenerateProfileAsync(string userDescription, string profileType)
{
    var apiKey = config["Groq:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        logger.LogWarning("Groq:ApiKey not configured. AI profile generation disabled.");
        return null;
    }

    var prompt = $$"""
        O usuário quer criar uma página de links estilo Linktree.
        Tipo de perfil: {{profileType}}
        Descrição do usuário: {{userDescription}}

        Retorne APENAS um JSON válido, sem markdown, sem explicação:
        {
          "title": "nome/título da página (máx 60 chars, pode ser nome + profissão)",
          "bio": "bio curta e engajante (máx 200 chars, primeira pessoa)",
          "slug": "slug-sem-espacos-e-sem-acentos (máx 30 chars, lowercase, hifens)"
        }
        """;

    var body = new
    {
        model = "llama-3.3-70b-versatile",
        messages = new[] { new { role = "user", content = prompt } },
        max_tokens = 200,
        temperature = 0.7
    };

    try
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(body);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var response = await http.SendAsync(request, cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            logger.LogWarning("Groq API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<GroqApiResponse>();
        var json = result?.Choices?[0]?.Message?.Content;
        if (string.IsNullOrEmpty(json))
        {
            logger.LogWarning("Groq API returned empty content in choices.");
            return null;
        }

        // Strip markdown code fences if present
        json = json.Trim();
        if (json.StartsWith("```")) json = json.Split('\n').Skip(1).TakeWhile(l => !l.StartsWith("```")).Aggregate((a, b) => a + "\n" + b);

        var profile = JsonSerializer.Deserialize<GenerateProfileResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return profile;
    }
    catch (TaskCanceledException)
    {
        logger.LogWarning("Groq API request timed out after 15 seconds.");
        return null;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error calling Groq API.");
        return null;
    }
}
```

- [ ] **Step 2: Melhorar resposta do endpoint no Program.cs**

Localizar o endpoint `app.MapPost("/api/ai/generate-profile", ...)` em `Program.cs` e substituir a linha:
```csharp
if (profile is null)
    return Results.StatusCode(503);
```
por:
```csharp
if (profile is null)
    return Results.Json(new { error = "Não foi possível gerar o perfil. Verifique se a chave Groq está configurada." }, statusCode: 503);
```

- [ ] **Step 3: Verificar chave em appsettings.Development.json**

Garantir que `src/UmbLink.Web/appsettings.Development.json` contenha:
```json
{
  "Groq": {
    "ApiKey": "gsk_SUA_CHAVE_AQUI"
  }
}
```
Se não estiver configurada, a IA retorna `null` silenciosamente — comportamento correto, mas agora logado.

- [ ] **Step 4: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Application/Services/GroqService.cs
git add src/UmbLink.Web/Program.cs
git commit -m "fix: improve Groq error logging and handle markdown-wrapped JSON responses"
```

---

## Task 4: Item 7 — Corrigir upload de avatar

O endpoint `/api/upload/avatar` em Program.cs usa `ms.GetBuffer()` que pode retornar bytes além do tamanho real. O fix é usar `ms.ToArray()`.

**Files:**
- Modify: `src/UmbLink.Web/Program.cs`

- [ ] **Step 1: Corrigir leitura de magic bytes no avatar upload**

Localizar em Program.cs o bloco do avatar upload onde está:
```csharp
var buffer = ms.GetBuffer();
var bytesRead = (int)Math.Min(ms.Length, 12);
```

Substituir por:
```csharp
var buffer = ms.ToArray();
var bytesRead = (int)Math.Min(buffer.Length, 12);
```

- [ ] **Step 2: Aplicar o mesmo fix no background upload**

Localizar o segundo bloco com `var buffer = ms.GetBuffer();` (endpoint `/api/upload/background`) e fazer o mesmo:
```csharp
var buffer = ms.ToArray();
var bytesRead = (int)Math.Min(buffer.Length, 12);
```

- [ ] **Step 3: Adicionar `.jpeg` ao Content-Type aceito**

No avatar upload, a linha:
```csharp
var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
```
Já inclui jpeg. Garantir que o frontend aceite também `image/jpeg`. No editor, verificar se o input accept inclui `.jpeg`.

- [ ] **Step 4: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/Program.cs
git commit -m "fix: use ms.ToArray() instead of GetBuffer() for correct magic bytes validation in uploads"
```

---

## Task 5: Item 1 — Adicionar links no onboarding (Wizard passo 3.5)

**Files:**
- Modify: `src/UmbLink.Web/Pages/Onboarding/Wizard.razor`

O Wizard tem 4 passos: 1 = perfil, 2 = template, 3 = detalhes, 4 = sucesso. Vamos inserir o passo 3.5 como `_step == 3` após criar a página (step 3 vira "detalhes", novo step 3.5 vira "links", step 4 vira "sucesso").

Estratégia: renumerar o step interno — step 4 = links, step 5 = sucesso. O progress bar sempre mostra "X de 4" mas mapeia internamente:

- [ ] **Step 1: Atualizar variáveis de estado e progress bar**

No bloco `@code`, adicionar estado dos links:
```csharp
// Links step
record InitialLink(string Title, string Url);
List<InitialLink> _initialLinks = [new("", ""), new("", ""), new("", "")];
string _linkError = "";
```

Atualizar `StepMaxWidth` para incluir step 4 e 5:
```csharp
string StepMaxWidth => _step switch { 2 => "780px", 3 or 4 => "900px", _ => "640px" };
```

Atualizar o display do progress bar para mapear steps internos → visuais (1-4):
```csharp
int VisualStep => _step switch { 4 => 3, 5 => 4, _ => _step };
int TotalVisualSteps => 4;
```

- [ ] **Step 2: Atualizar o HTML do progress bar**

Localizar:
```razor
<div class="d-flex align-items-center gap-2 mb-1">
    @for (int i = 1; i <= 4; i++)
    {
        var step = i;
        <div style="flex:1;height:4px;border-radius:4px;background:@(step <= _step ? "var(--accent)" : "var(--border)");transition:background .4s ease"></div>
    }
</div>
<p class="text-theme-secondary" style="font-size:.75rem;margin:0">Passo @_step de 4</p>
```

Substituir por:
```razor
<div class="d-flex align-items-center gap-2 mb-1">
    @for (int i = 1; i <= TotalVisualSteps; i++)
    {
        var step = i;
        <div style="flex:1;height:4px;border-radius:4px;background:@(step <= VisualStep ? "var(--accent)" : "var(--border)");transition:background .4s ease"></div>
    }
</div>
<p class="text-theme-secondary" style="font-size:.75rem;margin:0">Passo @VisualStep de @TotalVisualSteps</p>
```

- [ ] **Step 3: Atualizar os blocos de step para usar 1-5**

Mudar o bloco do sucesso de `@if (_step == 4)` para `@if (_step == 5)`.

Mudar os botões "Voltar" do Step 3 que apontam `_step = 2` (já correto) e o botão de avançar que chama `CreatePageAsync` para navegar para step 4 (links) após criar a página:

Em `CreatePageAsync`, mudar `_step = 4;` para `_step = 4;` (links step).

No Step 4 de sucesso atual (renomeado para step 5), a URL displayed permanece `_createdSlug`.

- [ ] **Step 4: Adicionar bloco HTML do Step 4 (links)**

Após o bloco `@* ───── Step 3: Page details ───── *@` e antes do bloco `@* ───── Step 4: Success ───── *@`, inserir:

```razor
@* ───── Step 4: Add initial links ───── *@
@if (_step == 4)
{
    <div class="animate-fade-in-up">
        <div class="text-center mb-4">
            <h3 style="font-weight:700;color:var(--text-primary)">Adicione seus primeiros links</h3>
            <p class="text-theme-secondary">Você pode adicionar até 3 links agora. É opcional — pode pular.</p>
        </div>

        @if (!string.IsNullOrEmpty(_linkError))
        {
            <div class="alert-theme mb-3" style="border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i> @_linkError
            </div>
        }

        @for (int i = 0; i < _initialLinks.Count; i++)
        {
            var idx = i;
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem;margin-bottom:.75rem">
                <div style="display:flex;gap:.75rem;align-items:center">
                    <div style="width:32px;height:32px;border-radius:50%;background:rgba(85,124,242,0.1);display:flex;align-items:center;justify-content:center;flex-shrink:0">
                        <span style="font-weight:700;font-size:.85rem;color:var(--accent)">@(idx + 1)</span>
                    </div>
                    <div style="flex:1;display:flex;gap:.5rem;flex-direction:column">
                        <input class="form-control-theme w-100"
                               placeholder="Título (ex: Instagram)"
                               value="@_initialLinks[idx].Title"
                               @oninput="e => _initialLinks[idx] = _initialLinks[idx] with { Title = e.Value?.ToString() ?? string.Empty }"
                               maxlength="80" />
                        <input class="form-control-theme w-100"
                               placeholder="URL (ex: https://instagram.com/seuperfil)"
                               value="@_initialLinks[idx].Url"
                               @oninput="e => _initialLinks[idx] = _initialLinks[idx] with { Url = e.Value?.ToString() ?? string.Empty }"
                               maxlength="2048" />
                    </div>
                </div>
            </div>
        }

        <div class="d-flex justify-content-between mt-4">
            <button class="btn-ghost" @onclick="() => _step = 3">
                <i class="bi bi-arrow-left me-1"></i> Voltar
            </button>
            <div class="d-flex gap-2">
                <button class="btn-ghost" @onclick="SkipLinks">
                    Pular por agora
                </button>
                <button class="btn-accent" @onclick="SaveLinksAsync" disabled="@_creatingLinks">
                    @if (_creatingLinks) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    Salvar e continuar <i class="bi bi-arrow-right ms-1"></i>
                </button>
            </div>
        </div>
    </div>
}
```

- [ ] **Step 5: Atualizar o bloco do sucesso para `@if (_step == 5)`**

Localizar `@if (_step == 4)` do bloco de sucesso e mudar para `@if (_step == 5)`.

- [ ] **Step 6: Adicionar métodos no @code**

Adicionar no `@code`:

```csharp
bool _creatingLinks;

void SkipLinks() => _step = 5;

async Task SaveLinksAsync()
{
    _linkError = "";
    var validLinks = _initialLinks
        .Where(l => !string.IsNullOrWhiteSpace(l.Title) && !string.IsNullOrWhiteSpace(l.Url))
        .ToList();

    // Validate URLs
    foreach (var link in validLinks)
    {
        if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http" && uri.Scheme != "mailto"))
        {
            _linkError = $"URL inválida: {link.Url}";
            return;
        }
    }

    if (validLinks.Count == 0)
    {
        _step = 5;
        return;
    }

    _creatingLinks = true;
    try
    {
        foreach (var link in validLinks)
        {
            await LinkSvc.AddAsync(_userId, _createdPageId, new()
            {
                Title = link.Title,
                Url = link.Url
            });
        }
        _step = 5;
    }
    catch
    {
        _linkError = "Erro ao salvar os links. Você pode adicioná-los no editor.";
    }
    finally
    {
        _creatingLinks = false;
    }
}
```

- [ ] **Step 7: Atualizar `CreatePageAsync` para ir ao passo 4 em vez de 4 (renomear para 4)**

Já que step 4 é agora links, substituir `_step = 4;` no final de `CreatePageAsync` por `_step = 4;` (sem mudança — verifica que está correto).

Verificar que `CreatePageAsync` não vai para 5. Deve ir para 4.

- [ ] **Step 8: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 9: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/Pages/Onboarding/Wizard.razor
git commit -m "feat: add optional links step to onboarding wizard (step 3.5)"
```

---

## Task 6: Item 9 — Trial exige cartão (modal com formulário)

**Files:**
- Modify: `src/UmbLink.Web/Pages/Plans/Index.razor`

- [ ] **Step 1: Adicionar estado do modal de trial**

No bloco `@code` de `Plans/Index.razor`, adicionar campos após `_trialError`:
```csharp
// Trial card modal state
bool _showTrialModal;
int _trialPlanId;
string _cardNumber = "", _cardHolder = "", _cardExpiry = "", _cardCvv = "";
string _cardError = "";
bool _processingTrial;
```

- [ ] **Step 2: Modificar botão "Trial grátis" para abrir modal**

Localizar os dois botões de trial (para plano Pro e Business):

Para Pro, substituir:
```razor
<button class="btn-ghost w-100" style="justify-content:center" @onclick="() => StartTrial(2)" disabled="@_starting">
    @if (_starting) { <span class="spinner-border spinner-border-sm me-1"></span> }
    Trial grátis (7 dias)
</button>
```
Por:
```razor
<button class="btn-ghost w-100" style="justify-content:center" @onclick="() => OpenTrialModal(2)">
    Trial grátis (7 dias)
</button>
```

Para Business, substituir o `@onclick="() => StartTrial(3)"` por `@onclick="() => OpenTrialModal(3)"`.

- [ ] **Step 3: Adicionar HTML do modal antes de `<ToastContainer>`**

Adicionar antes de `<ToastContainer @ref="_toast" />`:

```razor
@if (_showTrialModal)
{
    <div class="modal-overlay" style="position:fixed;inset:0;background:var(--modal-bg);z-index:1050;display:flex;align-items:center;justify-content:center;padding:1rem">
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:16px;width:100%;max-width:480px;min-height:400px;max-height:85vh;overflow-y:auto;padding:2rem">
            <div class="text-center mb-4">
                <div style="width:56px;height:56px;border-radius:50%;background:rgba(85,124,242,0.1);display:inline-flex;align-items:center;justify-content:center;margin-bottom:1rem">
                    <i class="bi bi-shield-check" style="font-size:1.5rem;color:var(--accent)"></i>
                </div>
                <h5 style="font-weight:700;color:var(--text-primary)">Inicie seu Trial Gratuito</h5>
                <p class="text-theme-secondary" style="font-size:.85rem">
                    Seu cartão <strong>não será cobrado</strong> durante os 7 dias de trial.<br/>
                    A cobrança só ocorre após o período terminar, e você pode cancelar a qualquer momento.
                </p>
            </div>

            @if (!string.IsNullOrEmpty(_cardError))
            {
                <div class="alert-theme mb-3" style="border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
                    <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i> @_cardError
                </div>
            }

            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Número do cartão</label>
                <input class="form-control-theme w-100" placeholder="0000 0000 0000 0000"
                       maxlength="19" @bind="_cardNumber" />
            </div>
            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Nome no cartão</label>
                <input class="form-control-theme w-100" placeholder="Nome como no cartão"
                       maxlength="50" @bind="_cardHolder" />
            </div>
            <div style="display:grid;grid-template-columns:1fr 1fr;gap:.75rem;margin-bottom:1.5rem">
                <div>
                    <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Validade (MM/AA)</label>
                    <input class="form-control-theme w-100" placeholder="MM/AA"
                           maxlength="5" @bind="_cardExpiry" />
                </div>
                <div>
                    <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">CVV</label>
                    <input class="form-control-theme w-100" placeholder="000"
                           maxlength="4" type="password" @bind="_cardCvv" />
                </div>
            </div>

            <div class="d-flex gap-2">
                <button class="btn-ghost flex-grow-1" style="justify-content:center" @onclick="CloseTrialModal" disabled="@_processingTrial">
                    Cancelar
                </button>
                <button class="btn-accent flex-grow-1" style="justify-content:center" @onclick="ConfirmTrialWithCard" disabled="@_processingTrial">
                    @if (_processingTrial) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    Iniciar meu Trial Gratuito
                </button>
            </div>
            <p style="text-align:center;font-size:.7rem;color:var(--text-muted);margin-top:.75rem">
                <i class="bi bi-lock-fill me-1"></i>Seus dados são protegidos com criptografia SSL
            </p>
        </div>
    </div>
}
```

- [ ] **Step 4: Adicionar métodos no @code**

Adicionar no `@code`:
```csharp
void OpenTrialModal(int planId)
{
    _trialPlanId = planId;
    _cardNumber = _cardHolder = _cardExpiry = _cardCvv = _cardError = "";
    _showTrialModal = true;
}

void CloseTrialModal()
{
    _showTrialModal = false;
}

async Task ConfirmTrialWithCard()
{
    _cardError = "";

    // Basic card validation
    var digits = _cardNumber.Replace(" ", "").Replace("-", "");
    if (digits.Length < 13 || digits.Length > 19 || !digits.All(char.IsDigit))
    { _cardError = "Número de cartão inválido."; return; }
    if (string.IsNullOrWhiteSpace(_cardHolder))
    { _cardError = "Informe o nome no cartão."; return; }
    if (!System.Text.RegularExpressions.Regex.IsMatch(_cardExpiry, @"^\d{2}/\d{2}$"))
    { _cardError = "Validade inválida. Use MM/AA."; return; }
    if (_cardCvv.Length < 3 || !_cardCvv.All(char.IsDigit))
    { _cardError = "CVV inválido."; return; }

    _processingTrial = true;
    try
    {
        var auth = await AuthState;
        var userId = auth.User.GetUserId();

        // Save mock payment method
        await PaymentRepo.SaveAsync(new UmbLink.Infrastructure.Data.Entities.PaymentMethod
        {
            UserId = userId,
            CardLastFour = digits[^4..],
            CardHolder = _cardHolder,
            ExpiryMonth = _cardExpiry[..2],
            ExpiryYear = _cardExpiry[3..],
            Brand = digits.StartsWith("4") ? "Visa" : digits.StartsWith("5") ? "Mastercard" : "Cartão"
        });

        var period = Enum.Parse<BillingPeriod>(_period);
        var result = await SubSvc.StartTrialAsync(userId, _trialPlanId, period);
        if (result.IsSuccess)
        {
            _sub = result.Value;
            _currentPlan = _sub!.PlanName;
            _showTrialModal = false;
            _toast?.Show("Trial ativado! Você tem 7 dias para explorar tudo.", "success");
        }
        else
        {
            _cardError = result.Error!;
        }
    }
    finally
    {
        _processingTrial = false;
    }
}
```

- [ ] **Step 5: Injetar IPaymentMethodRepository na página**

No topo do arquivo `Plans/Index.razor`, adicionar:
```razor
@inject IPaymentMethodRepository PaymentRepo
@using UmbLink.Infrastructure.Repositories
@using UmbLink.Infrastructure.Data.Entities
```

- [ ] **Step 6: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/Pages/Plans/Index.razor
git commit -m "feat: trial activation now requires card details (mock payment)"
```

---

## Task 7: Item 12 — Tela de Configurações da Conta (/account/settings)

**Files:**
- Create: `src/UmbLink.Web/Pages/AccountSettings.razor`
- Modify: `src/UmbLink.Web/Layouts/MainLayout.razor` (link no menu)

- [ ] **Step 1: Criar AccountSettings.razor**

```razor
@* src/UmbLink.Web/Pages/AccountSettings.razor *@
@page "/account/settings"
@attribute [Authorize]
@inject UserManager<AppUser> UserMgr
@inject SignInManager<AppUser> SignInMgr
@inject ISubscriptionService SubSvc
@inject NavigationManager Nav
@using Microsoft.AspNetCore.Identity
@using UmbLink.Infrastructure.Identity

<PageTitle>Configurações — UmbLink</PageTitle>

<div class="container py-4" style="max-width:720px">
    <h4 style="font-weight:700;color:var(--text-primary);margin-bottom:1.5rem">Configurações da Conta</h4>

    @* Tabs *@
    <div style="display:flex;gap:.5rem;border-bottom:1px solid var(--border);margin-bottom:1.5rem;flex-wrap:wrap">
        @foreach (var tab in new[] { ("perfil","Perfil"), ("seguranca","Segurança"), ("plano","Plano"), ("perigo","Zona de Perigo") })
        {
            <button style="background:none;border:none;padding:.5rem .75rem;font-size:.9rem;font-weight:@(_tab == tab.Item1 ? "700" : "500");
                           color:@(_tab == tab.Item1 ? "var(--accent)" : "var(--text-secondary)");
                           border-bottom:2px solid @(_tab == tab.Item1 ? "var(--accent)" : "transparent");
                           cursor:pointer;transition:all .2s"
                    @onclick="() => _tab = tab.Item1">
                @tab.Item2
            </button>
        }
    </div>

    @if (!string.IsNullOrEmpty(_successMsg))
    {
        <div class="alert-theme mb-3" style="border-color:rgba(16,185,129,0.3);background:rgba(16,185,129,0.08)">
            <i class="bi bi-check-circle" style="color:var(--success)"></i> @_successMsg
        </div>
    }
    @if (!string.IsNullOrEmpty(_errorMsg))
    {
        <div class="alert-theme mb-3" style="border-color:rgba(239,68,68,0.3);background:rgba(239,68,68,0.08)">
            <i class="bi bi-exclamation-circle" style="color:var(--danger)"></i> @_errorMsg
        </div>
    }

    <LoadingSpinner Loading="_loading">

    @* ─── Perfil ─── *@
    @if (_tab == "perfil")
    {
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.5rem">
            <h6 style="font-weight:600;color:var(--text-primary);margin-bottom:1.25rem">Perfil</h6>
            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Nome de exibição</label>
                <input class="form-control-theme w-100" @bind="_displayName" maxlength="50" />
            </div>
            <div style="margin-bottom:1.25rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">E-mail</label>
                <input class="form-control-theme w-100" value="@_email" disabled style="opacity:.6;cursor:not-allowed" />
                @if (_isOAuth) {
                    <small class="text-theme-muted">E-mail gerenciado pelo provedor Google.</small>
                }
            </div>
            <button class="btn-accent" @onclick="SaveProfileAsync" disabled="@_saving">
                @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                Salvar alterações
            </button>
        </div>
    }

    @* ─── Segurança ─── *@
    @if (_tab == "seguranca")
    {
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.5rem">
            <h6 style="font-weight:600;color:var(--text-primary);margin-bottom:.75rem">Segurança</h6>
            <p class="text-theme-secondary" style="font-size:.85rem">
                Provedor de login: <strong>@(_isOAuth ? "Google" : "E-mail e senha")</strong>
            </p>
            @if (!_isOAuth)
            {
                <div style="margin-bottom:1rem">
                    <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Senha atual</label>
                    <input type="password" class="form-control-theme w-100" @bind="_currentPassword" />
                </div>
                <div style="margin-bottom:1rem">
                    <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Nova senha</label>
                    <input type="password" class="form-control-theme w-100" @bind="_newPassword" />
                </div>
                <div style="margin-bottom:1.25rem">
                    <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Confirmar nova senha</label>
                    <input type="password" class="form-control-theme w-100" @bind="_confirmPassword" />
                </div>
                <button class="btn-accent" @onclick="ChangePasswordAsync" disabled="@_saving">
                    @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    Trocar senha
                </button>
            }
            else
            {
                <p class="text-theme-muted" style="font-size:.85rem">Gerenciamento de senha não disponível para contas Google.</p>
            }
        </div>
    }

    @* ─── Plano ─── *@
    @if (_tab == "plano")
    {
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.5rem">
            <h6 style="font-weight:600;color:var(--text-primary);margin-bottom:1rem">Plano e Faturamento</h6>
            @if (_sub is not null)
            {
                <div style="display:flex;gap:.75rem;align-items:center;margin-bottom:1rem">
                    <PlanBadge PlanName="@_sub.PlanName" />
                    <span class="text-theme-secondary" style="font-size:.85rem">
                        Status: @_sub.Status
                        @if (_sub.TrialEndsAt.HasValue && _sub.TrialEndsAt > DateTime.UtcNow)
                        {
                            <span> — Trial até @_sub.TrialEndsAt.Value.ToString("dd/MM/yyyy")</span>
                        }
                    </span>
                </div>
                <a href="/plans" class="btn-accent" style="text-decoration:none;display:inline-flex;align-items:center;gap:.4rem">
                    <i class="bi bi-arrow-up-circle"></i> Ver planos
                </a>

                <div style="margin-top:1.5rem;padding-top:1rem;border-top:1px solid var(--border)">
                    <h6 style="font-weight:600;color:var(--text-primary);font-size:.85rem;margin-bottom:.75rem">Método de pagamento</h6>
                    @if (_hasCard)
                    {
                        <div style="display:flex;align-items:center;gap:.75rem">
                            <i class="bi bi-credit-card" style="color:var(--accent);font-size:1.25rem"></i>
                            <span class="text-theme-secondary" style="font-size:.85rem">•••• •••• •••• @_cardLastFour</span>
                        </div>
                    }
                    else
                    {
                        <p class="text-theme-muted" style="font-size:.85rem">Nenhum método de pagamento cadastrado.</p>
                    }
                </div>

                <div style="margin-top:1.5rem;padding-top:1rem;border-top:1px solid var(--border)">
                    <h6 style="font-weight:600;color:var(--text-primary);font-size:.85rem;margin-bottom:.75rem">Histórico de faturas</h6>
                    <p class="text-theme-muted" style="font-size:.85rem">Nenhuma fatura disponível.</p>
                </div>
            }
        </div>
    }

    @* ─── Zona de Perigo ─── *@
    @if (_tab == "perigo")
    {
        <div style="background:var(--bg-card);border:1px solid rgba(239,68,68,0.3);border-radius:12px;padding:1.5rem">
            <h6 style="font-weight:600;color:var(--danger);margin-bottom:.5rem">Zona de Perigo</h6>
            <p class="text-theme-secondary" style="font-size:.85rem;margin-bottom:1rem">
                Excluir sua conta remove permanentemente todos os seus dados, páginas e links.
            </p>

            @if (!_confirmingDelete)
            {
                <button class="btn-ghost" style="border-color:var(--danger);color:var(--danger)" @onclick="() => _confirmingDelete = true">
                    <i class="bi bi-trash me-1"></i> Excluir minha conta
                </button>
            }
            else
            {
                <p class="text-theme-secondary" style="font-size:.85rem;margin-bottom:.75rem">
                    Digite seu e-mail para confirmar: <strong>@_email</strong>
                </p>
                <input class="form-control-theme w-100 mb-3" placeholder="Seu e-mail" @bind="_deleteConfirmEmail" />
                <div class="d-flex gap-2">
                    <button class="btn-ghost" @onclick="() => { _confirmingDelete = false; _deleteConfirmEmail = \"\"; }">Cancelar</button>
                    <button class="btn-ghost" style="border-color:var(--danger);color:var(--danger)"
                            @onclick="DeleteAccountAsync" disabled="@_saving">
                        @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                        Confirmar exclusão
                    </button>
                </div>
            }
        </div>
    }

    </LoadingSpinner>
</div>

@code {
    [CascadingParameter] Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject] IPaymentMethodRepository PaymentRepo { get; set; } = default!;

    string _tab = "perfil";
    bool _loading = true, _saving;
    string _successMsg = "", _errorMsg = "";

    // Profile
    string _displayName = "", _email = "";
    bool _isOAuth;

    // Password
    string _currentPassword = "", _newPassword = "", _confirmPassword = "";

    // Plan
    SubscriptionDto? _sub;
    bool _hasCard;
    string _cardLastFour = "";

    // Delete
    bool _confirmingDelete;
    string _deleteConfirmEmail = "";

    Guid _userId;

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState;
        _userId = auth.User.GetUserId();
        var user = await UserMgr.FindByIdAsync(_userId.ToString());
        if (user is null) { Nav.NavigateTo("/dashboard"); return; }

        _displayName = user.Name;
        _email = user.Email ?? "";
        var logins = await UserMgr.GetLoginsAsync(user);
        _isOAuth = logins.Any(l => l.LoginProvider == "Google");

        _sub = await SubSvc.GetAsync(_userId);

        var pm = await PaymentRepo.GetByUserIdAsync(_userId);
        _hasCard = pm is not null;
        _cardLastFour = pm?.CardLastFour ?? "";

        _loading = false;
    }

    async Task SaveProfileAsync()
    {
        _saving = true; _successMsg = ""; _errorMsg = "";
        var user = await UserMgr.FindByIdAsync(_userId.ToString());
        if (user is null) { _errorMsg = "Usuário não encontrado."; _saving = false; return; }
        user.Name = _displayName.Trim();
        var result = await UserMgr.UpdateAsync(user);
        if (result.Succeeded) _successMsg = "Perfil atualizado com sucesso.";
        else _errorMsg = result.Errors.First().Description;
        _saving = false;
    }

    async Task ChangePasswordAsync()
    {
        _saving = true; _successMsg = ""; _errorMsg = "";
        if (_newPassword != _confirmPassword)
        { _errorMsg = "As senhas não coincidem."; _saving = false; return; }
        var user = await UserMgr.FindByIdAsync(_userId.ToString());
        if (user is null) { _errorMsg = "Usuário não encontrado."; _saving = false; return; }
        var result = await UserMgr.ChangePasswordAsync(user, _currentPassword, _newPassword);
        if (result.Succeeded)
        {
            _successMsg = "Senha alterada com sucesso.";
            _currentPassword = _newPassword = _confirmPassword = "";
        }
        else _errorMsg = result.Errors.First().Description;
        _saving = false;
    }

    async Task DeleteAccountAsync()
    {
        if (_deleteConfirmEmail.Trim().ToLower() != _email.ToLower())
        { _errorMsg = "O e-mail informado não confere."; return; }
        _saving = true;
        var user = await UserMgr.FindByIdAsync(_userId.ToString());
        if (user is null) { _saving = false; return; }
        await UserMgr.DeleteAsync(user);
        await SignInMgr.SignOutAsync();
        Nav.NavigateTo("/");
    }
}
```

- [ ] **Step 2: Adicionar link no MainLayout.razor**

No menu desktop (dentro do `<div class="d-none d-md-flex">`), adicionar após `<a href="/my-plan">`:
```razor
<a href="/account/settings" class="nav-link-theme">Configurações</a>
```

No menu mobile (dentro do `@if (_menuOpen)`), adicionar:
```razor
<a href="/account/settings" class="nav-link-theme d-block py-2" @onclick="CloseMenu">
    <i class="bi bi-gear me-2"></i>Configurações
</a>
```

- [ ] **Step 3: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/Pages/AccountSettings.razor
git add src/UmbLink.Web/Layouts/MainLayout.razor
git commit -m "feat: add /account/settings page with profile, security, plan and danger zone tabs"
```

---

## Task 8: Item 11 — Enriquecer Dashboard do Usuário

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Index.razor`

- [ ] **Step 1: Adicionar injeções e estado necessário**

No topo do arquivo, adicionar após as injeções existentes:
```razor
@inject IMetricsService MetricsSvc
@inject IUserActivityLogService ActivitySvc
```

No `@code`, adicionar campos:
```csharp
int _totalViews30d, _totalClicks30d;
string _topPage = "", _topLink = "";
List<UserActivityLogDto> _recentActivity = [];
```

- [ ] **Step 2: Carregar dados adicionais em OnInitializedAsync**

No final do método `OnInitializedAsync` (depois de carregar `_pages` e `_sub`), adicionar:
```csharp
try
{
    var logs = await ActivitySvc.GetLogsAsync(_userId, 10);
    _recentActivity = logs;
}
catch { /* log service não crítico */ }
```

Para views/cliques, calcular somando das métricas de cada página:
```csharp
// Sum views last 30d across all pages
// GetPageMetricsAsync signature: (Guid userId, Guid pageId, int days)
foreach (var pg in _pages)
{
    try
    {
        var m = await MetricsSvc.GetPageMetricsAsync(_userId, pg.Id, 30);
        _totalViews30d += m.TotalViews;
        _totalClicks30d += m.TotalClicks;
        if (_topPage == "" && m.TotalViews > 0) _topPage = pg.Title;
    }
    catch { }
}
```

- [ ] **Step 3: Adicionar cards de métricas no topo do HTML**

Após `<TrialBanner>` e antes do `<div class="container py-4">` principal, adicionar:
```razor
<div class="container py-3" style="max-width:960px">
    <div class="row g-3 mb-2">
        <div class="col-6 col-md-3">
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem;text-align:center">
                <div style="font-size:1.5rem;font-weight:800;color:var(--accent)">@_totalViews30d</div>
                <div style="font-size:.75rem;color:var(--text-muted)">Views (30 dias)</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem;text-align:center">
                <div style="font-size:1.5rem;font-weight:800;color:var(--success)">@_totalClicks30d</div>
                <div style="font-size:.75rem;color:var(--text-muted)">Cliques (30 dias)</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem;text-align:center">
                <div style="font-size:.95rem;font-weight:700;color:var(--text-primary);white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                    @(string.IsNullOrEmpty(_topPage) ? "—" : _topPage)
                </div>
                <div style="font-size:.75rem;color:var(--text-muted)">Página + visitada</div>
            </div>
        </div>
        <div class="col-6 col-md-3">
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1rem;text-align:center">
                <div style="font-size:.95rem;font-weight:700;color:var(--text-primary)">@_pages.Count</div>
                <div style="font-size:.75rem;color:var(--text-muted)">Páginas criadas</div>
            </div>
        </div>
    </div>
</div>
```

- [ ] **Step 4: Adicionar seção "Atividade Recente" após a lista de páginas**

No final do bloco `<LoadingSpinner>`, antes de `</LoadingSpinner>`, adicionar após o `foreach` das páginas:

```razor
@if (_recentActivity.Count > 0)
{
    <div style="margin-top:2rem">
        <h6 style="font-weight:600;color:var(--text-primary);margin-bottom:.75rem">
            <i class="bi bi-clock-history me-1" style="color:var(--accent)"></i>Atividade Recente
        </h6>
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;overflow:hidden">
            @foreach (var log in _recentActivity)
            {
                <div style="padding:.75rem 1rem;border-bottom:1px solid var(--border);display:flex;justify-content:space-between;align-items:center">
                    <span style="font-size:.85rem;color:var(--text-primary)">@FormatAction(log.Action)</span>
                    <span style="font-size:.75rem;color:var(--text-muted)">@log.CreatedAt.ToString("dd/MM HH:mm")</span>
                </div>
            }
        </div>
    </div>
}
```

- [ ] **Step 5: Adicionar "Dicas" se páginas incompletas**

Após a seção de atividade recente:
```razor
@{
    var tips = new List<string>();
    if (_pages.Count == 0) tips.Add("Adicione seu primeiro link");
    if (_pages.All(p => p.Status != PageStatus.Published)) tips.Add("Publique sua página");
    if (_pages.Any(p => string.IsNullOrEmpty(p.AvatarUrl))) tips.Add("Adicione uma foto de perfil");
}
@if (tips.Count > 0)
{
    <div style="margin-top:1.5rem;background:rgba(85,124,242,0.05);border:1px solid rgba(85,124,242,0.15);border-radius:12px;padding:1.25rem">
        <h6 style="font-weight:600;color:var(--text-primary);margin-bottom:.75rem">
            <i class="bi bi-lightbulb me-1" style="color:var(--warning)"></i>Próximos passos
        </h6>
        @foreach (var tip in tips)
        {
            <div style="display:flex;align-items:center;gap:.5rem;margin-bottom:.4rem">
                <i class="bi bi-arrow-right-circle" style="color:var(--accent);font-size:.85rem"></i>
                <span style="font-size:.85rem;color:var(--text-secondary)">@tip</span>
            </div>
        }
    </div>
}
```

- [ ] **Step 6: Adicionar helper FormatAction no @code**

```csharp
static string FormatAction(string action) => action switch
{
    "page.create"     => "Página criada",
    "page.delete"     => "Página removida",
    "link.add"        => "Link adicionado",
    "link.remove"     => "Link removido",
    "subscription.trial_started" => "Trial iniciado",
    "subscription.plan_activated" => "Plano ativado",
    "AiProfileGenerated" => "Perfil gerado com IA",
    _                 => action
};
```

- [ ] **Step 7: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/Pages/Dashboard/Index.razor
git commit -m "feat: enrich user dashboard with metrics cards, recent activity and next steps tips"
```

---

## Task 9: Item 2 — Admin Dashboard funcional + gestão de usuários

**Files:**
- Modify: `src/UmbLink.Application/DTOs/AdminDto.cs`
- Modify: `src/UmbLink.Application/Interfaces/IAdminService.cs`
- Modify: `src/UmbLink.Application/Services/AdminService.cs`
- Modify: `src/UmbLink.Web/Pages/Admin/Index.razor`
- Modify: `src/UmbLink.Web/Pages/Admin/Users.razor`

- [ ] **Step 1: Ampliar AdminStatsDto e criar AdminUserDetailDto**

Em `src/UmbLink.Application/DTOs/AdminDto.cs`, substituir o conteúdo:
```csharp
using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.DTOs;

public record AdminUserDto(
    Guid Id,
    string Name,
    string Email,
    string PlanName,
    SubscriptionStatus Status,
    bool IsActive,
    DateTime CreatedAt
);

public record AdminStatsDto(
    int TotalUsers,
    int TotalPages,
    int TotalClicks,
    decimal SimulatedRevenue,
    int ActiveLast7Days,
    int ActiveLast30Days,
    Dictionary<string, int> PlanDistribution,
    int PublishedPages,
    int DraftPages
);

public record AdminUserDetailDto(
    AdminUserDto User,
    List<UserActivityLogDto> ActivityLogs
);

public record AdminNotificationDto(
    Guid Id,
    Guid UserId,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);
```

- [ ] **Step 2: Atualizar IAdminService**

```csharp
// src/UmbLink.Application/Interfaces/IAdminService.cs
using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
namespace UmbLink.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null);
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId, string justification);
    Task<AdminUserDetailDto> GetUserDetailAsync(Guid userId);
}
```

- [ ] **Step 3: Atualizar AdminService**

Em `src/UmbLink.Application/Services/AdminService.cs`, substituir o conteúdo:
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class AdminService(
    ISubscriptionRepository subRepo,
    IAnalyticsRepository analyticsRepo,
    IAuditService audit,
    IUserActivityLogService activityLog,
    UserManager<AppUser> userManager) : IAdminService
{
    public async Task<List<AdminUserDto>> GetUsersAsync(string? search = null)
    {
        var query = userManager.Users
            .Include(u => u.Subscription).ThenInclude(s => s != null ? s.Plan : null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email!.Contains(search));

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto(
                u.Id, u.Name, u.Email ?? "",
                u.Subscription != null ? u.Subscription.Plan.Name : "Free",
                u.Subscription != null ? u.Subscription.Status : SubscriptionStatus.Free,
                u.IsActive, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<AdminStatsDto> GetGlobalStatsAsync()
    {
        var users = await userManager.Users.CountAsync();
        var pages = await analyticsRepo.GetTotalPagesAsync();
        var clicks = await analyticsRepo.GetTotalClicksAsync();
        var revenue = await subRepo.GetActiveSubscriptionRevenueAsync();

        var cutoff7 = DateTime.UtcNow.AddDays(-7);
        var cutoff30 = DateTime.UtcNow.AddDays(-30);
        var active7 = await userManager.Users.CountAsync(u => u.CreatedAt >= cutoff7);
        var active30 = await userManager.Users.CountAsync(u => u.CreatedAt >= cutoff30);

        var planDist = await userManager.Users
            .Include(u => u.Subscription).ThenInclude(s => s != null ? s.Plan : null)
            .GroupBy(u => u.Subscription != null ? u.Subscription.Plan.Name : "Free")
            .Select(g => new { Plan = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Plan, x => x.Count);

        var publishedPages = await analyticsRepo.GetPublishedPagesCountAsync();
        var draftPages = pages - publishedPages;

        return new AdminStatsDto(users, pages, clicks, revenue, active7, active30, planDist, publishedPages, draftPages);
    }

    public async Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId)
    {
        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null) return Result<bool>.Fail("Usuário não encontrado.");
        user.IsActive = false;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(adminId, "admin.user_suspended", new { targetUserId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId)
    {
        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null) return Result<bool>.Fail("Usuário não encontrado.");
        user.IsActive = true;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(adminId, "admin.user_reactivated", new { targetUserId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId, string justification)
    {
        var plan = await subRepo.GetPlanByIdAsync(planId);
        if (plan is null) return Result<bool>.Fail("Plano não encontrado.");

        var sub = await subRepo.GetByUserIdAsync(targetUserId);
        if (sub is null)
        {
            sub = new Subscription { UserId = targetUserId };
            await subRepo.CreateAsync(sub);
        }

        sub.PlanId = planId;
        sub.Status = plan.Name == "Free" ? SubscriptionStatus.Free : SubscriptionStatus.Active;
        sub.UpdatedAt = DateTime.UtcNow;

        await subRepo.SaveChangesAsync();
        await audit.LogAsync(adminId, "admin.plan_changed", new { targetUserId, planId, justification });
        return Result<bool>.Ok(true);
    }

    public async Task<AdminUserDetailDto> GetUserDetailAsync(Guid userId)
    {
        var user = await userManager.Users
            .Include(u => u.Subscription).ThenInclude(s => s != null ? s.Plan : null)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var dto = user is null ? null : new AdminUserDto(
            user.Id, user.Name, user.Email ?? "",
            user.Subscription?.Plan.Name ?? "Free",
            user.Subscription?.Status ?? SubscriptionStatus.Free,
            user.IsActive, user.CreatedAt);

        var logs = await activityLog.GetLogsAsync(userId, 50);

        return new AdminUserDetailDto(dto!, logs);
    }
}
```

- [ ] **Step 4: Adicionar `GetPublishedPagesCountAsync` ao IAnalyticsRepository**

Em `src/UmbLink.Infrastructure/Repositories/IAnalyticsRepository.cs`, adicionar ao final da interface:
```csharp
Task<int> GetPublishedPagesCountAsync();
```

Em `src/UmbLink.Infrastructure/Repositories/AnalyticsRepository.cs`, adicionar o método (verificar se a classe injeta `AppDbContext db`):
```csharp
public async Task<int> GetPublishedPagesCountAsync()
    => await db.Pages.CountAsync(p => p.Status == PageStatus.Published);
```

(Verificar que `AnalyticsRepository` injeta `AppDbContext db` no construtor — se não, adicionar.)

- [ ] **Step 5: Atualizar Admin/Index.razor com stats detalhadas**

Substituir o conteúdo de `src/UmbLink.Web/Pages/Admin/Index.razor`:
```razor
@page "/admin"
@layout AdminLayout
@attribute [Authorize(Policy = "AdminOnly")]
@inject IAdminService AdminSvc

<PageTitle>Admin — UmbLink</PageTitle>

<h4 class="fw-bold mb-4">Visão Geral</h4>

<LoadingSpinner Loading="_loading">
    @if (_stats is not null)
    {
        <div class="row g-3 mb-4">
            <div class="col-sm-6 col-lg-3">
                <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <div class="fs-2 fw-bold text-primary">@_stats.TotalUsers</div>
                    <div class="text-theme-muted small">Total de usuários</div>
                </div>
            </div>
            <div class="col-sm-6 col-lg-3">
                <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <div class="fs-2 fw-bold" style="color:var(--success)">@_stats.ActiveLast7Days</div>
                    <div class="text-theme-muted small">Ativos (7 dias)</div>
                </div>
            </div>
            <div class="col-sm-6 col-lg-3">
                <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <div class="fs-2 fw-bold" style="color:var(--info)">@_stats.ActiveLast30Days</div>
                    <div class="text-theme-muted small">Ativos (30 dias)</div>
                </div>
            </div>
            <div class="col-sm-6 col-lg-3">
                <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <div class="fs-2 fw-bold" style="color:var(--warning)">R$@_stats.SimulatedRevenue.ToString("F2")</div>
                    <div class="text-theme-muted small">Receita simulada/mês</div>
                </div>
            </div>
        </div>

        <div class="row g-3 mb-4">
            <div class="col-md-6">
                <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <h6 class="fw-bold mb-3">Distribuição de planos</h6>
                    @foreach (var kv in _stats.PlanDistribution)
                    {
                        var pct = _stats.TotalUsers > 0 ? (kv.Value * 100 / _stats.TotalUsers) : 0;
                        <div class="mb-2">
                            <div class="d-flex justify-content-between small mb-1">
                                <span>@kv.Key</span><span>@kv.Value (@pct%)</span>
                            </div>
                            <div style="height:6px;background:var(--border);border-radius:3px">
                                <div style="height:6px;background:var(--accent);border-radius:3px;width:@(pct)%"></div>
                            </div>
                        </div>
                    }
                </div>
            </div>
            <div class="col-md-6">
                <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <h6 class="fw-bold mb-3">Páginas</h6>
                    <div class="d-flex gap-3">
                        <div class="text-center flex-grow-1">
                            <div class="fs-3 fw-bold" style="color:var(--success)">@_stats.PublishedPages</div>
                            <div class="small text-theme-muted">Publicadas</div>
                        </div>
                        <div class="text-center flex-grow-1">
                            <div class="fs-3 fw-bold" style="color:var(--text-secondary)">@_stats.DraftPages</div>
                            <div class="small text-theme-muted">Rascunhos</div>
                        </div>
                        <div class="text-center flex-grow-1">
                            <div class="fs-3 fw-bold">@_stats.TotalClicks</div>
                            <div class="small text-theme-muted">Cliques totais</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
</LoadingSpinner>

@code {
    AdminStatsDto? _stats;
    bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        _stats = await AdminSvc.GetGlobalStatsAsync();
        _loading = false;
    }
}
```

- [ ] **Step 6: Atualizar Admin/Users.razor com ações adicionais**

Substituir o conteúdo de `src/UmbLink.Web/Pages/Admin/Users.razor` por versão com: change plan (com justificativa), ver logs, modal de detalhes:

Manter a estrutura existente de busca/tabela, e adicionar:
- Coluna "Ações" com botão "Detalhes" (abre modal lateral)
- No modal: histórico de logs, botão "Trocar plano" com select + campo de justificativa
- Suspender/reativar como antes

```razor
@page "/admin/users"
@layout AdminLayout
@attribute [Authorize(Policy = "AdminOnly")]
@inject IAdminService AdminSvc

<PageTitle>Usuários — Admin UmbLink</PageTitle>

<div class="d-flex justify-content-between align-items-center mb-4">
    <h4 class="fw-bold mb-0">Usuários</h4>
    <input class="form-control-theme" style="max-width:280px" placeholder="Buscar por nome ou e-mail..."
           @bind="_search" @bind:event="oninput" />
</div>

<LoadingSpinner Loading="_loading">
    <div style="overflow-x:auto">
        <table class="table" style="color:var(--text-primary)">
            <thead>
                <tr>
                    <th>Nome</th>
                    <th>E-mail</th>
                    <th>Plano</th>
                    <th>Status</th>
                    <th>Cadastro</th>
                    <th>Ações</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var u in FilteredUsers)
                {
                    <tr>
                        <td>@u.Name</td>
                        <td style="font-size:.85rem">@u.Email</td>
                        <td><PlanBadge PlanName="@u.PlanName" /></td>
                        <td>
                            <span style="font-size:.75rem;padding:.2rem .5rem;border-radius:20px;
                                background:@(u.IsActive ? "rgba(16,185,129,.15)" : "rgba(239,68,68,.15)");
                                color:@(u.IsActive ? "var(--success)" : "var(--danger)")">
                                @(u.IsActive ? "Ativo" : "Suspenso")
                            </span>
                        </td>
                        <td style="font-size:.8rem">@u.CreatedAt.ToString("dd/MM/yyyy")</td>
                        <td>
                            <div class="d-flex gap-1 flex-wrap">
                                <button class="btn-ghost" style="font-size:.75rem;padding:.2rem .5rem"
                                        @onclick="() => OpenDetail(u)">
                                    <i class="bi bi-eye"></i> Detalhes
                                </button>
                                @if (u.IsActive)
                                {
                                    <button class="btn-ghost" style="font-size:.75rem;padding:.2rem .5rem;color:var(--danger)"
                                            @onclick="() => SuspendAsync(u)">
                                        Suspender
                                    </button>
                                }
                                else
                                {
                                    <button class="btn-ghost" style="font-size:.75rem;padding:.2rem .5rem;color:var(--success)"
                                            @onclick="() => ReactivateAsync(u)">
                                        Reativar
                                    </button>
                                }
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</LoadingSpinner>

@* Detail modal *@
@if (_selectedUser is not null)
{
    <div class="modal-overlay" style="position:fixed;inset:0;background:var(--modal-bg);z-index:1050;display:flex;align-items:center;justify-content:center;padding:1rem">
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:16px;width:100%;max-width:560px;min-height:400px;max-height:85vh;overflow-y:auto;padding:1.5rem">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="fw-bold mb-0">@_selectedUser.Name</h6>
                <button class="btn-ghost" @onclick="CloseDetail"><i class="bi bi-x-lg"></i></button>
            </div>

            <p class="text-theme-secondary" style="font-size:.85rem">@_selectedUser.Email · Plano: <PlanBadge PlanName="@_selectedUser.PlanName" /></p>

            <div style="border:1px solid var(--border);border-radius:10px;padding:1rem;margin-bottom:1rem">
                <h6 style="font-size:.85rem;font-weight:600;margin-bottom:.75rem">Trocar plano</h6>
                <div style="display:flex;gap:.5rem;margin-bottom:.5rem">
                    <select class="form-control-theme flex-grow-1" @bind="_newPlanId">
                        <option value="1">Free</option>
                        <option value="2">Pro</option>
                        <option value="3">Business</option>
                    </select>
                    <button class="btn-accent" style="font-size:.8rem;padding:.35rem .75rem" @onclick="ChangePlanAsync">
                        Aplicar
                    </button>
                </div>
                <input class="form-control-theme w-100" placeholder="Justificativa..." @bind="_planJustification" />
                @if (!string.IsNullOrEmpty(_planMsg)) { <div style="font-size:.75rem;color:var(--success);margin-top:.25rem">@_planMsg</div> }
            </div>

            <div>
                <h6 style="font-size:.85rem;font-weight:600;margin-bottom:.75rem">
                    <i class="bi bi-clock-history me-1"></i>Logs de atividade
                </h6>
                @if (_detailLogs.Count == 0)
                {
                    <p class="text-theme-muted" style="font-size:.85rem">Nenhum log registrado.</p>
                }
                else
                {
                    @foreach (var log in _detailLogs)
                    {
                        <div style="padding:.5rem 0;border-bottom:1px solid var(--border);display:flex;justify-content:space-between">
                            <span style="font-size:.8rem;color:var(--text-primary)">@log.Action</span>
                            <span style="font-size:.75rem;color:var(--text-muted)">@log.CreatedAt.ToString("dd/MM HH:mm")</span>
                        </div>
                    }
                }
            </div>
        </div>
    </div>
}

<ToastContainer @ref="_toast" />

@code {
    [CascadingParameter] Task<AuthenticationState> AuthState { get; set; } = default!;

    List<AdminUserDto> _users = [];
    string _search = "";
    bool _loading = true;
    ToastContainer? _toast;

    AdminUserDto? _selectedUser;
    List<UserActivityLogDto> _detailLogs = [];
    int _newPlanId = 1;
    string _planJustification = "", _planMsg = "";

    Guid _adminId;

    IEnumerable<AdminUserDto> FilteredUsers => string.IsNullOrWhiteSpace(_search)
        ? _users
        : _users.Where(u => u.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)
                          || u.Email.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState;
        _adminId = auth.User.GetUserId();
        _users = await AdminSvc.GetUsersAsync();
        _loading = false;
    }

    async Task OpenDetail(AdminUserDto user)
    {
        _selectedUser = user;
        _newPlanId = user.PlanName switch { "Pro" => 2, "Business" => 3, _ => 1 };
        _planJustification = ""; _planMsg = "";
        var detail = await AdminSvc.GetUserDetailAsync(user.Id);
        _detailLogs = detail.ActivityLogs;
    }

    void CloseDetail() { _selectedUser = null; _detailLogs = []; }

    async Task SuspendAsync(AdminUserDto u)
    {
        var result = await AdminSvc.SuspendUserAsync(_adminId, u.Id);
        if (result.IsSuccess)
        {
            _users = await AdminSvc.GetUsersAsync();
            _toast?.Show("Usuário suspenso.");
        }
    }

    async Task ReactivateAsync(AdminUserDto u)
    {
        var result = await AdminSvc.ReactivateUserAsync(_adminId, u.Id);
        if (result.IsSuccess)
        {
            _users = await AdminSvc.GetUsersAsync();
            _toast?.Show("Usuário reativado.");
        }
    }

    async Task ChangePlanAsync()
    {
        if (_selectedUser is null) return;
        var result = await AdminSvc.ChangePlanAsync(_adminId, _selectedUser.Id, _newPlanId, _planJustification);
        if (result.IsSuccess)
        {
            _planMsg = "Plano alterado com sucesso.";
            _users = await AdminSvc.GetUsersAsync();
            var updatedUser = _users.FirstOrDefault(u => u.Id == _selectedUser.Id);
            if (updatedUser is not null) _selectedUser = updatedUser;
        }
    }
}
```

- [ ] **Step 7: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Application/DTOs/AdminDto.cs
git add src/UmbLink.Application/Interfaces/IAdminService.cs
git add src/UmbLink.Application/Services/AdminService.cs
git add src/UmbLink.Infrastructure/Repositories/IAnalyticsRepository.cs
git add src/UmbLink.Infrastructure/Repositories/AnalyticsRepository.cs
git add src/UmbLink.Web/Pages/Admin/Index.razor
git add src/UmbLink.Web/Pages/Admin/Users.razor
git commit -m "feat: enrich admin dashboard with plan distribution, page stats, and user detail modal"
```

---

## Task 10: Items 5, 8, 10, 13, 14, 15 — Correções CSS e Navbar

**Files:**
- Modify: `src/UmbLink.Web/wwwroot/css/site.css`
- Modify: `src/UmbLink.Web/Layouts/MainLayout.razor`
- Modify: `src/UmbLink.Web/Pages/Index.razor`
- Modify: `src/UmbLink.Web/Pages/Auth/Login.razor`
- Modify: `src/UmbLink.Web/Pages/Auth/Register.razor`
- Modify: `src/UmbLink.Application/Models/ThemeDefinitions.cs`

### Item 15 — Admin link no header

- [ ] **Step 1: Adicionar link Admin no MainLayout.razor**

No bloco `<div class="d-none d-md-flex">`, após `<a href="/my-plan"`:
```razor
<AuthorizeView Roles="Admin">
    <Authorized>
        <a href="/admin" class="nav-link-theme" style="color:var(--warning)">
            <i class="bi bi-shield-lock me-1"></i>Admin
        </a>
    </Authorized>
</AuthorizeView>
```

No menu mobile (`@if (_menuOpen)`), após o link `/my-plan`:
```razor
<AuthorizeView Roles="Admin">
    <Authorized>
        <a href="/admin" class="nav-link-theme d-block py-2" style="color:var(--warning)" @onclick="CloseMenu">
            <i class="bi bi-shield-lock me-2"></i>Admin
        </a>
    </Authorized>
</AuthorizeView>
```

### Item 5 — Texto da home no tema light

- [ ] **Step 2: Corrigir hero-title em site.css**

Procurar a classe `.hero-title` no `site.css`. O problema é que a cor está hardcoded como `#fff`. Localizar e garantir que o hero usa cor branca apenas porque está sempre no dark gradient background.

O hero section usa `class="hero-section hero-gradient-interactive"` e está sempre em fundo escuro (gradiente), então o texto branco é correto. O problema citado no prompt pode ser no texto fora do hero (em outras seções).

Verificar a seção `.section-title` e `.section-subtitle`. No `site.css`, garantir que estas classes usem `var(--text-primary)`.

Adicionar ao final do bloco de `.section-steps` em site.css (se existir):
```css
.section-title {
    color: var(--text-primary);
}
.section-subtitle {
    color: var(--text-secondary);
}
.step-card h5,
.feature-card h6 {
    color: var(--text-primary);
}
.step-card p,
.feature-card p {
    color: var(--text-secondary);
}
```

### Item 8 — Footer fixo (sticky ao bottom)

- [ ] **Step 3: Verificar MainLayout.razor já tem `flex-grow-1` no main**

O `MainLayout.razor` já tem:
```html
<div class="min-vh-100 d-flex flex-column" ...>
    ...
    <main class="flex-grow-1">@Body</main>
    <footer class="footer-theme">...</footer>
</div>
```

Isso já é o comportamento correto. Verificar no site.css se `.footer-theme` está corretamente estilizado sem `position:fixed`.

Adicionar/garantir em site.css:
```css
.layout-wrapper {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}
main.flex-grow-1 {
    flex: 1 0 auto;
}
.footer-theme {
    flex-shrink: 0;
}
```

### Item 10 — Modais com tamanho fixo

- [ ] **Step 4: Adicionar CSS para modais fixas em site.css**

Adicionar no `site.css`:
```css
/* Fixed-size modals */
.modal-dialog {
    min-height: 400px;
    max-height: 85vh;
}
.modal-content {
    min-height: 400px;
    max-height: 85vh;
    overflow-y: auto;
}
.modal-body {
    overflow-y: auto;
}
```

### Item 13 — Labels acima dos inputs

- [ ] **Step 5: Padronizar Login.razor**

Em `Login.razor`, o campo E-mail já tem label acima. Verificar que está tudo padronizado — já está correto (labels já estão acima dos inputs). Apenas garantir espaçamento:

Adicionar em `site.css`:
```css
.form-label, .form-label-theme {
    display: block;
    margin-bottom: 4px;
    font-weight: 500;
    font-size: .875rem;
    color: var(--text-primary);
}
```

### Item 14 — Thumbnails de background com melhor qualidade

- [ ] **Step 6: Atualizar ThemeDefinitions.cs com ThumbUrl**

O seletor de background usa URLs de Unsplash. Verificar em `Editor.razor` como são exibidas as imagens. Por ora, garantir que os temas premium que usam background images os tenham com `object-fit:cover`.

Em `site.css`, adicionar:
```css
/* Background image thumbnails */
.bg-thumb {
    width: 100%;
    height: 80px;
    object-fit: cover;
    border-radius: 8px;
    border: 2px solid var(--border);
    cursor: pointer;
    transition: border-color .2s;
}
.bg-thumb:hover,
.bg-thumb.selected {
    border-color: var(--accent);
}
```

- [ ] **Step 7: Build**

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
cd "C:/Users/linck/OneDrive/Desktop/hackaton project"
git add src/UmbLink.Web/wwwroot/css/site.css
git add src/UmbLink.Web/Layouts/MainLayout.razor
git add src/UmbLink.Web/Pages/Index.razor
git add src/UmbLink.Web/Pages/Auth/Login.razor
git add src/UmbLink.Web/Pages/Auth/Register.razor
git commit -m "fix: CSS fixes for footer sticky, modal fixed size, label standardization, admin nav link, text contrast"
```

---

## Checklist de Finalização

- [ ] Nenhum `Console.WriteLine` ou `console.log` de debug deixado no código
- [ ] `IUserActivityLogRepository` e `UserActivityLogRepository` registrados no DI
- [ ] `IUserActivityLogService` e `UserActivityLogService` registrados no DI
- [ ] `IPaymentMethodRepository` e `PaymentMethodRepository` registrados no DI
- [ ] `AdminService` agora depende de `IUserActivityLogService` — verificar que o DI não quebra
- [ ] Migrations geradas para `UserActivityLog` e `PaymentMethod`
- [ ] `IAdminService.ChangePlanAsync` assinatura atualizada de 3 para 4 parâmetros — não há outros callers além de `Admin/Users.razor`
- [ ] As rotas novas (`/account/settings`) estão protegidas por `@attribute [Authorize]`
- [ ] O sistema compila sem warnings críticos
- [ ] Build final limpo: `dotnet build`

```bash
dotnet build "C:/Users/linck/OneDrive/Desktop/hackaton project/src/UmbLink.Web/UmbLink.Web.csproj"
```
