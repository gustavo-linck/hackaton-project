using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class UserActivityLogRepository(AppDbContext db) : IUserActivityLogRepository
{
    public async Task AddAsync(UserActivityLog log)
    {
        db.UserActivityLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<UserActivityLog>> GetByUserIdAsync(Guid userId, int take = 50)
    {
        return await db.UserActivityLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync();
    }
}
