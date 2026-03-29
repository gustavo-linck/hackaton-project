using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface ILinkRepository
{
    Task<Link?> GetByIdAsync(Guid id);
    Task<Link?> GetByIdWithPageAsync(Guid id);
    Task<Link?> GetByIdAndUserAsync(Guid linkId, Guid userId);
    Task<List<Link>> GetByPageIdAsync(Guid pageId);
    Task<List<Link>> GetActiveByPageIdAsync(Guid pageId);
    Task<int> GetMaxOrderAsync(Guid pageId);
    Task<int> CountByPageAsync(Guid pageId);
    Task<Link> CreateAsync(Link link);
    Task UpdateAsync(Link link);
    Task DeleteAsync(Link link);
    Task SaveChangesAsync();
}
