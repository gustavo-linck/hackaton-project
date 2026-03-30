using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log)
    {
        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count = 20) =>
        await db.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
}
