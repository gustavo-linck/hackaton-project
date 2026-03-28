using Microsoft.EntityFrameworkCore;
using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class LinkService(AppDbContext db, IPlanLimitService limits) : ILinkService
{
    public async Task<Result<LinkDto>> AddAsync(Guid userId, Guid pageId, CreateLinkRequest req)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return Result<LinkDto>.Fail("Página não encontrada.");

        var canAdd = await limits.CanAddLinkAsync(userId, pageId);
        if (canAdd.IsFailure) return Result<LinkDto>.LimitExceeded(canAdd.LimitError!);

        var order = await db.Links.Where(l => l.PageId == pageId).MaxAsync(l => (int?)l.Order) ?? 0;
        var link = new Link
        {
            PageId = pageId,
            Title = req.Title,
            Url = req.Url,
            IconName = req.IconName,
            IsActive = true,
            Order = order + 1
        };
        db.Links.Add(link);
        await db.SaveChangesAsync();
        return Result<LinkDto>.Ok(ToDto(link));
    }

    public async Task<Result<LinkDto>> UpdateAsync(Guid userId, Guid linkId, UpdateLinkRequest req)
    {
        var link = await db.Links.Include(l => l.Page)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Page.UserId == userId);
        if (link is null) return Result<LinkDto>.Fail("Link não encontrado.");

        link.Title = req.Title;
        link.Url = req.Url;
        link.IconName = req.IconName;
        link.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Result<LinkDto>.Ok(ToDto(link));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid linkId)
    {
        var link = await db.Links.Include(l => l.Page)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Page.UserId == userId);
        if (link is null) return Result<bool>.Fail("Link não encontrado.");

        db.Links.Remove(link);
        await db.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ReorderAsync(Guid userId, Guid pageId, List<Guid> orderedIds)
    {
        var page = await db.Pages.FirstOrDefaultAsync(p => p.Id == pageId && p.UserId == userId);
        if (page is null) return Result<bool>.Fail("Página não encontrada.");

        var links = await db.Links.Where(l => l.PageId == pageId).ToListAsync();
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var link = links.FirstOrDefault(l => l.Id == orderedIds[i]);
            if (link != null) link.Order = i + 1;
        }
        await db.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ToggleActiveAsync(Guid userId, Guid linkId)
    {
        var link = await db.Links.Include(l => l.Page)
            .FirstOrDefaultAsync(l => l.Id == linkId && l.Page.UserId == userId);
        if (link is null) return Result<bool>.Fail("Link não encontrado.");

        link.IsActive = !link.IsActive;
        await db.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }

    static LinkDto ToDto(Link l) => new(l.Id, l.PageId, l.Title, l.Url, l.IconName, l.IsActive, l.Order);
}
