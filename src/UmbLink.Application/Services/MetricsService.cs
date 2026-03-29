using Microsoft.Extensions.DependencyInjection;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class MetricsService(IBackgroundTaskQueue queue, IPageRepository pageRepo, IAnalyticsRepository analyticsRepo) : IMetricsService
{
    public Task TrackClickAsync(Guid linkId, string? userAgent, string? referrer)
    {
        queue.Enqueue(async (services, ct) =>
        {
            var repo = services.GetRequiredService<IAnalyticsRepository>();
            await repo.AddClickAsync(new ClickEvent
            {
                LinkId = linkId,
                UserAgentSummary = userAgent,
                Referrer = referrer
            }, ct);
        });
        return Task.CompletedTask;
    }

    public Task TrackViewAsync(Guid pageId, string? referrer)
    {
        queue.Enqueue(async (services, ct) =>
        {
            var repo = services.GetRequiredService<IAnalyticsRepository>();
            await repo.AddViewAsync(new PageView
            {
                PageId = pageId,
                Referrer = referrer
            }, ct);
        });
        return Task.CompletedTask;
    }

    public async Task<MetricsSummaryDto> GetPageMetricsAsync(Guid userId, Guid pageId, int days)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return new MetricsSummaryDto(0, 0, [], []);

        var from = DateTime.UtcNow.Date.AddDays(-days + 1);

        var views = await analyticsRepo.GetViewsByPageAsync(pageId, from);
        var links = await analyticsRepo.GetClicksByPageLinksAsync(pageId, from);

        var totalViews = views.Sum(v => v.Count);
        var totalClicks = links.Sum(l => l.Clicks);

        var daily = Enumerable.Range(0, days)
            .Select(i => from.AddDays(i))
            .Select(d => new DailyMetricDto(
                d,
                views.FirstOrDefault(v => v.Date == d).Count,
                0))
            .ToList();

        var linkMetrics = links
            .OrderByDescending(l => l.Clicks)
            .Select(l => new LinkMetricDto(
                l.LinkId, l.Title, l.Clicks,
                totalViews > 0 ? Math.Round((double)l.Clicks / totalViews * 100, 1) : 0))
            .ToList();

        return new MetricsSummaryDto(totalViews, totalClicks, daily, linkMetrics);
    }
}
