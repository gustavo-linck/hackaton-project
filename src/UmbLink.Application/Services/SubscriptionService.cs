using UmbLink.Application.DTOs;
using UmbLink.Application.Interfaces;
using UmbLink.Application.Models;
using UmbLink.Infrastructure.Data.Entities;

namespace UmbLink.Application.Services;

// Implementação completa no Task 19
public class SubscriptionService(
    UmbLink.Infrastructure.Data.AppDbContext db,
    IAuditService audit) : ISubscriptionService
{
    public Task<SubscriptionDto> GetAsync(Guid userId) => throw new NotImplementedException();
    public Task<Result<SubscriptionDto>> StartTrialAsync(Guid userId, int planId, BillingPeriod period) => throw new NotImplementedException();
    public Task<Result<SubscriptionDto>> ActivatePlanAsync(Guid userId, int planId, BillingPeriod period) => throw new NotImplementedException();
    public Task<Result<bool>> CancelAsync(Guid userId) => throw new NotImplementedException();
    public Task ProcessExpiredTrialsAsync() => throw new NotImplementedException();
}
