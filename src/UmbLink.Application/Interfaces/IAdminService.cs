using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
namespace UmbLink.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null);
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId);
}
