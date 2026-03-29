using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UmbLink.Infrastructure.Identity;

namespace UmbLink.Infrastructure.Repositories;

public class AdminUserRepository(UserManager<AppUser> userManager) : IAdminUserRepository
{
    public async Task<List<AppUser>> GetUsersWithPlanAsync(string? search = null)
    {
        var query = userManager.Users
            .Include(u => u.Subscription).ThenInclude(s => s != null ? s.Plan : null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email!.Contains(search));

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<int> CountAsync() =>
        await userManager.Users.CountAsync();

    public async Task<int> CountNewSinceAsync(DateTime since) =>
        await userManager.Users.CountAsync(u => u.IsActive && u.CreatedAt >= since);
}
