namespace UmbLink.Application.Models;

public class LimitExceededError
{
    public string FeatureName { get; set; } = string.Empty;
    public string CurrentPlan { get; set; } = string.Empty;
    public string RequiredPlan { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
