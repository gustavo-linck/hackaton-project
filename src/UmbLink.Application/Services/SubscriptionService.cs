using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class SubscriptionService(ISubscriptionRepository subRepo, IPageRepository pageRepo, ILinkRepository linkRepo, IAuditService audit, ICacheService cache) : ISubscriptionService
{
    public async Task<SubscriptionDto> GetAsync(Guid userId)
    {
        var cached = await cache.GetAsync<SubscriptionDto>(CacheKeys.UserSubscription(userId));
        if (cached is not null) return cached;

        var sub = await subRepo.GetByUserIdWithPlanAndLimitAsync(userId);

        SubscriptionDto dto;
        if (sub is null)
        {
            var freePlan = await subRepo.GetFreePlanWithLimitAsync();
            dto = BuildDto(null, freePlan);
        }
        else
        {
            dto = BuildDto(sub, sub.Plan);
        }

        await cache.SetAsync(CacheKeys.UserSubscription(userId), dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<Result<SubscriptionDto>> StartTrialAsync(Guid userId, int planId, BillingPeriod period)
    {
        var alreadyUsed = await subRepo.HasUsedTrialAsync(userId, planId);
        if (alreadyUsed) return Result<SubscriptionDto>.Fail("Você já usou o trial deste plano.");

        var plan = await subRepo.GetPlanByIdWithLimitAsync(planId);
        if (plan is null) return Result<SubscriptionDto>.Fail("Plano não encontrado.");

        var sub = await subRepo.GetByUserIdAsync(userId);
        var now = DateTime.UtcNow;

        if (sub is null)
        {
            sub = new Subscription { UserId = userId };
            await subRepo.CreateAsync(sub);
        }

        sub.PlanId = planId;
        sub.Status = SubscriptionStatus.Trial;
        sub.BillingPeriod = period;
        sub.TrialStartedAt = now;
        sub.TrialEndsAt = now.AddDays(7);
        sub.UpdatedAt = now;

        await subRepo.AddTrialUsageAsync(new TrialUsage { UserId = userId, PlanId = planId, Status = TrialStatus.Active, StartedAt = now });
        await subRepo.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.UserSubscription(userId));
        await audit.LogAsync(userId, "subscription.trial_started", new { planId });
        return Result<SubscriptionDto>.Ok(BuildDto(sub, plan));
    }

    public async Task<Result<SubscriptionDto>> ActivatePlanAsync(Guid userId, int planId, BillingPeriod period)
    {
        var plan = await subRepo.GetPlanByIdWithLimitAsync(planId);
        if (plan is null) return Result<SubscriptionDto>.Fail("Plano não encontrado.");

        var price = await subRepo.GetPlanPriceAsync(planId, period);
        if (price is null) return Result<SubscriptionDto>.Fail("Preço não encontrado para esta periodicidade.");

        var sub = await subRepo.GetByUserIdAsync(userId);
        var now = DateTime.UtcNow;

        if (sub is null)
        {
            sub = new Subscription { UserId = userId };
            await subRepo.CreateAsync(sub);
        }

        sub.PlanId = planId;
        sub.PlanPriceId = price.Id;
        sub.Status = SubscriptionStatus.Active;
        sub.BillingPeriod = period;
        sub.CurrentPeriodStart = now;
        sub.CurrentPeriodEnd = period switch {
            BillingPeriod.Monthly   => now.AddMonths(1),
            BillingPeriod.Quarterly => now.AddMonths(3),
            BillingPeriod.Annual    => now.AddYears(1),
            _ => now.AddMonths(1)
        };
        sub.TrialStartedAt = null;
        sub.TrialEndsAt = null;
        sub.UpdatedAt = now;

        await subRepo.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.UserSubscription(userId));
        await audit.LogAsync(userId, "subscription.activated", new { planId, period = period.ToString() });
        return Result<SubscriptionDto>.Ok(BuildDto(sub, plan));
    }

    public async Task<Result<bool>> CancelAsync(Guid userId)
    {
        var sub = await subRepo.GetByUserIdWithPlanAsync(userId);
        if (sub is null) return Result<bool>.Fail("Nenhuma assinatura ativa.");

        var freePlan = await subRepo.GetFreePlanAsync();
        sub.PlanId = freePlan.Id;
        sub.Status = SubscriptionStatus.Free;
        sub.BillingPeriod = null;
        sub.PlanPriceId = null;
        sub.CurrentPeriodStart = null;
        sub.CurrentPeriodEnd = null;
        sub.UpdatedAt = DateTime.UtcNow;

        // Downgrade: suspend excess pages and deactivate excess links
        var freeLimit = await subRepo.GetPlanLimitAsync(freePlan.Id);
        if (freeLimit is not null)
        {
            var pages = await pageRepo.GetByUserIdWithStatusAsync(userId, PageStatus.Suspended);

            for (int i = freeLimit.MaxPages; i < pages.Count; i++)
                pages[i].Status = PageStatus.Suspended;

            foreach (var page in pages.Take(freeLimit.MaxPages))
            {
                var links = await linkRepo.GetActiveByPageIdAsync(page.Id);

                for (int i = freeLimit.MaxLinksPerPage; i < links.Count; i++)
                    links[i].IsActive = false;
            }
        }

        await subRepo.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.UserSubscription(userId));
        await audit.LogAsync(userId, "subscription.cancelled", null);
        return Result<bool>.Ok(true);
    }

    public async Task ProcessExpiredTrialsAsync()
    {
        var expired = await subRepo.GetExpiredTrialsAsync();

        if (expired.Count == 0) return;

        var freePlan = await subRepo.GetFreePlanAsync();
        foreach (var sub in expired)
        {
            sub.PlanId = freePlan.Id;
            sub.Status = SubscriptionStatus.Free;
            sub.BillingPeriod = null;
            sub.TrialStartedAt = null;
            sub.TrialEndsAt = null;
            sub.UpdatedAt = DateTime.UtcNow;

            await subRepo.ExpireActiveTrialsAsync(sub.UserId);
        }
        await subRepo.SaveChangesAsync();
    }

    static SubscriptionDto BuildDto(Subscription? sub, Plan plan)
    {
        var l = plan.Limit;
        var limits = l is null
            ? new PlanLimitDto(1, 3, 7, false, false, false, 2, 2)
            : new PlanLimitDto(l.MaxPages, l.MaxLinksPerPage, l.AnalyticsDays,
                l.AllowReferrer, l.AllowCustomDomain, l.AllowRemoveBranding,
                l.ThemeCount, l.FontCount);

        return new SubscriptionDto(
            sub?.Id ?? Guid.Empty,
            plan.Name,
            sub?.Status ?? SubscriptionStatus.Free,
            sub?.BillingPeriod,
            sub?.TrialEndsAt,
            sub?.CurrentPeriodEnd,
            limits);
    }
}
