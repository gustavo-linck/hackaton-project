using UmbLink.Application.DTOs;
namespace UmbLink.Application.Interfaces;

public interface IMetricsService
{
    Task<MetricsSummaryDto> GetPageMetricsAsync(Guid userId, Guid pageId, int days);
    Task TrackClickAsync(Guid linkId, string? userAgent, string? referrer);
    Task TrackViewAsync(Guid pageId, string? referrer);
}
