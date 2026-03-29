using System.Text.Json;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data.Entities;
using UmbLink.Infrastructure.Repositories;

namespace UmbLink.Application.Services;

public class AuditService(IAuditLogRepository auditRepo) : IAuditService
{
    public async Task LogAsync(Guid userId, string action, object? metadata = null)
    {
        await auditRepo.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata)
        });
    }
}
