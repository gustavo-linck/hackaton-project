using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
namespace UmbLink.Application.Interfaces;

public interface IPageService
{
    Task<Result<PageDto>> CreateAsync(Guid userId, CreatePageRequest req);
    Task<Result<PageDto>> UpdateAsync(Guid userId, Guid pageId, UpdatePageRequest req);
    Task<Result<bool>> DeleteAsync(Guid userId, Guid pageId);
    Task<Result<bool>> PublishAsync(Guid userId, Guid pageId);
    Task<List<PageDto>> GetUserPagesAsync(Guid userId);
    Task<PageDto?> GetBySlugAsync(string slug);
    Task<List<LinkDto>> GetLinksAsync(Guid pageId);
    Task<LinkDto?> GetLinkByIdAsync(Guid linkId);
    Task<bool> IsSlugAvailableAsync(string slug, Guid? excludePageId = null);
}
