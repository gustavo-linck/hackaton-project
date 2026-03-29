using System.ComponentModel.DataAnnotations;
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class Page
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(30)]
    public string Slug { get; set; } = string.Empty;
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(200)]
    public string? Bio { get; set; }
    [MaxLength(2048)]
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
