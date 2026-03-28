using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.DTOs;

public record AdminUserDto(
    Guid Id,
    string Name,
    string Email,
    string PlanName,
    SubscriptionStatus Status,
    bool IsActive,
    DateTime CreatedAt
);

public record AdminStatsDto(
    int TotalUsers,
    int TotalPages,
    int TotalClicks,
    decimal SimulatedRevenue
);
