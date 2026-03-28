using Microsoft.EntityFrameworkCore;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class SubscriptionService(AppDbContext db, IAuditService audit) : ISubscriptionService
{
    public async Task<SubscriptionDto> GetAsync(Guid userId)
    {
        var sub = await db.Subscriptions
            .Include(s => s.Plan).ThenInclude(p => p.Limit!)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (sub is null)
        {
            var freePlan = await db.Plans.Include(p => p.Limit).FirstAsync(p => p.Name == "Free");
            return BuildDto(null, freePlan);
        }
        return BuildDto(sub, sub.Plan);
    }

    public async Task<Result<SubscriptionDto>> StartTrialAsync(Guid userId, int planId, BillingPeriod period)
    {
        var alreadyUsed = await db.TrialUsages.AnyAsync(t => t.UserId == userId && t.PlanId == planId);
        if (alreadyUsed) return Result<SubscriptionDto>.Fail("Você já usou o trial deste plano.");

        var plan = await db.Plans.Include(p => p.Limit).FirstOrDefaultAsync(p => p.Id == planId);
        if (plan is null) return Result<SubscriptionDto>.Fail("Plano não encontrado.");

        var sub = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        var now = DateTime.UtcNow;

        if (sub is null)
        {
            sub = new Subscription { UserId = userId };
            db.Subscriptions.Add(sub);
        }

        sub.PlanId = planId;
        sub.Status = SubscriptionStatus.Trial;
        sub.BillingPeriod = period;
        sub.TrialStartedAt = now;
        sub.TrialEndsAt = now.AddDays(7);
        sub.UpdatedAt = now;

        db.TrialUsages.Add(new TrialUsage { UserId = userId, PlanId = planId, Status = TrialStatus.Active, StartedAt = now });
        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "subscription.trial_started", new { planId });
        return Result<SubscriptionDto>.Ok(BuildDto(sub, plan));
    }

    public async Task<Result<SubscriptionDto>> ActivatePlanAsync(Guid userId, int planId, BillingPeriod period)
    {
        var plan = await db.Plans.Include(p => p.Limit).FirstOrDefaultAsync(p => p.Id == planId);
        if (plan is null) return Result<SubscriptionDto>.Fail("Plano não encontrado.");

        var price = await db.PlanPrices.FirstOrDefaultAsync(p => p.PlanId == planId && p.BillingPeriod == period);
        if (price is null) return Result<SubscriptionDto>.Fail("Preço não encontrado para esta periodicidade.");

        var sub = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        var now = DateTime.UtcNow;

        if (sub is null) { sub = new Subscription { UserId = userId }; db.Subscriptions.Add(sub); }

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

        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "subscription.activated", new { planId, period = period.ToString() });
        return Result<SubscriptionDto>.Ok(BuildDto(sub, plan));
    }

    public async Task<Result<bool>> CancelAsync(Guid userId)
    {
        var sub = await db.Subscriptions.Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub is null) return Result<bool>.Fail("Nenhuma assinatura ativa.");

        var freePlan = await db.Plans.FirstAsync(p => p.Name == "Free");
        sub.PlanId = freePlan.Id;
        sub.Status = SubscriptionStatus.Free;
        sub.BillingPeriod = null;
        sub.PlanPriceId = null;
        sub.CurrentPeriodStart = null;
        sub.CurrentPeriodEnd = null;
        sub.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "subscription.cancelled", null);
        return Result<bool>.Ok(true);
    }

    public async Task ProcessExpiredTrialsAsync()
    {
        var expired = await db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial && s.TrialEndsAt < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Count == 0) return;

        var freePlan = await db.Plans.FirstAsync(p => p.Name == "Free");
        foreach (var sub in expired)
        {
            sub.PlanId = freePlan.Id;
            sub.Status = SubscriptionStatus.Free;
            sub.BillingPeriod = null;
            sub.TrialStartedAt = null;
            sub.TrialEndsAt = null;
            sub.UpdatedAt = DateTime.UtcNow;

            await db.TrialUsages
                .Where(t => t.UserId == sub.UserId && t.Status == TrialStatus.Active)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TrialStatus.Expired));
        }
        await db.SaveChangesAsync();
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
