using Microsoft.EntityFrameworkCore;
using UmbLink.Application;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class PageService(AppDbContext db, IPlanLimitService limits, IAuditService audit, ICacheService cache) : IPageService
{
    public async Task<Result<PageDto>> CreateAsync(Guid userId, CreatePageRequest req)
    {
        var canAdd = await limits.CanAddPageAsync(userId);
        if (canAdd.IsFailure) return Result<PageDto>.LimitExceeded(canAdd.LimitError!);

        if (await db.Pages.AnyAsync(p => p.Slug == req.Slug))
            return Result<PageDto>.Fail("Este endereço já está em uso.");

        var page = new Page
        {
            UserId = userId,
            Slug = req.Slug,
            Title = req.Title,
            Bio = req.Bio,
            Status = PageStatus.Draft,
            ThemeConfig = """{"themeId":"minimal-light","bgColor":"#ffffff","buttonStyle":"outline","buttonColor":"#111111","buttonTextColor":"#111111","textColor":"#111111","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}"""
        };
        db.Pages.Add(page);
        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "page.create", new { page.Id, page.Slug });
        return Result<PageDto>.Ok(ToDto(page, 0));
    }

    public async Task<Result<PageDto>> UpdateAsync(Guid userId, Guid pageId, UpdatePageRequest req)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return Result<PageDto>.Fail("Página não encontrada.");

        if (page.Slug != req.Slug && await db.Pages.AnyAsync(p => p.Slug == req.Slug))
            return Result<PageDto>.Fail("Este endereço já está em uso.");

        var oldSlug = page.Slug;
        page.Slug = req.Slug;
        page.Title = req.Title;
        page.Bio = req.Bio;
        page.AvatarUrl = req.AvatarUrl ?? page.AvatarUrl;
        if (!string.IsNullOrEmpty(req.ThemeConfig)) page.ThemeConfig = req.ThemeConfig;
        page.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.PageBySlug(oldSlug));
        await cache.RemoveAsync(CacheKeys.PageBySlug(page.Slug));
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        var linkCount = await db.Links.CountAsync(l => l.PageId == pageId);
        return Result<PageDto>.Ok(ToDto(page, linkCount));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid pageId)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        var slug = page.Slug;
        db.Pages.Remove(page);
        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.PageBySlug(slug));
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        await audit.LogAsync(userId, "page.delete", new { pageId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> PublishAsync(Guid userId, Guid pageId)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        page.Status = page.Status == PageStatus.Published ? PageStatus.Draft : PageStatus.Published;
        page.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.PageBySlug(page.Slug));
        return Result<bool>.Ok(true);
    }

    public async Task<List<PageDto>> GetUserPagesAsync(Guid userId) =>
        await db.Pages
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PageDto(
                p.Id, p.UserId, p.Slug, p.Title, p.Bio, p.AvatarUrl, p.Status, p.ThemeConfig,
                p.Links.Count))
            .ToListAsync();

    public async Task<PageDto?> GetBySlugAsync(string slug)
    {
        var cached = await cache.GetAsync<PageDto>(CacheKeys.PageBySlug(slug));
        if (cached is not null) return cached;

        var page = await db.Pages
            .Include(p => p.Links)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PageStatus.Published);
        if (page is null) return null;

        var dto = ToDto(page, page.Links.Count);
        await cache.SetAsync(CacheKeys.PageBySlug(slug), dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<List<LinkDto>> GetLinksAsync(Guid pageId)
    {
        var cached = await cache.GetAsync<List<LinkDto>>(CacheKeys.PageLinks(pageId));
        if (cached is not null) return cached;

        var links = await db.Links
            .Where(l => l.PageId == pageId)
            .OrderBy(l => l.Order)
            .Select(l => new LinkDto(l.Id, l.PageId, l.Title, l.Url, l.IconName, l.IsActive, l.Order))
            .ToListAsync();

        await cache.SetAsync(CacheKeys.PageLinks(pageId), links, TimeSpan.FromMinutes(10));
        return links;
    }

    public async Task<LinkDto?> GetLinkByIdAsync(Guid linkId)
    {
        var link = await db.Links.FindAsync(linkId);
        if (link is null) return null;
        return new LinkDto(link.Id, link.PageId, link.Title, link.Url, link.IconName, link.IsActive, link.Order);
    }

    public async Task<bool> IsSlugAvailableAsync(string slug, Guid? excludePageId = null)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;
        var query = db.Pages.Where(p => p.Slug == slug);
        if (excludePageId.HasValue)
            query = query.Where(p => p.Id != excludePageId.Value);
        return !await query.AnyAsync();
    }

    static PageDto ToDto(Page p, int linkCount) =>
        new(p.Id, p.UserId, p.Slug, p.Title, p.Bio, p.AvatarUrl, p.Status, p.ThemeConfig, linkCount);
}
