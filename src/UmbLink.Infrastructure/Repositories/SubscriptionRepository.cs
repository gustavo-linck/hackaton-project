using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByUserIdAsync(Guid userId) =>
        await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<Subscription?> GetByUserIdWithPlanAsync(Guid userId) =>
        await db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<Subscription?> GetByUserIdWithPlanAndLimitAsync(Guid userId) =>
        await db.Subscriptions
            .Include(s => s.Plan).ThenInclude(p => p.Limit!)
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<Plan?> GetPlanByIdAsync(int planId) =>
        await db.Plans.FindAsync(planId);

    public async Task<Plan?> GetPlanByIdWithLimitAsync(int planId) =>
        await db.Plans.Include(p => p.Limit).FirstOrDefaultAsync(p => p.Id == planId);

    public async Task<Plan> GetFreePlanAsync() =>
        await db.Plans.FirstAsync(p => p.Name == "Free");

    public async Task<Plan> GetFreePlanWithLimitAsync() =>
        await db.Plans.Include(p => p.Limit).FirstAsync(p => p.Name == "Free");

    public async Task<PlanPrice?> GetPlanPriceAsync(int planId, BillingPeriod period) =>
        await db.PlanPrices.FirstOrDefaultAsync(p => p.PlanId == planId && p.BillingPeriod == period);

    public async Task<PlanLimit?> GetPlanLimitAsync(int planId) =>
        await db.PlanLimits.FirstOrDefaultAsync(pl => pl.PlanId == planId);

    public async Task<bool> HasUsedTrialAsync(Guid userId, int planId) =>
        await db.TrialUsages.AnyAsync(t => t.UserId == userId && t.PlanId == planId);

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync();
        return subscription;
    }

    public async Task AddTrialUsageAsync(TrialUsage trialUsage)
    {
        db.TrialUsages.Add(trialUsage);
        await db.SaveChangesAsync();
    }

    public async Task ExpireActiveTrialsAsync(Guid userId) =>
        await db.TrialUsages
            .Where(t => t.UserId == userId && t.Status == TrialStatus.Active)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TrialStatus.Expired));

    public async Task<List<Subscription>> GetExpiredTrialsAsync() =>
        await db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAt < DateTime.UtcNow)
            .ToListAsync();

    public async Task<decimal> GetActiveSubscriptionRevenueAsync() =>
        await db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && s.PlanPrice != null)
            .SumAsync(s => (decimal?)s.PlanPrice!.PricePerMonth) ?? 0m;

    public async Task SaveChangesAsync() =>
        await db.SaveChangesAsync();
}
