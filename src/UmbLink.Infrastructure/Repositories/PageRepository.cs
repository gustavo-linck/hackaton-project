using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class PageRepository(AppDbContext db) : IPageRepository
{
    public async Task<Page?> GetByIdAsync(Guid id) =>
        await db.Pages.FindAsync(id);

    public async Task<Page?> GetByIdAndUserAsync(Guid id, Guid userId) =>
        await db.Pages.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    public async Task<Page?> GetBySlugAsync(string slug) =>
        await db.Pages.FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<Page?> GetPublishedBySlugWithLinksAsync(string slug) =>
        await db.Pages
            .Include(p => p.Links)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PageStatus.Published);

    public async Task<List<Page>> GetByUserIdAsync(Guid userId) =>
        await db.Pages
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<(Page page, int linkCount)>> GetByUserIdWithCountsAsync(Guid userId)
    {
        var results = await db.Pages
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { Page = p, LinkCount = p.Links.Count() })
            .ToListAsync();
        return results.Select(r => (r.Page, r.LinkCount)).ToList();
    }

    public async Task<Page> CreateAsync(Page page)
    {
        db.Pages.Add(page);
        await db.SaveChangesAsync();
        return page;
    }

    public async Task UpdateAsync(Page page)
    {
        page.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Page page)
    {
        db.Pages.Remove(page);
        await db.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludePageId = null)
    {
        var query = db.Pages.Where(p => p.Slug == slug);
        if (excludePageId.HasValue)
            query = query.Where(p => p.Id != excludePageId.Value);
        return await query.AnyAsync();
    }

    public async Task<int> CountByUserAsync(Guid userId, PageStatus? excludeStatus = null)
    {
        var query = db.Pages.Where(p => p.UserId == userId);
        if (excludeStatus.HasValue)
            query = query.Where(p => p.Status != excludeStatus.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetLinkCountAsync(Guid pageId) =>
        await db.Links.CountAsync(l => l.PageId == pageId);

    public async Task<List<Page>> GetByUserIdWithStatusAsync(Guid userId, PageStatus? excludeStatus = null)
    {
        var query = db.Pages.Where(p => p.UserId == userId);
        if (excludeStatus.HasValue)
            query = query.Where(p => p.Status != excludeStatus.Value);
        return await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }
}
