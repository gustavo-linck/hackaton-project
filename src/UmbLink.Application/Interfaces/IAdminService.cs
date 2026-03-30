using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
namespace UmbLink.Application.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null);
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<List<AdminAuditLogDto>> GetRecentAuditLogsAsync(int count = 20);
    Task<Result<bool>> SuspendUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ReactivateUserAsync(Guid adminId, Guid targetUserId);
    Task<Result<bool>> ChangePlanAsync(Guid adminId, Guid targetUserId, int planId);
    Task<Result<bool>> GrantTrialAsync(Guid adminId, Guid targetUserId, int planId, int durationDays);
}
