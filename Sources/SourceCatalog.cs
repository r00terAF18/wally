using wally.Core;

namespace wally.Sources;

/// <param name="Cli">Usable from "wally get" (API-based, never prompts).</param>
public sealed record SourceInfo(string Name, string DisplayName, string Kind, bool Cli, bool NeedsKey, string Notes);

public static class SourceCatalog
{
    public static readonly IReadOnlyList<SourceInfo> All =
    [
        new("wallhaven", "Wallhaven", "API", true, false, "resolution/ratio filters; key only for NSFW"),
        new("pexels", "Pexels", "API", true, true, "free key; original-size photos"),
        new("wallpaperswide", "WallpapersWide", "scraper", false, false, "pick resolutions in the menu"),
        new("hdwallpapers", "HdWallpapers", "scraper", false, false, "pick resolutions in the menu"),
        new("konachan", "Konachan", "scraper", false, false, "work in progress"),
    ];

    public static IReadOnlyList<string> CliNames => All.Where(s => s.Cli).Select(s => s.Name).ToList();

    public static IWallpaperSource Create(string name, AppConfig config) => name.Trim().ToLowerInvariant() switch
    {
        "wallhaven" => new WallhavenSource(config.Wallhaven),
        "pexels" => new PexelsSource(config.Pexels),
        _ => throw new SourceException(
            $"Unknown or interactive-only source '{name}'. CLI sources: {string.Join(", ", CliNames)} (see 'wally sources').")
    };
}
