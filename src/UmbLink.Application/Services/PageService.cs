using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class PageService(IPageRepository pageRepo, ILinkRepository linkRepo, IPlanLimitService limits, IAuditService audit, ICacheService cache) : IPageService
{
    public async Task<Result<PageDto>> CreateAsync(Guid userId, CreatePageRequest req)
    {
        var canAdd = await limits.CanAddPageAsync(userId);
        if (canAdd.IsFailure) return Result<PageDto>.LimitExceeded(canAdd.LimitError!);

        if (await pageRepo.SlugExistsAsync(req.Slug))
            return Result<PageDto>.Fail("Este endereço já está em uso.");

        var template = !string.IsNullOrEmpty(req.TemplateId)
            ? Models.Templates.All.FirstOrDefault(t => t.Id == req.TemplateId)
            : null;

        var page = new Page
        {
            UserId = userId,
            Slug = req.Slug,
            Title = req.Title,
            Bio = req.Bio,
            Status = PageStatus.Draft,
            ThemeConfig = template?.ThemeConfig
                ?? """{"themeId":"minimal-light","bgColor":"#ffffff","buttonStyle":"outline","buttonColor":"#111111","buttonTextColor":"#111111","textColor":"#111111","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}"""
        };
        await pageRepo.CreateAsync(page);
        await audit.LogAsync(userId, "page.create", new { page.Id, page.Slug });
        return Result<PageDto>.Ok(ToDto(page, 0));
    }

    public async Task<Result<PageDto>> UpdateAsync(Guid userId, Guid pageId, UpdatePageRequest req)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return Result<PageDto>.Fail("Página não encontrada.");

        if (page.Slug != req.Slug && await pageRepo.SlugExistsAsync(req.Slug))
            return Result<PageDto>.Fail("Este endereço já está em uso.");

        var oldSlug = page.Slug;
        page.Slug = req.Slug;
        page.Title = req.Title;
        page.Bio = req.Bio;
        page.AvatarUrl = req.AvatarUrl ?? page.AvatarUrl;
        if (!string.IsNullOrEmpty(req.ThemeConfig)) page.ThemeConfig = req.ThemeConfig;

        await pageRepo.UpdateAsync(page);
        await cache.RemoveAsync(CacheKeys.PageBySlug(oldSlug));
        await cache.RemoveAsync(CacheKeys.PageBySlug(page.Slug));
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        var linkCount = await pageRepo.GetLinkCountAsync(pageId);
        return Result<PageDto>.Ok(ToDto(page, linkCount));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid pageId)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        var slug = page.Slug;
        await pageRepo.DeleteAsync(page);
        await cache.RemoveAsync(CacheKeys.PageBySlug(slug));
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        await audit.LogAsync(userId, "page.delete", new { pageId });
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> PublishAsync(Guid userId, Guid pageId)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        page.Status = page.Status == PageStatus.Published ? PageStatus.Draft : PageStatus.Published;
        await pageRepo.UpdateAsync(page);
        await cache.RemoveAsync(CacheKeys.PageBySlug(page.Slug));
        return Result<bool>.Ok(true);
    }

    public async Task<List<PageDto>> GetUserPagesAsync(Guid userId)
    {
        var pages = await pageRepo.GetByUserIdWithCountsAsync(userId);
        return pages.Select(t => ToDto(t.page, t.linkCount)).ToList();
    }

    public async Task<PageDto?> GetBySlugAsync(string slug)
    {
        var cached = await cache.GetAsync<PageDto>(CacheKeys.PageBySlug(slug));
        if (cached is not null) return cached;

        var page = await pageRepo.GetPublishedBySlugWithLinksAsync(slug);
        if (page is null) return null;

        var dto = ToDto(page, page.Links.Count);
        await cache.SetAsync(CacheKeys.PageBySlug(slug), dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<List<LinkDto>> GetLinksAsync(Guid pageId)
    {
        var cached = await cache.GetAsync<List<LinkDto>>(CacheKeys.PageLinks(pageId));
        if (cached is not null) return cached;

        var links = await linkRepo.GetByPageIdAsync(pageId);
        var dtos = links.Select(l => new LinkDto(l.Id, l.PageId, l.Title, l.Url, l.IconName, l.IsActive, l.Order)).ToList();

        await cache.SetAsync(CacheKeys.PageLinks(pageId), dtos, TimeSpan.FromMinutes(10));
        return dtos;
    }

    public async Task<LinkDto?> GetLinkByIdAsync(Guid linkId)
    {
        var link = await linkRepo.GetByIdAsync(linkId);
        if (link is null) return null;
        return new LinkDto(link.Id, link.PageId, link.Title, link.Url, link.IconName, link.IsActive, link.Order);
    }

    public async Task<bool> IsSlugAvailableAsync(string slug, Guid? excludePageId = null)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;
        return !await pageRepo.SlugExistsAsync(slug, excludePageId);
    }

    static PageDto ToDto(Page p, int linkCount) =>
        new(p.Id, p.UserId, p.Slug, p.Title, p.Bio, p.AvatarUrl, p.Status, p.ThemeConfig, linkCount);
}
