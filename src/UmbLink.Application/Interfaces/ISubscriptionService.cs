using UmbLink.Application.DTOs;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionDto> GetAsync(Guid userId);
    Task<Result<SubscriptionDto>> StartTrialAsync(Guid userId, int planId, BillingPeriod period);
    Task<Result<SubscriptionDto>> ActivatePlanAsync(Guid userId, int planId, BillingPeriod period);
    Task<Result<bool>> CancelAsync(Guid userId);
    Task ProcessExpiredTrialsAsync();
}
