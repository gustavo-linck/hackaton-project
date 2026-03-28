namespace UmbLink.Application.DTOs;

public record LinkDto(
    Guid Id,
    Guid PageId,
    string Title,
    string Url,
    string? IconName,
    bool IsActive,
    int Order
);
