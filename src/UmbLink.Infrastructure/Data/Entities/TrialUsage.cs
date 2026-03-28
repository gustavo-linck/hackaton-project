using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class TrialUsage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PlanId { get; set; }
    public TrialStatus Status { get; set; } = TrialStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}

public enum TrialStatus { Active, Expired, Converted }
