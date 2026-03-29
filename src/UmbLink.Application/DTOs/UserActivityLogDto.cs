namespace UmbLink.Application.DTOs;

public record UserActivityLogDto(
    Guid Id,
    string Action,
    string? Details,
    string? IpAddress,
    DateTime CreatedAt
);
