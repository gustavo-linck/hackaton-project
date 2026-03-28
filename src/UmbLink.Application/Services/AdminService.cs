using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;

namespace UmbLink.Application.Services;

// Implementação completa no Task 23
public class AdminService(
    UmbLink.Infrastructure.Data.AppDbContext db,
    IAuditService audit,
    Microsoft.AspNetCore.Identity.UserManager<UmbLink.Infrastructure.Identity.AppUser> userManager) : IAdminService
{
    public Task<List<AdminUserDto>> GetUsersAsync(string? search = null) => throw new NotImplementedException();
    public Task<AdminStatsDto> GetGlobalStatsAsync() => throw new NotImplementedException();
    public Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId) => throw new NotImplementedException();
    public Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId) => throw new NotImplementedException();
    public Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId) => throw new NotImplementedException();
}
