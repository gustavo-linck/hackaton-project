using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Infrastructure.Repositories;

public interface IPlanLimitRepository
{
    Task<int?> GetPlanIdByUserAsync(Guid userId);
    Task<int?> GetFreePlanIdAsync();
    Task<PlanLimit?> GetByPlanIdAsync(int planId);
    Task<string> GetPlanNameByUserAsync(Guid userId);
    Task<int> CountPagesByUserAsync(Guid userId, PageStatus? excludeStatus = null);
    Task<int> CountLinksByPageAsync(Guid pageId);
}
