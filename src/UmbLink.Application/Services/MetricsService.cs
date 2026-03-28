using Microsoft.Extensions.DependencyInjection;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;

namespace UmbLink.Application.Services;

// Implementação completa no Task 17/18
public class MetricsService(IBackgroundTaskQueue queue, IServiceProvider sp) : IMetricsService
{
    public Task TrackClickAsync(Guid linkId, string? userAgent, string? referrer)
    {
        queue.Enqueue(async (services, ct) =>
        {
            var db = services.GetRequiredService<UmbLink.Infrastructure.Data.AppDbContext>();
            db.ClickEvents.Add(new UmbLink.Infrastructure.Data.Entities.ClickEvent
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
            var db = services.GetRequiredService<UmbLink.Infrastructure.Data.AppDbContext>();
            db.PageViews.Add(new UmbLink.Infrastructure.Data.Entities.PageView
            {
                PageId = pageId,
                Referrer = referrer
            });
            await db.SaveChangesAsync(ct);
        });
        return Task.CompletedTask;
    }

    public Task<MetricsSummaryDto> GetPageMetricsAsync(Guid userId, Guid pageId, int days) =>
        throw new NotImplementedException(); // Task 18
}
