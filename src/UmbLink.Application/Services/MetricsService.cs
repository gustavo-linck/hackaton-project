using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class MetricsService(IBackgroundTaskQueue queue, IServiceProvider sp) : IMetricsService
{
    public Task TrackClickAsync(Guid linkId, string? userAgent, string? referrer)
    {
        queue.Enqueue(async (services, ct) =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            db.ClickEvents.Add(new ClickEvent
            {
                LinkId = linkId,
                UserAgentSummary = userAgent,
                Referrer = referrer
            });
            await db.SaveChangesAsync(ct);
        });
        return Task.CompletedTask;
    }

    public Task TrackViewAsync(Guid pageId, string? referrer)
    {
        queue.Enqueue(async (services, ct) =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            db.PageViews.Add(new PageView
            {
                PageId = pageId,
                Referrer = referrer
            });
            await db.SaveChangesAsync(ct);
        });
        return Task.CompletedTask;
    }

    public async Task<MetricsSummaryDto> GetPageMetricsAsync(Guid userId, Guid pageId, int days)
    {
        await using var scope = sp.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return new MetricsSummaryDto(0, 0, [], []);

        var from = DateTime.UtcNow.Date.AddDays(-days + 1);

        var views = await db.PageViews
            .Where(v => v.PageId == pageId && v.Timestamp >= from)
            .GroupBy(v => v.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var links = await db.Links
            .Where(l => l.PageId == pageId)
            .Select(l => new
            {
                l.Id, l.Title,
                Clicks = l.Clicks.Count(c => c.Timestamp >= from)
            })
            .ToListAsync();

        var totalViews = views.Sum(v => v.Count);
        var totalClicks = links.Sum(l => l.Clicks);

        var daily = Enumerable.Range(0, days)
            .Select(i => from.AddDays(i))
            .Select(d => new DailyMetricDto(
                d,
                views.FirstOrDefault(v => v.Date == d)?.Count ?? 0,
                0))
            .ToList();

        var linkMetrics = links
            .OrderByDescending(l => l.Clicks)
            .Select(l => new LinkMetricDto(
                l.Id, l.Title, l.Clicks,
                totalViews > 0 ? Math.Round((double)l.Clicks / totalViews * 100, 1) : 0))
            .ToList();

        return new MetricsSummaryDto(totalViews, totalClicks, daily, linkMetrics);
    }
}
