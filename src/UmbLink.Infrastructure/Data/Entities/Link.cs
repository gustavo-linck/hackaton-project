using System.ComponentModel.DataAnnotations;

namespace UmbLink.Infrastructure.Data.Entities;

public class Link
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    [MaxLength(80)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Page Page { get; set; } = null!;
    public ICollection<ClickEvent> Clicks { get; set; } = [];
}
