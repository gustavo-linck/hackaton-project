using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(Guid id);
    Task<Page?> GetByIdAndUserAsync(Guid id, Guid userId);
    Task<Page?> GetBySlugAsync(string slug);
    Task<Page?> GetPublishedBySlugWithLinksAsync(string slug);
    Task<List<Page>> GetByUserIdAsync(Guid userId);
    Task<List<(Page page, int linkCount)>> GetByUserIdWithCountsAsync(Guid userId);
    Task<Page> CreateAsync(Page page);
    Task UpdateAsync(Page page);
    Task DeleteAsync(Page page);
    Task<bool> SlugExistsAsync(string slug, Guid? excludePageId = null);
    Task<int> CountByUserAsync(Guid userId, PageStatus? excludeStatus = null);
    Task<int> GetLinkCountAsync(Guid pageId);
    Task<List<Page>> GetByUserIdWithStatusAsync(Guid userId, PageStatus? excludeStatus = null);
}
