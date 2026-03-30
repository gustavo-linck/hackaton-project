using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<List<AuditLog>> GetRecentAsync(int count = 20);
}
