namespace UmbLink.Application.Models;

public record ThemeDefinition(string Id, string Name, bool IsPremium,
    string DefaultBg, string DefaultButtonColor, string DefaultButtonTextColor, string DefaultTextColor);

public static class Themes
{
    public static readonly List<ThemeDefinition> All =
    [
        // Free
        new("minimal-light",   "Minimal Light",   false, "#ffffff", "#111111", "#ffffff", "#111111"),
        new("dark-pro",        "Dark Pro",         false, "#0f172a", "#6366f1", "#ffffff", "#f8fafc"),
        new("pastel-dream",    "Pastel Dream",     false, "#fdf2f8", "#ec4899", "#ffffff", "#831843"),
        new("earth-tone",      "Earth Tone",       false, "#fef3e2", "#b45309", "#ffffff", "#78350f"),
        // Premium
        new("gradient-sunset", "Gradient Sunset",  true,  "#ffecd2", "#f97316", "#ffffff", "#1f2937"),
        new("glass-morphism",  "Glass",            true,  "#1a1a2e", "#e94560", "#ffffff", "#ffffff"),
        new("neon-dark",       "Neon Dark",        true,  "#0a0a0a", "#00ff88", "#000000", "#e0e0e0"),
        new("mono-gray",       "Mono Gray",        true,  "#f5f5f5", "#404040", "#ffffff", "#1a1a1a"),
        new("retro-warm",      "Retro Warm",       true,  "#fdf6e3", "#cb4b16", "#fdf6e3", "#073642"),
        new("ocean-blue",      "Ocean Blue",       true,  "#0c1445", "#00b4d8", "#0c1445", "#caf0f8"),
        new("high-contrast",   "High Contrast",    true,  "#000000", "#ffff00", "#000000", "#ffffff"),
        new("lavender-mist",   "Lavender Mist",    true,  "#f0e6ff", "#7c3aed", "#ffffff", "#4c1d95"),
    ];
}

public record PageTemplate(
    string Id,
    string Name,
    string Description,
    string Category,
    string Icon,
    string ThemeConfig,
    string ThemeId = "minimal-light"
);

public static class Templates
{
    public static readonly List<PageTemplate> All =
    [
        new("creator", "Creator", "Para influenciadores e criadores de conteúdo", "criador", "bi-camera-reels",
            """{"themeId":"gradient-sunset","bgColor":"#ffecd2","buttonStyle":"pill","buttonColor":"#f97316","buttonTextColor":"#ffffff","textColor":"#1f2937","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}""",
            "gradient-sunset"),

        new("dev-portfolio", "Dev / Portfólio", "Para desenvolvedores e portfólios técnicos", "portfolio", "bi-code-slash",
            """{"themeId":"dark-pro","bgColor":"#0f172a","buttonStyle":"rounded","buttonColor":"#6366f1","buttonTextColor":"#ffffff","textColor":"#f8fafc","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"square"}""",
            "dark-pro"),

        new("business", "Business", "Para negócios, lojas e serviços", "negócio", "bi-briefcase",
            """{"themeId":"minimal-light","bgColor":"#ffffff","buttonStyle":"rounded","buttonColor":"#111111","buttonTextColor":"#ffffff","textColor":"#111111","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}""",
            "minimal-light"),

        new("artist", "Artista", "Para artistas visuais, fotógrafos e designers", "portfolio", "bi-palette",
            """{"themeId":"glass-morphism","bgColor":"#1a1a2e","buttonStyle":"pill","buttonColor":"#e94560","buttonTextColor":"#ffffff","textColor":"#ffffff","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}""",
            "glass-morphism"),

        new("musician", "Músico", "Para músicos, bandas e produtores", "criador", "bi-music-note-beamed",
            """{"themeId":"neon-dark","bgColor":"#0a0a0a","buttonStyle":"rounded","buttonColor":"#00ff88","buttonTextColor":"#000000","textColor":"#e0e0e0","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}""",
            "neon-dark"),

        new("minimalist", "Minimalista", "Apenas o essencial — limpo e direto", "portfolio", "bi-dash-lg",
            """{"themeId":"minimal-light","bgColor":"#ffffff","buttonStyle":"outline","buttonColor":"#111111","buttonTextColor":"#111111","textColor":"#111111","titleFont":"inter","linkFont":"inter","spacing":"normal","avatarShape":"circle"}""",
            "minimal-light"),
    ];

    public static readonly List<string> Categories = ["todos", "criador", "negócio", "portfolio"];
}
