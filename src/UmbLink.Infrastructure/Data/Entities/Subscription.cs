using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PlanId { get; set; }
    public int? PlanPriceId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Free;
    public BillingPeriod? BillingPeriod { get; set; }
    public DateTime? TrialStartedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public PlanPrice? PlanPrice { get; set; }
}

public enum SubscriptionStatus { Free, Trial, Active, Cancelled, Expired }
