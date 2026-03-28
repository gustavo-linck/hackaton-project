using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.Interfaces;

public interface IPlanLimitService
{
    Task<Result<bool>> CanAddPageAsync(Guid userId);
    Task<Result<bool>> CanAddLinkAsync(Guid userId, Guid pageId);
    Task<Result<bool>> CanUseFeatureAsync(Guid userId, Feature feature);
    Task<PlanLimitDto> GetLimitsAsync(Guid userId);
}
