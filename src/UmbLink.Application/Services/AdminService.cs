using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Application.Services;

public class AdminService(AppDbContext db, IAuditService audit, UserManager<AppUser> userManager) : IAdminService
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
        var pages = await db.Pages.CountAsync();
        var clicks = await db.ClickEvents.CountAsync();
        var revenue = await db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && s.PlanPrice != null)
            .SumAsync(s => (decimal?)s.PlanPrice!.PricePerMonth) ?? 0m;
        return new AdminStatsDto(users, pages, clicks, revenue);
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
        var plan = await db.Plans.FindAsync(planId);
        if (plan is null) return Result<bool>.Fail("Plano não encontrado.");

        var sub = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == targetUserId);
        if (sub is null) { sub = new Subscription { UserId = targetUserId }; db.Subscriptions.Add(sub); }

        sub.PlanId = planId;
        sub.Status = plan.Name == "Free" ? SubscriptionStatus.Free : SubscriptionStatus.Active;
        sub.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await audit.LogAsync(adminId, "admin.plan_changed", new { targetUserId, planId });
        return Result<bool>.Ok(true);
    }
}
