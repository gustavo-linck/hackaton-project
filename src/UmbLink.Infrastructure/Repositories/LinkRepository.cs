using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class LinkRepository(AppDbContext db) : ILinkRepository
{
    public async Task<Link?> GetByIdAsync(Guid id) =>
        await db.Links.FindAsync(id);

    public async Task<Link?> GetByIdWithPageAsync(Guid id) =>
        await db.Links.Include(l => l.Page).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Link?> GetByIdAndUserAsync(Guid linkId, Guid userId) =>
        await db.Links.Include(l => l.Page)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Page.UserId == userId);

    public async Task<List<Link>> GetByPageIdAsync(Guid pageId) =>
        await db.Links.Where(l => l.PageId == pageId).OrderBy(l => l.Order).ToListAsync();

    public async Task<List<Link>> GetActiveByPageIdAsync(Guid pageId) =>
        await db.Links.Where(l => l.PageId == pageId && l.IsActive).OrderBy(l => l.Order).ToListAsync();

    public async Task<int> GetMaxOrderAsync(Guid pageId) =>
        await db.Links.Where(l => l.PageId == pageId).MaxAsync(l => (int?)l.Order) ?? 0;

    public async Task<int> CountByPageAsync(Guid pageId) =>
        await db.Links.CountAsync(l => l.PageId == pageId);

    public async Task<Link> CreateAsync(Link link)
    {
        db.Links.Add(link);
        await db.SaveChangesAsync();
        return link;
    }

    public async Task UpdateAsync(Link link) =>
        await db.SaveChangesAsync();

    public async Task DeleteAsync(Link link)
    {
        db.Links.Remove(link);
        await db.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() =>
        await db.SaveChangesAsync();
}
