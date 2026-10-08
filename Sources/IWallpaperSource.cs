using System.Diagnostics.CodeAnalysis;

namespace wally.Sources;

/// <summary>A non-interactive wallpaper source (an API, not an HTML scraper). Never prompts.</summary>
public interface IWallpaperSource
{
    /// <summary>Name used on the command line, e.g. "wallhaven".</summary>
    string Name { get; }

    /// <summary>Sub-folder of the download dir, e.g. "Wallhaven".</summary>
    string FolderName { get; }

    Task<IReadOnlyList<WallpaperResult>> SearchAsync(SearchOptions options, CancellationToken ct = default);
}

public sealed record SearchOptions(string? Query, bool Random, Resolution? MinResolution, string? Ratios);

public sealed record WallpaperResult(
    string Id,
    string DownloadUrl,
    string FileName,
    int Width,
    int Height,
    string? PageUrl = null,
    string? Author = null);

public readonly record struct Resolution(int Width, int Height)
{
    public static bool TryParse(string? text, [NotNullWhen(true)] out Resolution? resolution)
    {
        resolution = null;
        string[] parts = (text ?? "").ToLowerInvariant().Split('x', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h) && w > 0 && h > 0)
            resolution = new Resolution(w, h);
        return resolution is not null;
    }

    public override string ToString() => $"{Width}x{Height}";
}

public sealed class SourceException(string message, Exception? inner = null) : Exception(message, inner);
