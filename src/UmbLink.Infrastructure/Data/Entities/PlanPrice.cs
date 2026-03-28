namespace UmbLink.Infrastructure.Data.Entities;

public class PlanPrice
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public BillingPeriod BillingPeriod { get; set; }
    public decimal PricePerMonth { get; set; }
    public decimal TotalCharged { get; set; }
    public int DiscountPercent { get; set; }
    public Plan Plan { get; set; } = null!;
}

public enum BillingPeriod { Monthly, Quarterly, Annual }
