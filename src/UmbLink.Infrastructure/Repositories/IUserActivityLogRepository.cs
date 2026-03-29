using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IUserActivityLogRepository
{
    Task AddAsync(UserActivityLog log);
    Task<List<UserActivityLog>> GetByUserIdAsync(Guid userId, int take = 50);
}
