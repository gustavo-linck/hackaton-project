using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
}
