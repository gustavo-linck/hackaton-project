using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Data.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CardLastFour { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Brand { get; set; } = "Visa";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AppUser User { get; set; } = null!;
}
