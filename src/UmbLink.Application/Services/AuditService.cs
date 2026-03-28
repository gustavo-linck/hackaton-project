using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UmbLink.Application.Interfaces;
using UmbLink.Infrastructure.Data;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    public async Task LogAsync(Guid userId, string action, object? metadata = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata)
        });
        await db.SaveChangesAsync();
    }
}
