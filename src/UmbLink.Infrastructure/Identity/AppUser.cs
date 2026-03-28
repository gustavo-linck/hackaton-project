using Microsoft.AspNetCore.Identity;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Subscription? Subscription { get; set; }
    public ICollection<Page> Pages { get; set; } = [];
    public ICollection<TrialUsage> TrialUsages { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
