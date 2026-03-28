using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.DTOs;

public record SubscriptionDto(
    Guid Id,
    string PlanName,
    SubscriptionStatus Status,
    BillingPeriod? BillingPeriod,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    PlanLimitDto Limits
);
