namespace UmbLink.Infrastructure.Data.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public PlanLimit Limit { get; set; } = null!;
    public ICollection<PlanPrice> Prices { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
