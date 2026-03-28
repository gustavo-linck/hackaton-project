namespace UmbLink.Application.DTOs;

public record MetricsSummaryDto(
    int TotalViews,
    int TotalClicks,
    List<DailyMetricDto> DailyViews,
    List<LinkMetricDto> LinkMetrics
);

public record DailyMetricDto(DateTime Date, int Views, int Clicks);

public record LinkMetricDto(Guid LinkId, string Title, int Clicks, double ClickRate);
