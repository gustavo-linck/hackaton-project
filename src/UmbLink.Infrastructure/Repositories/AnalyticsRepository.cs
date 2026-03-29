using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class AnalyticsRepository(AppDbContext db) : IAnalyticsRepository
{
    public async Task AddClickAsync(ClickEvent clickEvent, CancellationToken ct = default)
    {
        db.ClickEvents.Add(clickEvent);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddViewAsync(PageView pageView, CancellationToken ct = default)
    {
        db.PageViews.Add(pageView);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<(DateTime Date, int Count)>> GetViewsByPageAsync(Guid pageId, DateTime from)
    {
        var results = await db.PageViews
            .Where(v => v.PageId == pageId && v.Timestamp >= from)
            .GroupBy(v => v.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        return results.Select(r => (r.Date, r.Count)).ToList();
    }

    public async Task<List<(Guid LinkId, string Title, int Clicks)>> GetClicksByPageLinksAsync(Guid pageId, DateTime from)
    {
        var results = await db.Links
            .Where(l => l.PageId == pageId)
            .Select(l => new
            {
                l.Id,
                l.Title,
                Clicks = l.Clicks.Count(c => c.Timestamp >= from)
            })
            .ToListAsync();

        return results.Select(r => (r.Id, r.Title, r.Clicks)).ToList();
    }

    public async Task<int> GetTotalClicksAsync() =>
        await db.ClickEvents.CountAsync();

    public async Task<int> GetTotalPagesAsync() =>
        await db.Pages.CountAsync();
}
