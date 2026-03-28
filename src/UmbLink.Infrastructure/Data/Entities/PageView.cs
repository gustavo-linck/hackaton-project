namespace UmbLink.Infrastructure.Data.Entities;

public class PageView
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Referrer { get; set; }
    public Page Page { get; set; } = null!;
}
