namespace UmbLink.Application.DTOs;

public record PlanDto(
    int Id,
    string Name,
    PlanLimitDto Limit,
    List<PlanPriceDto> Prices
);

public record PlanPriceDto(
    int Id,
    string BillingPeriod,
    decimal PricePerMonth,
    decimal TotalCharged,
    int DiscountPercent
);
