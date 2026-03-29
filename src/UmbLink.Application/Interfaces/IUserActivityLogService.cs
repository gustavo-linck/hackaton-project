using UmbLink.Application.DTOs;

namespace UmbLink.Application.Interfaces;

public interface IUserActivityLogService
{
    Task LogAsync(Guid userId, string action, string? details = null, string? ipAddress = null);
    Task<List<UserActivityLogDto>> GetLogsAsync(Guid userId, int take = 50);
}
