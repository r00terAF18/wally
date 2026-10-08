using Spectre.Console;
using wally.Core;
using wally.Downloaders;
using wally.Setters;
using wally.Sources;

namespace wally.Tui;

/// <summary>The original interactive menu. Runs for plain "wally" and "wally tui".</summary>
public static class InteractiveMenu
{
    private const string Logo = """

                                       _ _       
                        __      ____ _| | |_   _ 
                        \ \ /\ / / _` | | | | | |
                         \ V  V / (_| | | | |_| |
                          \_/\_/ \__,_|_|_|\__, |
                                           |___/ 


                        """;

    public static async Task<int> RunAsync()
    {
        if (Console.IsInputRedirected || !AnsiConsole.Profile.Capabilities.Interactive)
        {
            Log.Error("The interactive menu needs a terminal. See 'wally --help' for the non-interactive commands.");
            return 1;
        }

        Console.Clear();
        AnsiConsole.WriteLine(Logo);

        string website = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which Website to download from?")
                .PageSize(7)
                .MoreChoicesText("[grey](Move up and down to reveal more Websites)[/]")
                .AddChoices("Wallhaven", "Konachan (SFW)", "Konachan (NSFW)", "WallpapersWide", "HdWallpapers", "Pexels"));

        string randomOrLatest = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey][[Optional]][/] [green]Random or latest (R/L)[/]?")
                .AddChoice("R")
                .AddChoice("L")
                .DefaultValue("L")
                .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
                .AllowEmpty());
        bool random = randomOrLatest == "R";

        string query = "";
        if (!random)
            query = AnsiConsole.Ask<string>("What to search for?");

        string res = website is "Wallhaven" or "Pexels"
            ? "S"
            : AnsiConsole.Prompt(
                new TextPrompt<string>("Download single resolution(Single Resolution/Multiple Resolutions) (S/M)?")
                    .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
                    .DefaultValue("S")
                    .AddChoice("S")
                    .AddChoice("M"));

        AppConfig config = ConfigStore.Current;
        string? downloaded = null;
        try
        {
            switch (website)
            {
                case "Wallhaven":
                    downloaded = await ApiSourceMenu.RunAsync(new WallhavenSource(config.Wallhaven), query, random);
                    break;
                case "Pexels":
                    ApiKeys.EnsurePexelsKey();
                    downloaded = await ApiSourceMenu.RunAsync(new PexelsSource(config.Pexels), query, random);
                    break;
                case "Konachan (SFW)":
                    KonachanSfw k = new(query);
                    if (res == "S")
                        await k.DownloadAsync(random);
                    else
                        await k.MultiDownloadAsync(random);
                    break;
                case "Konachan (NSFW)":
                    KonachanNsfw kNsfw = new(query);
                    if (res == "S")
                        await kNsfw.DownloadAsync(random);
                    else
                        await kNsfw.MultiDownloadAsync(random);
                    break;
                case "WallpapersWide":
                    WallpapersWide w = new(query);
                    if (res == "S")
                        await w.DownloadAsync(random);
                    else
                        await w.MultiDownloadAsync();
                    downloaded = w.DestFile;
                    break;
                case "HdWallpapers":
                    HdWallpaper h = new(query);
                    if (res == "S")
                        await h.DownloadAsync(random);
                    else
                        await h.MultiDownloadAsync();
                    downloaded = h.DestFile;
                    break;
            }
        }
        catch (NotImplementedException)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow][[!]] {website} isn't implemented yet.[/]");
            return 1;
        }
        catch (Exception ex) when (ex is InvalidOperationException or SourceException or ConfigException or HttpRequestException)
        {
            AnsiConsole.MarkupLineInterpolated($"[red][[x]] {ex.Message}[/]");
            return 1;
        }

        // Before this, the downloaders had a private SetWallpaper() that was never called.
        if (!string.IsNullOrEmpty(downloaded) && File.Exists(downloaded)
            && AnsiConsole.Confirm("Set it as your wallpaper?"))
        {
            SetResult set = await WallpaperSetters.SetAsync(downloaded, config.Setter);
            if (set.Success)
                AnsiConsole.MarkupLineInterpolated($"[green][[+]] {set.Message}[/]");
            else
                AnsiConsole.MarkupLineInterpolated($"[red][[x]] {set.Message}[/]");
            return set.Success ? 0 : 1;
        }

        return 0;
    }
}
