namespace UmbLink.Web;

public static class RedirectKnownDomains
{
    public static readonly HashSet<string> KnownDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "instagram.com", "www.instagram.com",
        "tiktok.com", "www.tiktok.com",
        "youtube.com", "www.youtube.com", "youtu.be",
        "x.com", "twitter.com", "www.twitter.com",
        "linkedin.com", "www.linkedin.com",
        "facebook.com", "www.facebook.com",
        "wa.me", "whatsapp.com",
        "t.me", "telegram.me",
        "github.com", "www.github.com",
        "open.spotify.com", "spotify.com",
        "pinterest.com", "www.pinterest.com",
        "twitch.tv", "www.twitch.tv",
        "behance.net", "www.behance.net",
        "dribbble.com", "www.dribbble.com",
        "medium.com", "www.medium.com",
        "substack.com",
        "discord.gg", "discord.com"
    };
}
