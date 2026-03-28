namespace UmbLink.Application.Models;

public record ThemeDefinition(string Id, string Name, bool IsPremium,
    string DefaultBg, string DefaultButtonColor, string DefaultButtonTextColor, string DefaultTextColor);

public static class Themes
{
    public static readonly List<ThemeDefinition> All =
    [
        new("minimal-light",   "Minimal Light",   false, "#ffffff", "#111111", "#ffffff", "#111111"),
        new("dark-pro",        "Dark Pro",         false, "#0f172a", "#6366f1", "#ffffff", "#f8fafc"),
        new("gradient-sunset", "Gradient Sunset",  true,  "#ffecd2", "#f97316", "#ffffff", "#1f2937"),
        new("glass-morphism",  "Glass",            true,  "#1a1a2e", "#e94560", "#ffffff", "#ffffff"),
    ];
}
