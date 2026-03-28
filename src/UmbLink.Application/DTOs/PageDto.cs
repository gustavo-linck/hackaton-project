using UmbLink.Infrastructure.Data.Entities;
namespace UmbLink.Application.DTOs;

public record PageDto(
    Guid Id,
    string Slug,
    string Title,
    string? Bio,
    string? AvatarUrl,
    PageStatus Status,
    string ThemeConfig,
    int LinkCount
);
