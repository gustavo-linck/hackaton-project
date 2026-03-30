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
