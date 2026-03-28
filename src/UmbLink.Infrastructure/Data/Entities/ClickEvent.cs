namespace UmbLink.Infrastructure.Data.Entities;

public class ClickEvent
{
    public Guid Id { get; set; }
    public Guid LinkId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserAgentSummary { get; set; }
    public string? Referrer { get; set; }
    public Link Link { get; set; } = null!;
}
