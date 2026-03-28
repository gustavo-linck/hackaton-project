namespace UmbLink.Infrastructure.Data.Entities;

public class PlanLimit
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public int MaxPages { get; set; }           // -1 = unlimited
    public int MaxLinksPerPage { get; set; }    // -1 = unlimited
    public int AnalyticsDays { get; set; }      // -1 = unlimited
    public bool AllowReferrer { get; set; }
    public bool AllowCustomDomain { get; set; }
    public bool AllowRemoveBranding { get; set; }
    public int ThemeCount { get; set; }         // -1 = all
    public int FontCount { get; set; }          // -1 = all
    public Plan Plan { get; set; } = null!;
}
