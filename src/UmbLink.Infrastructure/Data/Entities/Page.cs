using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class Page
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Draft;
    public string ThemeConfig { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
    public ICollection<Link> Links { get; set; } = [];
    public ICollection<PageView> Views { get; set; } = [];
    public CustomDomain? CustomDomain { get; set; }
}

public enum PageStatus { Draft, Published, Suspended }
