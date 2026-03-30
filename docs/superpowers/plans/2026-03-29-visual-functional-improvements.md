# Visual & Functional Improvements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement dark mode depth improvements, admin trial control, skeleton loading screens, and general UX/bug fixes across the UmbLink Blazor Server app.

**Architecture:** Pure CSS additions for visuals; new `Skeleton.razor` component replaces `LoadingSpinner` in 4 pages; backend service layer extended with `GrantTrialAsync`; all UI changes isolated to their respective `.razor` files with no structural refactoring.

**Tech Stack:** .NET 8, Blazor Server, Bootstrap 5.3.3, EF Core / SQLite, xUnit + Moq + FluentAssertions

---

## File Map

| File | Action | Purpose |
|------|--------|---------|
| `src/UmbLink.Web/wwwroot/css/site.css` | Modify | Dark page-card depth + skeleton CSS variables/keyframes |
| `src/UmbLink.Web/Components/Skeleton.razor` | **Create** | Reusable shimmer skeleton block component |
| `src/UmbLink.Application/DTOs/AdminDto.cs` | Modify | Add `TrialEndsAt`, `IsOnTrial` to `AdminUserDto` |
| `src/UmbLink.Application/Interfaces/IAdminService.cs` | Modify | Add `GrantTrialAsync` signature |
| `src/UmbLink.Application/Services/AdminService.cs` | Modify | Inject `ICacheService`, implement `GrantTrialAsync`, enrich `GetUsersAsync` |
| `tests/UmbLink.UnitTests/AdminServiceTests.cs` | **Create** | Unit tests for `GrantTrialAsync` |
| `src/UmbLink.Web/Pages/Dashboard/Index.razor` | Modify | Replace `LoadingSpinner` with skeleton |
| `src/UmbLink.Web/Pages/Admin/Index.razor` | Modify | Replace `LoadingSpinner` with skeleton + new `FormatAction` case |
| `src/UmbLink.Web/Pages/Admin/Users.razor` | Modify | Skeleton + trial column/modal + loading states + ConfirmModal for suspend |
| `src/UmbLink.Web/Pages/Dashboard/Metrics.razor` | Modify | Replace `LoadingSpinner` with skeleton |
| `src/UmbLink.Web/Pages/Plans/Checkout.razor` | Modify | Fix title/card visual separation |

---

## Task 1: CSS — Dark Mode page-card Depth + Skeleton Styles

**Files:**
- Modify: `src/UmbLink.Web/wwwroot/css/site.css` (append after last line)

- [ ] **Step 1: Append dark-mode page-card and skeleton CSS to `site.css`**

Open `site.css` and append the following at the very end of the file:

```css
/* ===== Dark Mode — Page Card Depth ===== */
[data-theme="dark"] .page-card {
    background: linear-gradient(135deg, #151c30 0%, #1a2440 100%);
}

[data-theme="dark"] .page-card:hover {
    background: linear-gradient(135deg, #1c2540 0%, #1f2d52 100%);
    box-shadow: 0 8px 28px rgba(85, 124, 242, 0.18), 0 2px 8px rgba(0, 0, 0, 0.4);
    border-color: rgba(85, 124, 242, 0.3);
}

[data-theme="dark"] .page-card-footer {
    background: rgba(0, 0, 0, 0.15);
    border-top-color: rgba(85, 124, 242, 0.1);
}

/* ===== Skeleton Loading ===== */
:root,
:root[data-theme="light"] {
    --skeleton-base: #e5e7eb;
    --skeleton-highlight: #f3f4f6;
}

:root[data-theme="dark"] {
    --skeleton-base: #1c2540;
    --skeleton-highlight: #243058;
}

@keyframes skeleton-shimmer {
    0%   { background-position: -400px 0; }
    100% { background-position:  400px 0; }
}

.skeleton-block {
    background: linear-gradient(
        90deg,
        var(--skeleton-base)      25%,
        var(--skeleton-highlight) 50%,
        var(--skeleton-base)      75%
    );
    background-size: 800px 100%;
    animation: skeleton-shimmer 2s ease-in-out infinite;
    border-radius: 6px;
    display: block;
}

.fade-in-content {
    animation: fadeIn 0.3s ease forwards;
}
```

- [ ] **Step 2: Verify build compiles**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/wwwroot/css/site.css
git commit -m "style: add dark mode page-card depth and skeleton CSS"
```

---

## Task 2: Create `Skeleton.razor` Component

**Files:**
- Create: `src/UmbLink.Web/Components/Skeleton.razor`

- [ ] **Step 1: Create the file with full content**

```razor
@* Renders one or more skeleton shimmer blocks stacked vertically.
   Auto-available via _Imports.razor (@using UmbLink.Web.Components). *@

<div style="display:flex;flex-direction:column;gap:.5rem">
    @for (int i = 0; i < Lines; i++)
    {
        <span class="skeleton-block"
              style="width:@Width;height:@Height;border-radius:@BorderRadius"></span>
    }
</div>

@code {
    [Parameter] public string Width { get; set; } = "100%";
    [Parameter] public string Height { get; set; } = "1rem";
    [Parameter] public string BorderRadius { get; set; } = "6px";
    [Parameter] public int Lines { get; set; } = 1;
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/Components/Skeleton.razor
git commit -m "feat: add Skeleton shimmer component"
```

---

## Task 3: Backend — `AdminUserDto`, `IAdminService`, `AdminService` + Tests

**Files:**
- Modify: `src/UmbLink.Application/DTOs/AdminDto.cs`
- Modify: `src/UmbLink.Application/Interfaces/IAdminService.cs`
- Modify: `src/UmbLink.Application/Services/AdminService.cs`
- Create: `tests/UmbLink.UnitTests/AdminServiceTests.cs`

- [ ] **Step 1: Write the failing tests first**

Create `tests/UmbLink.UnitTests/AdminServiceTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Services;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.UnitTests;

public class AdminServiceTests
{
    private readonly Mock<ISubscriptionRepository> _subRepo = new();
    private readonly Mock<IAnalyticsRepository> _analyticsRepo = new();
    private readonly Mock<IAuditService> _audit = new();
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<IAdminUserRepository> _adminUserRepo = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepo = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManager = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _sut = new AdminService(
            _subRepo.Object, _analyticsRepo.Object, _audit.Object,
            _userManager.Object, _adminUserRepo.Object, _auditLogRepo.Object,
            _cache.Object);
    }

    [Fact]
    public async Task GrantTrialAsync_UserNotFound_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var result = await _sut.GrantTrialAsync(Guid.NewGuid(), Guid.NewGuid(), 2, 7);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Usuário não encontrado.");
    }

    [Fact]
    public async Task GrantTrialAsync_AlreadyUsedTrial_ReturnsFailure()
    {
        var targetId = Guid.NewGuid();
        _userManager.Setup(m => m.FindByIdAsync(targetId.ToString()))
            .ReturnsAsync(new AppUser { Id = targetId, Name = "Test", IsActive = true });
        _subRepo.Setup(r => r.HasUsedTrialAsync(targetId, 2)).ReturnsAsync(true);

        var result = await _sut.GrantTrialAsync(Guid.NewGuid(), targetId, 2, 7);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Este usuário já utilizou o trial deste plano.");
    }

    [Fact]
    public async Task GrantTrialAsync_ValidRequest_SetsTrialStatusAndAudits()
    {
        var adminId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var plan = new Plan { Id = 2, Name = "Pro", IsActive = true };
        var sub = new Subscription { UserId = targetId, PlanId = 1 };

        _userManager.Setup(m => m.FindByIdAsync(targetId.ToString()))
            .ReturnsAsync(new AppUser { Id = targetId, Name = "Test", IsActive = true });
        _subRepo.Setup(r => r.HasUsedTrialAsync(targetId, 2)).ReturnsAsync(false);
        _subRepo.Setup(r => r.GetPlanByIdAsync(2)).ReturnsAsync(plan);
        _subRepo.Setup(r => r.GetByUserIdAsync(targetId)).ReturnsAsync(sub);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _audit.Setup(a => a.LogAsync(adminId, "admin.trial_granted", It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GrantTrialAsync(adminId, targetId, 2, 7);

        result.IsSuccess.Should().BeTrue();
        sub.Status.Should().Be(SubscriptionStatus.Trial);
        sub.PlanId.Should().Be(2);
        sub.TrialEndsAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        _subRepo.Verify(r => r.AddTrialUsageAsync(
            It.Is<TrialUsage>(t => t.UserId == targetId && t.PlanId == 2 && t.Status == TrialStatus.Active)),
            Times.Once);
        _subRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _audit.Verify(a => a.LogAsync(adminId, "admin.trial_granted", It.IsAny<object?>()), Times.Once);
    }
}
```

- [ ] **Step 2: Run tests — expect compile failure (AdminService missing GrantTrialAsync)**

```bash
dotnet test tests/UmbLink.UnitTests/UmbLink.UnitTests.csproj
```
Expected: Build error — `AdminService` does not contain `GrantTrialAsync` and constructor mismatch.

- [ ] **Step 3: Update `AdminUserDto` in `AdminDto.cs`**

Replace the entire `AdminUserDto` record:

```csharp
public record AdminUserDto(
    Guid Id,
    string Name,
    string Email,
    string PlanName,
    SubscriptionStatus Status,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? TrialEndsAt,
    bool IsOnTrial
);
```

- [ ] **Step 4: Add `GrantTrialAsync` to `IAdminService.cs`**

Replace the entire interface content:

```csharp
using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
namespace UmbLink.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null);
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<List<AdminAuditLogDto>> GetRecentAuditLogsAsync(int count = 20);
    Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId);
    Task<Result<bool>> GrantTrialAsync(Guid adminId, Guid targetUserId, int planId, int durationDays);
}
```

- [ ] **Step 5: Implement `GrantTrialAsync` in `AdminService.cs`**

Replace the entire file:

```csharp
using Microsoft.AspNetCore.Identity;
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
    UserManager<AppUser> userManager,
    IAdminUserRepository adminUserRepo,
    IAuditLogRepository auditLogRepo,
    ICacheService cache) : IAdminService
{
    public async Task<List<AdminUserDto>> GetUsersAsync(string? search = null)
    {
        var users = await adminUserRepo.GetUsersWithPlanAsync(search);
        return users.Select(u => new AdminUserDto(
            u.Id, u.Name, u.Email ?? "",
            u.Subscription?.Plan.Name ?? "Free",
            u.Subscription?.Status ?? SubscriptionStatus.Free,
            u.IsActive, u.CreatedAt,
            u.Subscription?.TrialEndsAt,
            u.Subscription?.Status == SubscriptionStatus.Trial
        )).ToList();
    }

    public async Task<AdminStatsDto> GetGlobalStatsAsync()
    {
        var users = await adminUserRepo.CountAsync();
        var pages = await analyticsRepo.GetTotalPagesAsync();
        var clicks = await analyticsRepo.GetTotalClicksAsync();
        var revenue = await subRepo.GetActiveSubscriptionRevenueAsync();

        var now = DateTime.UtcNow;
        var active7d  = await adminUserRepo.CountNewSinceAsync(now.AddDays(-7));
        var active30d = await adminUserRepo.CountNewSinceAsync(now.AddDays(-30));
        var published = await analyticsRepo.GetPublishedPagesCountAsync();
        var draft     = await analyticsRepo.GetDraftPagesCountAsync();
        var planDist  = await subRepo.GetPlanDistributionAsync();

        return new AdminStatsDto(users, pages, clicks, revenue, active7d, active30d, published, draft, planDist);
    }

    public async Task<List<AdminAuditLogDto>> GetRecentAuditLogsAsync(int count = 20)
    {
        var logs = await auditLogRepo.GetRecentAsync(count);
        return logs.Select(l => new AdminAuditLogDto(
            l.User?.Name ?? "Sistema",
            l.Action,
            l.CreatedAt
        )).ToList();
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

    public async Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId)
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
        await audit.LogAsync(adminId, "admin.plan_changed", new { targetUserId, planId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> GrantTrialAsync(Guid adminId, Guid targetUserId, int planId, int durationDays)
    {
        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null) return Result<bool>.Fail("Usuário não encontrado.");

        var alreadyUsed = await subRepo.HasUsedTrialAsync(targetUserId, planId);
        if (alreadyUsed) return Result<bool>.Fail("Este usuário já utilizou o trial deste plano.");

        var plan = await subRepo.GetPlanByIdAsync(planId);
        if (plan is null) return Result<bool>.Fail("Plano não encontrado.");

        var sub = await subRepo.GetByUserIdAsync(targetUserId);
        var now = DateTime.UtcNow;

        if (sub is null)
        {
            sub = new Subscription { UserId = targetUserId };
            await subRepo.CreateAsync(sub);
        }

        sub.PlanId = planId;
        sub.Status = SubscriptionStatus.Trial;
        sub.TrialStartedAt = now;
        sub.TrialEndsAt = now.AddDays(durationDays);
        sub.UpdatedAt = now;

        await subRepo.AddTrialUsageAsync(new TrialUsage
        {
            UserId = targetUserId,
            PlanId = planId,
            Status = TrialStatus.Active,
            StartedAt = now
        });
        await subRepo.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.UserSubscription(targetUserId));
        await audit.LogAsync(adminId, "admin.trial_granted", new { targetUserId, planId, durationDays });
        return Result<bool>.Ok(true);
    }
}
```

- [ ] **Step 6: Run tests — expect pass**

```bash
dotnet test tests/UmbLink.UnitTests/UmbLink.UnitTests.csproj
```
Expected: `Passed! - 3 tests`  (the 3 new `AdminServiceTests` + any existing)

- [ ] **Step 7: Build full solution**

```bash
dotnet build
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 8: Commit**

```bash
git add src/UmbLink.Application/DTOs/AdminDto.cs \
        src/UmbLink.Application/Interfaces/IAdminService.cs \
        src/UmbLink.Application/Services/AdminService.cs \
        tests/UmbLink.UnitTests/AdminServiceTests.cs
git commit -m "feat: add GrantTrialAsync to AdminService with trial status on AdminUserDto"
```

---

## Task 4: `Dashboard/Index.razor` — Skeleton Loading

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Index.razor`

The current file has:
- Lines 17–49: metric cards wrapped in `@if (_pages.Count > 0)`
- Lines 51–70: header + upgrade alert
- Lines 73–131: `<LoadingSpinner Loading="_loading">` wrapping page list

Replace lines 17–131 (everything inside `<div class="dashboard-content ...">`) with the block below. Lines 1–16 (up to and including the opening of `dashboard-content`) and lines 133 onward remain unchanged.

- [ ] **Step 1: Replace the inner content of `dashboard-content` div**

The new content of `<div class="dashboard-content container py-4" style="max-width:960px">`:

```razor
        @if (_loading)
        {
            <div class="row g-3 mb-4">
                @for (int i = 0; i < 4; i++)
                {
                    <div class="col-6 col-md-3">
                        <div class="dashboard-glass-card">
                            <Skeleton Width="60%" Height=".75rem" />
                            <Skeleton Width="50%" Height="1.75rem" />
                            <Skeleton Width="45%" Height=".75rem" />
                        </div>
                    </div>
                }
            </div>
            <div class="d-flex justify-content-between align-items-center mb-4">
                <Skeleton Width="180px" Height="1.5rem" />
                <Skeleton Width="130px" Height="2.25rem" BorderRadius="10px" />
            </div>
            <div class="row g-3">
                @for (int i = 0; i < 3; i++)
                {
                    <div class="col-md-6 col-lg-4">
                        <div class="page-card">
                            <div class="page-card-body">
                                <div class="d-flex justify-content-between mb-2">
                                    <Skeleton Width="55%" Height="1rem" />
                                    <Skeleton Width="20%" Height="1rem" BorderRadius="50px" />
                                </div>
                                <Skeleton Width="80%" Height=".75rem" />
                                <div style="margin-top:.35rem">
                                    <Skeleton Width="40%" Height=".75rem" />
                                </div>
                            </div>
                            <div class="page-card-footer">
                                <Skeleton Width="80px" Height="2rem" BorderRadius="10px" />
                                <Skeleton Width="44px" Height="2rem" BorderRadius="10px" />
                                <Skeleton Width="44px" Height="2rem" BorderRadius="10px" />
                                <Skeleton Width="44px" Height="2rem" BorderRadius="10px" />
                            </div>
                        </div>
                    </div>
                }
            </div>
        }
        else
        {
            <div class="fade-in-content">
                @* ─── Metrics cards ─── *@
                @if (_pages.Count > 0)
                {
                    <div class="row g-3 mb-4">
                        <div class="col-6 col-md-3">
                            <div class="dashboard-glass-card">
                                <div class="metric-label">Views (30d)</div>
                                <div class="metric-value">@_totalViews30d</div>
                                <div class="metric-sub"><i class="bi bi-eye me-1"></i>visualizações</div>
                            </div>
                        </div>
                        <div class="col-6 col-md-3">
                            <div class="dashboard-glass-card">
                                <div class="metric-label">Cliques (30d)</div>
                                <div class="metric-value">@_totalClicks30d</div>
                                <div class="metric-sub"><i class="bi bi-cursor me-1"></i>cliques</div>
                            </div>
                        </div>
                        <div class="col-6 col-md-3">
                            <div class="dashboard-glass-card">
                                <div class="metric-label">Páginas</div>
                                <div class="metric-value">@_pages.Count</div>
                                <div class="metric-sub"><i class="bi bi-file-earmark me-1"></i>criadas</div>
                            </div>
                        </div>
                        <div class="col-6 col-md-3">
                            <div class="dashboard-glass-card">
                                <div class="metric-label">Mais views</div>
                                <div class="metric-value" style="font-size:1rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">@(_topPageTitle ?? "—")</div>
                                <div class="metric-sub"><i class="bi bi-star me-1"></i>destaque</div>
                            </div>
                        </div>
                    </div>
                }

                <div class="d-flex justify-content-between align-items-center mb-4">
                    <div>
                        <h4 style="font-weight:700;color:var(--text-primary);margin:0">Minhas Páginas</h4>
                        @if (_sub is not null)
                        {
                            <span class="text-theme-secondary" style="font-size:.8rem">Plano <PlanBadge PlanName="@_sub.PlanName" /></span>
                        }
                    </div>
                    <button class="btn-accent" @onclick="ShowCreateModal">
                        <i class="bi bi-plus-lg"></i>Nova página
                    </button>
                </div>

                @if (_sub?.PlanName == "Free" && _pages.Count >= 1)
                {
                    <div class="alert-theme mb-3">
                        <i class="bi bi-stars text-accent flex-shrink-0"></i>
                        <span class="flex-grow-1">Você está no plano gratuito — <strong>@_pages.Count/1</strong> página usada.
                            <a href="/plans" style="color:var(--accent);font-weight:600">Fazer upgrade</a> para até 3 páginas e links ilimitados.</span>
                    </div>
                }

                @if (_pages.Count == 0)
                {
                    <div class="text-center py-5">
                        <div class="animate-fade-in-up">
                            <i class="bi bi-stars" style="font-size:3rem;color:var(--accent);display:block;margin-bottom:1rem"></i>
                            <h5 style="font-weight:700;color:var(--text-primary)">Crie sua primeira página!</h5>
                            <p class="text-theme-secondary" style="max-width:360px;margin:0 auto 1.5rem">
                                Junte todos os seus links em um só lugar e compartilhe com o mundo.
                            </p>
                            <button class="btn-accent btn-accent-lg" @onclick="ShowCreateModal">
                                <i class="bi bi-rocket-takeoff"></i>Criar minha página
                            </button>
                            <p class="text-theme-muted" style="font-size:.8rem;margin-top:0.75rem">Grátis, sem cartão. Pronto em minutos.</p>
                        </div>
                    </div>
                }
                else
                {
                    <div class="row g-3">
                        @foreach (var pg in _pages)
                        {
                            <div @key="pg.Id" class="col-md-6 col-lg-4">
                                <div class="page-card">
                                    <div class="page-card-body">
                                        <div class="d-flex justify-content-between align-items-start mb-2">
                                            <h6 style="font-weight:600;color:var(--text-primary);margin:0">@pg.Title</h6>
                                            <span class="badge-theme @(pg.Status == PageStatus.Published ? "badge-success" : "badge-muted") ms-2">
                                                @(pg.Status == PageStatus.Published ? "Publicada" : "Rascunho")
                                            </span>
                                        </div>
                                        <p class="text-theme-muted" style="font-size:.8rem;margin-bottom:.25rem">@GetPublicPageUrl(pg.Slug)</p>
                                        <p class="text-theme-secondary" style="font-size:.8rem;margin:0">@pg.LinkCount link(s)</p>
                                    </div>
                                    <div class="page-card-footer">
                                        <a href="/editor/@pg.Id" class="btn-accent" style="flex-grow:1;justify-content:center">
                                            <i class="bi bi-pencil"></i>Editar
                                        </a>
                                        <a href="/dashboard/metrics/@pg.Id" class="btn-ghost" title="Métricas">
                                            <i class="bi bi-bar-chart"></i>
                                        </a>
                                        <button class="btn-ghost" @onclick="() => CopyLink(pg.Slug)" title="Copiar link">
                                            <i class="bi @(_copiedSlug == pg.Slug ? "bi-check-lg" : "bi-link-45deg")" style="color:@(_copiedSlug == pg.Slug ? "var(--success)" : "")"></i>
                                        </button>
                                        <button class="btn-ghost" @onclick="() => TogglePublish(pg)"
                                                title="@(pg.Status == PageStatus.Published ? "Despublicar" : "Publicar")"
                                                style="color:@(pg.Status == PageStatus.Published ? "var(--warning)" : "var(--success)")">
                                            <i class="bi @(pg.Status == PageStatus.Published ? "bi-eye-slash" : "bi-eye")"></i>
                                        </button>
                                        <button class="btn-ghost" style="color:var(--danger)" @onclick="() => ConfirmDelete(pg)" title="Excluir">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                }
            </div>
        }
```

- [ ] **Step 2: Build**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Index.razor
git commit -m "feat: replace LoadingSpinner with skeleton in Dashboard/Index"
```

---

## Task 5: `Admin/Index.razor` — Skeleton + FormatAction

**Files:**
- Modify: `src/UmbLink.Web/Pages/Admin/Index.razor`

Two changes: (a) replace `<LoadingSpinner Loading="_loading">` with `@if/_loading_` skeleton, (b) add `"admin.trial_granted"` case to the three switch expressions.

- [ ] **Step 1: Replace `<LoadingSpinner Loading="_loading">` wrapper**

In `Admin/Index.razor`, the `<LoadingSpinner Loading="_loading">` block (lines 18–158) wraps all `_stats`-dependent content. Replace it with:

```razor
@if (_loading)
{
    <div class="row g-3 mb-3 align-items-stretch">
        @for (int i = 0; i < 4; i++)
        {
            <div class="col-sm-6 col-lg-3">
                <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <Skeleton Width="55%" Height="2rem" BorderRadius="8px" />
                    <div style="margin-top:.5rem"><Skeleton Width="75%" Height=".75rem" /></div>
                </div>
            </div>
        }
    </div>
    <div class="row g-3 mb-4 align-items-stretch">
        @for (int i = 0; i < 4; i++)
        {
            <div class="col-sm-6 col-lg-3">
                <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <div class="d-flex justify-content-between align-items-center">
                        <div style="flex:1">
                            <Skeleton Width="60%" Height="1.5rem" />
                            <div style="margin-top:.4rem"><Skeleton Width="80%" Height=".75rem" /></div>
                        </div>
                        <Skeleton Width="38px" Height="38px" BorderRadius="50%" />
                    </div>
                </div>
            </div>
        }
    </div>
    <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.25rem">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <Skeleton Width="160px" Height="1rem" />
            <Skeleton Width="100px" Height=".75rem" />
        </div>
        <div class="table-responsive">
            <table class="table table-sm align-middle mb-0">
                <tbody>
                    @for (int i = 0; i < 5; i++)
                    {
                        <tr>
                            <td><Skeleton Width="100px" Height=".875rem" /></td>
                            <td><Skeleton Width="120px" Height="1.25rem" BorderRadius="6px" /></td>
                            <td><Skeleton Width="70px" Height=".875rem" /></td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
}
else
{
    <div class="fade-in-content">
        @if (_stats is not null)
        {
            @* Row 1: main KPIs *@
            <div class="row g-3 mb-3 align-items-stretch">
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold" style="color:var(--accent)">@_stats.TotalUsers</div>
                        <div class="text-theme-muted small"><i class="bi bi-people me-1"></i>Usuários totais</div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-success">@_stats.TotalPages</div>
                        <div class="text-theme-muted small"><i class="bi bi-file-earmark me-1"></i>Páginas totais</div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-info">@_stats.TotalClicks</div>
                        <div class="text-theme-muted small"><i class="bi bi-cursor me-1"></i>Cliques totais</div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-warning">R$@_stats.SimulatedRevenue.ToString("F2")</div>
                        <div class="text-theme-muted small"><i class="bi bi-cash-coin me-1"></i>Receita simulada/mês</div>
                    </div>
                </div>
            </div>

            @* Row 2: engagement & pages breakdown *@
            <div class="row g-3 mb-4 align-items-stretch">
                <div class="col-sm-6 col-lg-3">
                    <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <div class="fs-4 fw-bold" style="color:var(--text-primary)">@_stats.ActiveLast7Days</div>
                                <div class="text-theme-muted small">Novos usuários (7d)</div>
                            </div>
                            <i class="bi bi-person-plus fs-3" style="color:var(--accent);opacity:.4"></i>
                        </div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <div class="fs-4 fw-bold" style="color:var(--text-primary)">@_stats.ActiveLast30Days</div>
                                <div class="text-theme-muted small">Novos usuários (30d)</div>
                            </div>
                            <i class="bi bi-calendar-range fs-3" style="color:var(--accent);opacity:.4"></i>
                        </div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <div class="fs-4 fw-bold text-success">@_stats.PublishedPages</div>
                                <div class="text-theme-muted small">Páginas publicadas</div>
                            </div>
                            <i class="bi bi-eye fs-3 text-success" style="opacity:.4"></i>
                        </div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <div class="fs-4 fw-bold text-warning">@_stats.DraftPages</div>
                                <div class="text-theme-muted small">Páginas em rascunho</div>
                            </div>
                            <i class="bi bi-pencil fs-3 text-warning" style="opacity:.4"></i>
                        </div>
                    </div>
                </div>
            </div>

            @* Plan distribution *@
            @if (_stats.PlanDistribution.Count > 0)
            {
                <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.25rem;margin-bottom:1.5rem">
                    <h6 class="fw-bold mb-3" style="color:var(--text-primary)"><i class="bi bi-pie-chart me-2" style="color:var(--accent)"></i>Distribuição de Planos</h6>
                    <div class="d-flex flex-wrap gap-3">
                        @foreach (var kv in _stats.PlanDistribution.OrderBy(k => k.Key))
                        {
                            <div style="display:flex;align-items:center;gap:.5rem">
                                <PlanBadge PlanName="@kv.Key" />
                                <span style="font-weight:700;color:var(--text-primary)">@kv.Value</span>
                                <span class="text-theme-muted" style="font-size:.8rem">(@((_stats.TotalUsers > 0 ? kv.Value * 100.0 / _stats.TotalUsers : 0).ToString("F0"))%)</span>
                            </div>
                        }
                    </div>
                </div>
            }

            @* Recent activity logs *@
            <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:12px;padding:1.25rem">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="fw-bold mb-0" style="color:var(--text-primary)">
                        <i class="bi bi-clock-history me-2" style="color:var(--accent)"></i>Atividade Recente
                    </h6>
                    <span class="text-theme-muted" style="font-size:.75rem">Últimas 20 ações</span>
                </div>
                @if (_auditLogs.Count == 0)
                {
                    <p class="text-theme-muted" style="font-size:.85rem">Nenhuma ação registrada ainda.</p>
                }
                else
                {
                    <div class="table-responsive">
                        <table class="table table-sm align-middle mb-0">
                            <thead>
                                <tr>
                                    <th style="font-size:.75rem;color:var(--text-muted);font-weight:600;text-transform:uppercase">Usuário</th>
                                    <th style="font-size:.75rem;color:var(--text-muted);font-weight:600;text-transform:uppercase">Ação</th>
                                    <th style="font-size:.75rem;color:var(--text-muted);font-weight:600;text-transform:uppercase">Quando</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var log in _auditLogs)
                                {
                                    <tr>
                                        <td style="font-size:.85rem;color:var(--text-primary)">@log.UserName</td>
                                        <td>
                                            <span style="font-size:.8rem;padding:.2rem .5rem;border-radius:6px;background:@ActionBadgeBg(log.Action);color:@ActionBadgeColor(log.Action);font-weight:600">
                                                @FormatAction(log.Action)
                                            </span>
                                        </td>
                                        <td style="font-size:.8rem;color:var(--text-muted)" title="@log.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss") UTC">
                                            @FormatRelativeTime(log.CreatedAt)
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                }
            </div>
        }
    </div>
}
```

- [ ] **Step 2: Update the three switch expressions in `@code` — add `"admin.trial_granted"` case**

In `@code`, find `FormatAction` and replace:

```csharp
    static string FormatAction(string action) => action switch
    {
        "page.create"              => "Página criada",
        "page.delete"              => "Página excluída",
        "page.publish"             => "Página publicada",
        "page.unpublish"           => "Página despublicada",
        "subscription.trial"       => "Trial iniciado",
        "subscription.trial_started" => "Trial iniciado",
        "subscription.activated"   => "Plano ativado",
        "subscription.cancelled"   => "Plano cancelado",
        "admin.user_suspended"     => "Usuário suspenso",
        "admin.user_reactivated"   => "Usuário reativado",
        "admin.plan_changed"       => "Plano alterado (admin)",
        "admin.trial_granted"      => "Trial concedido (admin)",
        _                          => action
    };
```

Find `ActionBadgeBg` and replace:

```csharp
    static string ActionBadgeBg(string action) => action switch
    {
        "page.create" or "subscription.trial" or "subscription.trial_started" or "subscription.activated" or "admin.trial_granted"
            => "rgba(16,185,129,0.1)",
        "page.delete" or "subscription.cancelled" or "admin.user_suspended"
            => "rgba(239,68,68,0.1)",
        "admin.user_reactivated" or "admin.plan_changed"
            => "rgba(245,158,11,0.1)",
        _ => "var(--bg-hover)"
    };
```

Find `ActionBadgeColor` and replace:

```csharp
    static string ActionBadgeColor(string action) => action switch
    {
        "page.create" or "subscription.trial" or "subscription.trial_started" or "subscription.activated" or "admin.trial_granted"
            => "#10b981",
        "page.delete" or "subscription.cancelled" or "admin.user_suspended"
            => "#ef4444",
        "admin.user_reactivated" or "admin.plan_changed"
            => "#f59e0b",
        _ => "var(--text-secondary)"
    };
```

- [ ] **Step 3: Build**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add src/UmbLink.Web/Pages/Admin/Index.razor
git commit -m "feat: skeleton loading and trial_granted audit label in Admin/Index"
```

---

## Task 6: `Admin/Users.razor` — Full Overhaul

**Files:**
- Modify: `src/UmbLink.Web/Pages/Admin/Users.razor`

This task replaces the entire file content with all changes consolidated: skeleton, trial column, trial modal, loading states, ConfirmModal for suspend, and the modal title fix.

- [ ] **Step 1: Replace the entire file**

```razor
@page "/admin/users"
@layout AdminLayout
@attribute [Authorize(Policy = "AdminOnly")]
@inject IAdminService AdminSvc

<PageTitle>Usuários — Admin</PageTitle>

<div class="d-flex justify-content-between align-items-center mb-4">
    <h4 class="fw-bold mb-0">Usuários</h4>
    <div class="input-group" style="max-width:280px">
        <input @bind="_search" @bind:event="oninput" @bind:after="Search" class="form-control-theme form-control-sm" placeholder="Buscar..." />
    </div>
</div>

@if (_loading)
{
    <div class="table-responsive">
        <table class="table table-sm align-middle">
            <thead>
                <tr>
                    <th>Nome</th><th>E-mail</th><th>Plano</th><th>Status</th><th>Trial</th><th>Cadastro</th><th></th>
                </tr>
            </thead>
            <tbody>
                @for (int i = 0; i < 6; i++)
                {
                    <tr>
                        <td><Skeleton Width="110px" Height=".875rem" /></td>
                        <td><Skeleton Width="160px" Height=".875rem" /></td>
                        <td><Skeleton Width="55px" Height="1.25rem" BorderRadius="50px" /></td>
                        <td><Skeleton Width="65px" Height="1.25rem" BorderRadius="50px" /></td>
                        <td><Skeleton Width="70px" Height=".875rem" /></td>
                        <td><Skeleton Width="55px" Height=".875rem" /></td>
                        <td><Skeleton Width="160px" Height="2rem" BorderRadius="8px" /></td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
else
{
    <div class="fade-in-content table-responsive">
        <table class="table table-sm align-middle">
            <thead>
                <tr>
                    <th>Nome</th>
                    <th>E-mail</th>
                    <th>Plano</th>
                    <th>Status</th>
                    <th>Trial</th>
                    <th>Cadastro</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var u in _users)
                {
                    <tr @key="u.Id" class="@(u.IsActive ? "" : "table-secondary text-muted")">
                        <td>@u.Name</td>
                        <td>@u.Email</td>
                        <td><PlanBadge PlanName="@u.PlanName" /></td>
                        <td>
                            <span class="badge @(u.IsActive ? "bg-success" : "bg-danger")">
                                @(u.IsActive ? "Ativo" : "Suspenso")
                            </span>
                        </td>
                        <td>
                            @if (u.IsOnTrial && u.TrialEndsAt.HasValue)
                            {
                                var daysLeft = (int)Math.Ceiling((u.TrialEndsAt.Value - DateTime.UtcNow).TotalDays);
                                <span style="font-size:.75rem;padding:.2rem .5rem;border-radius:50px;background:rgba(85,124,242,0.12);color:var(--accent);font-weight:600">
                                    @(daysLeft > 0 ? $"Ativo ({daysLeft}d)" : "Expirando")
                                </span>
                            }
                            else
                            {
                                <span style="color:var(--text-muted);font-size:.85rem">—</span>
                            }
                        </td>
                        <td class="small text-theme-muted">@u.CreatedAt.ToString("dd/MM/yy")</td>
                        <td>
                            <div class="d-flex gap-1 flex-wrap">
                                @if (u.IsActive)
                                {
                                    <button class="btn btn-sm btn-outline-danger"
                                            @onclick="() => AskSuspend(u)"
                                            disabled="@(_suspendingId == u.Id)">
                                        @if (_suspendingId == u.Id)
                                        {
                                            <span class="spinner-border spinner-border-sm me-1"></span>
                                        }
                                        Suspender
                                    </button>
                                }
                                else
                                {
                                    <button class="btn btn-sm btn-outline-success"
                                            @onclick="() => Reactivate(u.Id)"
                                            disabled="@(_reactivatingId == u.Id)">
                                        @if (_reactivatingId == u.Id)
                                        {
                                            <span class="spinner-border spinner-border-sm me-1"></span>
                                        }
                                        Reativar
                                    </button>
                                }
                                <button class="btn btn-sm btn-outline-secondary" @onclick="() => OpenChangePlan(u)">Plano</button>
                                <button class="btn btn-sm btn-outline-primary"
                                        @onclick="() => OpenGrantTrial(u)"
                                        disabled="@u.IsOnTrial"
                                        title="@(u.IsOnTrial ? "Usuário já está em trial ativo" : "Conceder trial")">
                                    Trial
                                </button>
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}

@* ── Change Plan Modal ── *@
@if (_showChangePlan && _changePlanUser is not null)
{
    <div class="modal-overlay" style="position:fixed;inset:0;background:var(--modal-bg);z-index:1050;display:flex;align-items:center;justify-content:center;padding:1rem">
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:16px;width:100%;max-width:400px;padding:1.5rem">
            <h6 class="fw-bold mb-0" style="color:var(--text-primary);font-size:1rem">Alterar Plano</h6>
            <p class="text-theme-muted mb-3" style="font-size:.85rem">Usuário: <strong>@_changePlanUser.Name</strong></p>
            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Novo plano</label>
                <select class="form-control-theme w-100" @bind="_newPlanId">
                    <option value="1">Free</option>
                    <option value="2">Pro</option>
                    <option value="3">Business</option>
                </select>
            </div>
            <div class="d-flex gap-2 justify-content-end">
                <button class="btn-ghost" @onclick="CloseChangePlan">Cancelar</button>
                <button class="btn-accent" @onclick="ConfirmChangePlan">Confirmar</button>
            </div>
        </div>
    </div>
}

@* ── Grant Trial Modal ── *@
@if (_showGrantTrial && _grantTrialUser is not null)
{
    <div class="modal-overlay" style="position:fixed;inset:0;background:var(--modal-bg);z-index:1050;display:flex;align-items:center;justify-content:center;padding:1rem">
        <div style="background:var(--bg-card);border:1px solid var(--border);border-radius:16px;width:100%;max-width:400px;padding:1.5rem">
            <h6 class="fw-bold mb-0" style="color:var(--text-primary);font-size:1rem">Conceder Trial</h6>
            <p class="text-theme-muted mb-3" style="font-size:.85rem">Usuário: <strong>@_grantTrialUser.Name</strong></p>
            @if (!string.IsNullOrEmpty(_grantTrialError))
            {
                <div class="alert alert-danger py-2 mb-3" style="font-size:.8rem">@_grantTrialError</div>
            }
            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Plano</label>
                <select class="form-control-theme w-100" @bind="_grantTrialPlanId">
                    <option value="2">Pro</option>
                    <option value="3">Business</option>
                </select>
            </div>
            <div style="margin-bottom:1rem">
                <label style="display:block;font-size:.8rem;font-weight:600;color:var(--text-primary);margin-bottom:4px">Duração (dias)</label>
                <input type="number" min="1" max="90" class="form-control-theme w-100" @bind="_grantTrialDays" />
            </div>
            <div class="d-flex gap-2 justify-content-end">
                <button class="btn-ghost" @onclick="CloseGrantTrial" disabled="@_grantingTrial">Cancelar</button>
                <button class="btn-accent" @onclick="ConfirmGrantTrial" disabled="@_grantingTrial">
                    @if (_grantingTrial) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    Confirmar
                </button>
            </div>
        </div>
    </div>
}

<ConfirmModal @bind-Visible="_showSuspendConfirm"
    Title="Suspender usuário"
    Message="@($"Tem certeza que deseja suspender '{_suspendTarget?.Name}'? O acesso será bloqueado.")"
    OnConfirm="SuspendConfirmed" />

<ToastContainer @ref="_toast" />

@code {
    List<AdminUserDto> _users = [];
    bool _loading = true;
    string _search = "";
    ToastContainer? _toast;

    [CascadingParameter] Task<AuthenticationState> AuthState { get; set; } = default!;
    Guid _adminId;

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState;
        _adminId = auth.User.GetUserId();
        await Load();
    }

    async Task Load()
    {
        _loading = true;
        try { _users = await AdminSvc.GetUsersAsync(_search); }
        catch { _users = []; }
        finally { _loading = false; }
    }

    async Task Search() => await Load();

    // ── Suspend ──────────────────────────────────────────────────────────────
    AdminUserDto? _suspendTarget;
    bool _showSuspendConfirm;
    Guid? _suspendingId;

    void AskSuspend(AdminUserDto u) { _suspendTarget = u; _showSuspendConfirm = true; }

    async Task SuspendConfirmed()
    {
        if (_suspendTarget is null) return;
        _suspendingId = _suspendTarget.Id;
        await AdminSvc.SuspendUserAsync(_adminId, _suspendTarget.Id);
        _suspendingId = null;
        _suspendTarget = null;
        await Load();
        _toast?.Show("Usuário suspenso.", "warning");
    }

    // ── Reactivate ───────────────────────────────────────────────────────────
    Guid? _reactivatingId;

    async Task Reactivate(Guid id)
    {
        _reactivatingId = id;
        await AdminSvc.ReactivateUserAsync(_adminId, id);
        _reactivatingId = null;
        await Load();
        _toast?.Show("Usuário reativado.", "success");
    }

    // ── Change Plan ──────────────────────────────────────────────────────────
    AdminUserDto? _changePlanUser;
    bool _showChangePlan;
    int _newPlanId = 1;

    void OpenChangePlan(AdminUserDto u)
    {
        _changePlanUser = u;
        _newPlanId = u.PlanName switch { "Pro" => 2, "Business" => 3, _ => 1 };
        _showChangePlan = true;
    }

    void CloseChangePlan() { _showChangePlan = false; _changePlanUser = null; }

    async Task ConfirmChangePlan()
    {
        if (_changePlanUser is null) return;
        await AdminSvc.ChangePlanAsync(_adminId, _changePlanUser.Id, _newPlanId);
        _showChangePlan = false;
        _changePlanUser = null;
        await Load();
        _toast?.Show("Plano alterado com sucesso.", "success");
    }

    // ── Grant Trial ──────────────────────────────────────────────────────────
    AdminUserDto? _grantTrialUser;
    bool _showGrantTrial;
    int _grantTrialPlanId = 2;
    int _grantTrialDays = 7;
    bool _grantingTrial;
    string _grantTrialError = "";

    void OpenGrantTrial(AdminUserDto u)
    {
        _grantTrialUser = u;
        _grantTrialPlanId = 2;
        _grantTrialDays = 7;
        _grantTrialError = "";
        _showGrantTrial = true;
    }

    void CloseGrantTrial() { _showGrantTrial = false; _grantTrialUser = null; _grantTrialError = ""; }

    async Task ConfirmGrantTrial()
    {
        if (_grantTrialUser is null) return;
        _grantingTrial = true;
        _grantTrialError = "";
        var result = await AdminSvc.GrantTrialAsync(_adminId, _grantTrialUser.Id, _grantTrialPlanId, _grantTrialDays);
        _grantingTrial = false;
        if (result.IsSuccess)
        {
            _showGrantTrial = false;
            _grantTrialUser = null;
            await Load();
            _toast?.Show("Trial concedido com sucesso.", "success");
        }
        else
        {
            _grantTrialError = result.Error ?? "Erro ao conceder trial.";
        }
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/Pages/Admin/Users.razor
git commit -m "feat: skeleton, trial grant modal, loading states and suspend confirm in Admin/Users"
```

---

## Task 7: `Dashboard/Metrics.razor` — Skeleton Loading

**Files:**
- Modify: `src/UmbLink.Web/Pages/Dashboard/Metrics.razor`

Replace lines 23–87 (`<LoadingSpinner Loading="_loading">` block) with:

- [ ] **Step 1: Replace the `LoadingSpinner` block**

```razor
    @if (_loading)
    {
        <div class="row g-3 mb-4">
            @for (int i = 0; i < 3; i++)
            {
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <Skeleton Width="50%" Height="2rem" BorderRadius="8px" />
                        <div style="margin-top:.5rem"><Skeleton Width="65%" Height=".75rem" /></div>
                    </div>
                </div>
            }
        </div>
        <div class="card mb-4 p-3" style="background:var(--bg-card);border-color:var(--border)">
            <Skeleton Width="200px" Height="1rem" />
            <div style="margin-top:1rem"><Skeleton Width="100%" Height="280px" BorderRadius="8px" /></div>
        </div>
        <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
            <Skeleton Width="180px" Height="1rem" />
            <div style="margin-top:1rem">
                @for (int i = 0; i < 4; i++)
                {
                    <div class="d-flex justify-content-between py-2" style="border-bottom:1px solid var(--border)">
                        <Skeleton Width="55%" Height=".875rem" />
                        <Skeleton Width="10%" Height=".875rem" />
                        <Skeleton Width="10%" Height=".875rem" />
                    </div>
                }
            </div>
        </div>
    }
    else if (_metrics is not null)
    {
        <div class="fade-in-content">
            <!-- Summary cards -->
            <div class="row g-3 mb-4">
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-primary">@_metrics.TotalViews</div>
                        <div class="text-theme-muted small">Visualizações</div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-success">@_metrics.TotalClicks</div>
                        <div class="text-theme-muted small">Cliques</div>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-3">
                    <div class="card text-center p-3" style="background:var(--bg-card);border-color:var(--border)">
                        <div class="fs-2 fw-bold text-info">
                            @(_metrics.TotalViews > 0
                                ? $"{Math.Round((double)_metrics.TotalClicks / _metrics.TotalViews * 100, 1)}%"
                                : "—")
                        </div>
                        <div class="text-theme-muted small">CTR Geral</div>
                    </div>
                </div>
            </div>

            <!-- Chart -->
            <div class="card mb-4 p-3" style="background:var(--bg-card);border-color:var(--border)">
                <h6 class="fw-semibold mb-3">Visitas e cliques diários</h6>
                <canvas id="views-chart" height="100"></canvas>
            </div>

            <!-- Link table -->
            @if (_metrics.LinkMetrics.Count > 0)
            {
                <div class="card p-3" style="background:var(--bg-card);border-color:var(--border)">
                    <h6 class="fw-semibold mb-3">Performance por link</h6>
                    <div class="table-responsive">
                        <table class="table table-sm" style="--bs-table-color:var(--text-primary);--bs-table-bg:transparent;--bs-table-border-color:var(--border)">
                            <thead>
                                <tr>
                                    <th>Link</th>
                                    <th class="text-end">Cliques</th>
                                    <th class="text-end">CTR</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var lm in _metrics.LinkMetrics)
                                {
                                    <tr @key="lm.LinkId">
                                        <td>@lm.Title</td>
                                        <td class="text-end">@lm.Clicks</td>
                                        <td class="text-end">@lm.ClickRate%</td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                </div>
            }
        </div>
    }
```

- [ ] **Step 2: Build**

```bash
dotnet build src/UmbLink.Web/UmbLink.Web.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add src/UmbLink.Web/Pages/Dashboard/Metrics.razor
git commit -m "feat: skeleton loading in Dashboard/Metrics"
```

---

## Task 8: Quick Fixes — Checkout Visual + Final Tests

**Files:**
- Modify: `src/UmbLink.Web/Pages/Plans/Checkout.razor` (title/card separation)

- [ ] **Step 1: Fix `Checkout.razor` title separation**

In `Checkout.razor`, find lines 8–10:

```razor
<div class="container py-5" style="max-width:480px;color:var(--text-primary)">
    <h4 class="fw-bold mb-1">Finalizar assinatura</h4>
    <p class="text-theme-muted small mb-4">Plano <strong>@_planName</strong> — @_priceLabel</p>
```

Replace with:

```razor
<div class="container py-5" style="max-width:480px;color:var(--text-primary)">
    <div style="margin-bottom:1.75rem">
        <h4 class="fw-bold mb-2">Finalizar assinatura</h4>
        <p class="text-theme-muted small mb-0">Plano <strong>@_planName</strong> — @_priceLabel</p>
    </div>
```

- [ ] **Step 2: Run all tests**

```bash
dotnet test
```
Expected: All tests pass.

- [ ] **Step 3: Build full solution**

```bash
dotnet build
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add src/UmbLink.Web/Pages/Plans/Checkout.razor
git commit -m "fix: checkout title spacing and visual hierarchy"
```

---

## Self-Review Checklist

| Spec requirement | Task that covers it |
|---|---|
| Dark mode page-card gradients + hover glow | Task 1 |
| Dark mode page-card-footer depth | Task 1 |
| Skeleton CSS variables + keyframe (2s) | Task 1 |
| `Skeleton.razor` component | Task 2 |
| `AdminUserDto` + `TrialEndsAt`/`IsOnTrial` | Task 3 |
| `IAdminService.GrantTrialAsync` | Task 3 |
| `AdminService.GrantTrialAsync` implementation | Task 3 |
| `ICacheService` injected into AdminService | Task 3 |
| Unit tests for `GrantTrialAsync` | Task 3 |
| Dashboard/Index skeleton | Task 4 |
| Admin/Index skeleton | Task 5 |
| `admin.trial_granted` audit label + badge | Task 5 |
| Admin/Users skeleton | Task 6 |
| Trial column in Users table | Task 6 |
| Grant trial modal | Task 6 |
| Loading states for Suspend/Reactivate buttons | Task 6 |
| ConfirmModal before suspending user | Task 6 |
| Admin change-plan modal title spacing fixed | Task 6 |
| Metrics skeleton | Task 7 |
| Checkout title/card visual separation | Task 8 |
| `fade-in-content` transition on real content | Tasks 4–7 |
