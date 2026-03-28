using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;
namespace UmbLink.Application.Interfaces;

public interface ILinkService
{
    Task<Result<LinkDto>> AddAsync(Guid userId, Guid pageId, CreateLinkRequest req);
    Task<Result<LinkDto>> UpdateAsync(Guid userId, Guid linkId, UpdateLinkRequest req);
    Task<Result<bool>> DeleteAsync(Guid userId, Guid linkId);
    Task<Result<bool>> ReorderAsync(Guid userId, Guid pageId, List<Guid> orderedIds);
    Task<Result<bool>> ToggleActiveAsync(Guid userId, Guid linkId);
}
