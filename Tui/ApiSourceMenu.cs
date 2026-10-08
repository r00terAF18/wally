using Spectre.Console;
using wally.Sources;

namespace wally.Tui;

/// <summary>Interactive flow for API sources: search, pick one (or a random one), download.</summary>
public static class ApiSourceMenu
{
    /// <returns>The downloaded file, or null when nothing was found.</returns>
    public static async Task<string?> RunAsync(IWallpaperSource source, string query, bool random, Resolution? minResolution = null)
    {
        IReadOnlyList<WallpaperResult> results = [];
        await AnsiConsole.Status().StartAsync($"Searching {source.Name}...", async _ =>
            results = await source.SearchAsync(new SearchOptions(query, random, minResolution, null)));

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[red][[x]] No wallpapers found.[/]");
            return null;
        }

        WallpaperResult pick = random
            ? results[Random.Shared.Next(results.Count)]
            : AnsiConsole.Prompt(
                new SelectionPrompt<WallpaperResult>()
                    .Title($"Which wallpaper? [grey]({results.Count} results)[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more wallpapers)[/]")
                    .UseConverter(r => Markup.Escape($"{r.Width}x{r.Height}  {r.PageUrl ?? r.Id}{(r.Author is null ? "" : $"  by {r.Author}")}"))
                    .AddChoices(results));

        string path = "";
        bool existed = false;
        await AnsiConsole.Status().StartAsync($"Downloading {pick.FileName}...", async _ =>
            (path, existed) = await WallpaperDownloader.DownloadAsync(pick, source));

        AnsiConsole.MarkupLineInterpolated($"[green][[+]] {(existed ? "Already downloaded" : "Downloaded")}: {path}[/]");
        return path;
    }
}
