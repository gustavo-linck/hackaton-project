namespace UmbLink.Application;

public static class CacheKeys
{
    public static string PageBySlug(string slug) => $"page:slug:{slug}";
    public static string PageLinks(Guid pageId) => $"page:links:{pageId}";
    public static string UserSubscription(Guid userId) => $"user:sub:{userId}";
}
