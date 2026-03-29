using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IAnalyticsRepository
{
    Task AddClickAsync(ClickEvent clickEvent, CancellationToken ct = default);
    Task AddViewAsync(PageView pageView, CancellationToken ct = default);
    Task<List<(DateTime Date, int Count)>> GetViewsByPageAsync(Guid pageId, DateTime from);
    Task<List<(Guid LinkId, string Title, int Clicks)>> GetClicksByPageLinksAsync(Guid pageId, DateTime from);
    Task<int> GetTotalClicksAsync();
    Task<int> GetTotalPagesAsync();
    Task<int> GetPublishedPagesCountAsync();
    Task<int> GetDraftPagesCountAsync();
}
