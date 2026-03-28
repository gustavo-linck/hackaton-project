using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;

namespace UmbLink.Application.Services;

// Implementação completa no Task 13
public class LinkService(
    UmbLink.Infrastructure.Data.AppDbContext db,
    IPlanLimitService limits) : ILinkService
{
    public Task<Result<LinkDto>> AddAsync(Guid userId, Guid pageId, CreateLinkRequest req) => throw new NotImplementedException();
    public Task<Result<LinkDto>> UpdateAsync(Guid userId, Guid linkId, UpdateLinkRequest req) => throw new NotImplementedException();
    public Task<Result<bool>> DeleteAsync(Guid userId, Guid linkId) => throw new NotImplementedException();
    public Task<Result<bool>> ReorderAsync(Guid userId, Guid pageId, List<Guid> orderedIds) => throw new NotImplementedException();
    public Task<Result<bool>> ToggleActiveAsync(Guid userId, Guid linkId) => throw new NotImplementedException();
}
