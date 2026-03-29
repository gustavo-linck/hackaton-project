using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Repositories;

public interface IAdminUserRepository
{
    Task<List<AppUser>> GetUsersWithPlanAsync(string? search = null);
    Task<int> CountAsync();
    Task<int> CountNewSinceAsync(DateTime since);
}
