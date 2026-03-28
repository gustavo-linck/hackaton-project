using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Application.Requests;

namespace UmbLink.Application.Services;

// Implementação completa no Task 12
public class PageService(
    UmbLink.Infrastructure.Data.AppDbContext db,
    IPlanLimitService limits,
    IAuditService audit) : IPageService
{
    public Task<Result<PageDto>> CreateAsync(Guid userId, CreatePageRequest req) => throw new NotImplementedException();
    public Task<Result<PageDto>> UpdateAsync(Guid userId, Guid pageId, UpdatePageRequest req) => throw new NotImplementedException();
    public Task<Result<bool>> DeleteAsync(Guid userId, Guid pageId) => throw new NotImplementedException();
    public Task<Result<bool>> PublishAsync(Guid userId, Guid pageId) => throw new NotImplementedException();
    public Task<List<PageDto>> GetUserPagesAsync(Guid userId) => throw new NotImplementedException();
    public Task<PageDto?> GetBySlugAsync(string slug) => throw new NotImplementedException();
    public Task<List<LinkDto>> GetLinksAsync(Guid pageId) => throw new NotImplementedException();
}
