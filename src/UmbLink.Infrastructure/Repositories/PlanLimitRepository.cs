using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class PlanLimitRepository(AppDbContext db) : IPlanLimitRepository
{
    public async Task<int?> GetPlanIdByUserAsync(Guid userId) =>
        await db.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => (int?)s.PlanId)
            .FirstOrDefaultAsync();

    public async Task<int?> GetFreePlanIdAsync() =>
        await db.Plans
            .Where(p => p.Name == "Free")
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    public async Task<PlanLimit?> GetByPlanIdAsync(int planId) =>
        await db.PlanLimits.FirstOrDefaultAsync(pl => pl.PlanId == planId);

    public async Task<string> GetPlanNameByUserAsync(Guid userId) =>
        await db.Subscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.Plan.Name)
            .FirstOrDefaultAsync() ?? "Free";

    public async Task<int> CountPagesByUserAsync(Guid userId, PageStatus? excludeStatus = null)
    {
        var query = db.Pages.Where(p => p.UserId == userId);
        if (excludeStatus.HasValue)
            query = query.Where(p => p.Status != excludeStatus.Value);
        return await query.CountAsync();
    }

    public async Task<int> CountLinksByPageAsync(Guid pageId) =>
        await db.Links.CountAsync(l => l.PageId == pageId);
}
