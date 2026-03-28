namespace UmbLink.Application.DTOs;

public record PlanLimitDto(
    int MaxPages,
    int MaxLinksPerPage,
    int AnalyticsDays,
    bool AllowReferrer,
    bool AllowCustomDomain,
    bool AllowRemoveBranding,
    int ThemeCount,
    int FontCount
);
