using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByUserIdAsync(Guid userId);
    Task<Subscription?> GetByUserIdWithPlanAsync(Guid userId);
    Task<Subscription?> GetByUserIdWithPlanAndLimitAsync(Guid userId);
    Task<Plan?> GetPlanByIdAsync(int planId);
    Task<Plan?> GetPlanByIdWithLimitAsync(int planId);
    Task<Plan> GetFreePlanAsync();
    Task<Plan> GetFreePlanWithLimitAsync();
    Task<PlanPrice?> GetPlanPriceAsync(int planId, BillingPeriod period);
    Task<PlanLimit?> GetPlanLimitAsync(int planId);
    Task<bool> HasUsedTrialAsync(Guid userId, int planId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task AddTrialUsageAsync(TrialUsage trialUsage);
    Task ExpireActiveTrialsAsync(Guid userId);
    Task<List<Subscription>> GetExpiredTrialsAsync();
    Task<decimal> GetActiveSubscriptionRevenueAsync();
    Task SaveChangesAsync();
}
