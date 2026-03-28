namespace UmbLink.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid userId, string action, object? metadata = null);
}
