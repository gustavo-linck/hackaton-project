using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class UserActivityLogService(IUserActivityLogRepository repo) : IUserActivityLogService
{
    public async Task LogAsync(Guid userId, string action, string? details = null, string? ipAddress = null)
    {
        await repo.AddAsync(new UserActivityLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = ipAddress
        });
    }

    public async Task<List<UserActivityLogDto>> GetLogsAsync(Guid userId, int take = 50)
    {
        var logs = await repo.GetByUserIdAsync(userId, take);
        return logs.Select(l => new UserActivityLogDto(l.Id, l.Action, l.Details, l.IpAddress, l.CreatedAt)).ToList();
    }
}
