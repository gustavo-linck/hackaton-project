using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class LinkService(ILinkRepository linkRepo, IPageRepository pageRepo, IPlanLimitService limits, ICacheService cache) : ILinkService
{
    public async Task<Result<LinkDto>> AddAsync(Guid userId, Guid pageId, CreateLinkRequest req)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return Result<LinkDto>.Fail("Página não encontrada.");

        var canAdd = await limits.CanAddLinkAsync(userId, pageId);
        if (canAdd.IsFailure) return Result<LinkDto>.LimitExceeded(canAdd.LimitError!);

        var order = await linkRepo.GetMaxOrderAsync(pageId);
        var link = new Link
        {
            PageId = pageId,
            Title = req.Title,
            Url = req.Url,
            IconName = req.IconName,
            IsActive = true,
            Order = order + 1
        };
        await linkRepo.CreateAsync(link);
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        return Result<LinkDto>.Ok(ToDto(link));
    }

    public async Task<Result<LinkDto>> UpdateAsync(Guid userId, Guid linkId, UpdateLinkRequest req)
    {
        var link = await linkRepo.GetByIdAndUserAsync(linkId, userId);
        if (link is null) return Result<LinkDto>.Fail("Link não encontrado.");

        link.Title = req.Title;
        link.Url = req.Url;
        link.IconName = req.IconName;
        link.IsActive = req.IsActive;
        await linkRepo.UpdateAsync(link);
        await cache.RemoveAsync(CacheKeys.PageLinks(link.PageId));
        return Result<LinkDto>.Ok(ToDto(link));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid linkId)
    {
        var link = await linkRepo.GetByIdAndUserAsync(linkId, userId);
        if (link is null) return Result<bool>.Fail("Link não encontrado.");

        var pageId = link.PageId;
        await linkRepo.DeleteAsync(link);
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ReorderAsync(Guid userId, Guid pageId, List<Guid> orderedIds)
    {
        var page = await pageRepo.GetByIdAndUserAsync(pageId, userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        var links = await linkRepo.GetByPageIdAsync(pageId);
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var link = links.FirstOrDefault(l => l.Id == orderedIds[i]);
            if (link != null) link.Order = i + 1;
        }
        await linkRepo.SaveChangesAsync();
        await cache.RemoveAsync(CacheKeys.PageLinks(pageId));
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ToggleActiveAsync(Guid userId, Guid linkId)
    {
        var link = await linkRepo.GetByIdAndUserAsync(linkId, userId);
        if (link is null) return Result<bool>.Fail("Link não encontrado.");

        link.IsActive = !link.IsActive;
        await linkRepo.UpdateAsync(link);
        await cache.RemoveAsync(CacheKeys.PageLinks(link.PageId));
        return Result<bool>.Ok(true);
    }

    static LinkDto ToDto(Link l) => new(l.Id, l.PageId, l.Title, l.Url, l.IconName, l.IsActive, l.Order);
}
