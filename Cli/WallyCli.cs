using System.CommandLine;
using Spectre.Console;
using wally.Core;
using wally.Setters;
using wally.Sources;
using wally.Tui;

namespace wally.Cli;

/// <summary>
/// Command-line layer built on System.CommandLine 2.0: it's reflection-free (explicit
/// Option/Argument objects and GetValue calls), so it is trim- and Native-AOT-safe, unlike
/// Spectre.Console.Cli whose settings binding is reflection-based.
///
/// Non-interactive commands never prompt. Status goes to stderr, results (paths) to stdout.
/// </summary>
public static class WallyCli
{
    // Recursive: accepted by every command that sets a wallpaper.
    private static readonly Option<string?> SetterOption = new("--setter")
    {
        Description = "Force a wallpaper backend instead of auto-detecting it",
        Recursive = true
    };

    public static RootCommand Build()
    {
        SetterOption.AcceptOnlyFromAmong(WallpaperSetters.Names);

        var root = new RootCommand(
            "wally - a terminal-first wallpaper manager.\nRun it without arguments for the interactive menu.");
        root.Options.Add(SetterOption);
        root.Subcommands.Add(TuiCommand());
        root.Subcommands.Add(GetCommand());
        root.Subcommands.Add(SetCommand());
        root.Subcommands.Add(RandomCommand());
        root.Subcommands.Add(SourcesCommand());
        root.Subcommands.Add(ConfigCommand());

        // Plain "wally" keeps the classic interactive menu.
        root.SetAction((_, _) => InteractiveMenu.RunAsync());
        return root;
    }

    private static Command TuiCommand()
    {
        var command = new Command("tui", "Open the interactive menu (same as running wally without arguments)");
        command.SetAction((_, _) => InteractiveMenu.RunAsync());
        return command;
    }

    private static Command GetCommand()
    {
        var query = new Argument<string?>("query")
        {
            Description = "What to search for (optional with --random)",
            Arity = ArgumentArity.ZeroOrOne
        };
        var source = new Option<string?>("--source", "-s")
        {
            Description = $"Where to download from: {string.Join(", ", SourceCatalog.CliNames)} [default: config defaultSource, else wallhaven]"
        };
        source.AcceptOnlyFromAmong([.. SourceCatalog.CliNames]);
        var random = new Option<bool>("--random", "-r") { Description = "Pick a random result instead of the best match" };
        var resolution = new Option<string?>("--resolution")
        {
            Description = "Minimum resolution, e.g. 2560x1440",
            HelpName = "WxH"
        };
        resolution.Validators.Add(result =>
        {
            string? value = result.GetValueOrDefault<string?>();
            if (value is not null && !Resolution.TryParse(value, out _))
                result.AddError($"Invalid resolution '{value}'. Use WIDTHxHEIGHT, e.g. 1920x1080.");
        });
        var ratio = new Option<string?>("--ratio") { Description = "Aspect ratio(s), e.g. 16x9 or 16x9,21x9", HelpName = "WxH" };
        var set = new Option<bool>("--set") { Description = "Set the downloaded wallpaper right away" };

        var command = new Command("get", "Search a source, download one wallpaper and print its path")
        {
            query, source, random, resolution, ratio, set
        };

        command.SetAction((parse, ct) => Guard(async () =>
        {
            AppConfig config = ConfigStore.Current;
            string? text = parse.GetValue(query);
            bool pickRandom = parse.GetValue(random);
            Resolution.TryParse(parse.GetValue(resolution), out Resolution? minResolution);

            IWallpaperSource wallpaperSource = SourceCatalog.Create(parse.GetValue(source) ?? config.DefaultSource, config);

            Log.Info(string.IsNullOrWhiteSpace(text)
                ? $"Searching {wallpaperSource.Name} ({(pickRandom ? "random" : "top list")})..."
                : $"Searching {wallpaperSource.Name} for \"{text}\"...");

            IReadOnlyList<WallpaperResult> results = await wallpaperSource.SearchAsync(
                new SearchOptions(text, pickRandom, minResolution, parse.GetValue(ratio)), ct);
            if (results.Count == 0)
            {
                Log.Error("No wallpapers found. Try another query or drop the --resolution/--ratio filters.");
                return 1;
            }

            WallpaperResult pick = pickRandom ? results[Random.Shared.Next(results.Count)] : results[0];
            Log.Info($"Picked {pick.Width}x{pick.Height} {pick.PageUrl ?? pick.Id}{(pick.Author is null ? "" : $" by {pick.Author}")}");

            var (path, existed) = await WallpaperDownloader.DownloadAsync(pick, wallpaperSource, ct);
            Log.Info(existed ? $"Already downloaded: {path}" : $"Downloaded: {path}");
            Console.WriteLine(path);

            return parse.GetValue(set) ? await SetAndReportAsync(path, parse, config, ct) : 0;
        }, ct));

        return command;
    }

    private static Command SetCommand()
    {
        var file = new Argument<string>("file") { Description = "Image file to use as wallpaper" };
        var command = new Command("set", "Set an image file as the wallpaper") { file };

        command.SetAction((parse, ct) => Guard(async () =>
        {
            string path = Path.GetFullPath(Paths.ExpandHome(parse.GetValue(file)!));
            if (!File.Exists(path))
            {
                Log.Error($"File not found: {path}");
                return 1;
            }

            if (!Library.IsImage(path))
                Log.Warn($"{Path.GetExtension(path)} doesn't look like an image; trying anyway.");

            return await SetAndReportAsync(path, parse, ConfigStore.Current, ct);
        }, ct));

        return command;
    }

    private static Command RandomCommand()
    {
        var dir = new Option<string?>("--dir", "-d")
        {
            Description = "Folder to pick from, searched recursively [default: the download dir]",
            HelpName = "folder"
        };
        var command = new Command("random", "Set a random wallpaper from your local collection and print its path") { dir };

        command.SetAction((parse, ct) => Guard(async () =>
        {
            AppConfig config = ConfigStore.Current;
            string? requested = parse.GetValue(dir);
            string root = requested is null ? config.WallpaperRoot : Paths.ExpandHome(requested);
            if (!Directory.Exists(root))
            {
                Log.Error($"Folder not found: {root}");
                return 1;
            }

            List<string> images = Library.EnumerateImages(root);
            if (images.Count == 0)
            {
                Log.Error($"No images in {root}. Download some with 'wally get <query>'.");
                return 1;
            }

            string pick = images[Random.Shared.Next(images.Count)];
            Log.Info($"Picked {pick} (1 of {images.Count})");
            Console.WriteLine(pick);
            return await SetAndReportAsync(pick, parse, config, ct);
        }, ct));

        return command;
    }

    private static Command SourcesCommand()
    {
        var command = new Command("sources", "List the wallpaper sources");
        command.SetAction(_ =>
        {
            AppConfig config = ConfigStore.Current;
            var table = new Table().Border(TableBorder.Rounded)
                .AddColumns("Source", "Type", "wally get", "API key", "Notes");

            foreach (SourceInfo s in SourceCatalog.All)
            {
                string key = !s.NeedsKey ? "[grey]not needed[/]"
                    : HasKey(config, s.Name) ? "[green]configured[/]" : "[yellow]missing[/]";
                string name = s.Name.Equals(config.DefaultSource, StringComparison.OrdinalIgnoreCase)
                    ? $"[bold]{s.Name}[/] [grey](default)[/]"
                    : s.Name;
                table.AddRow(name, s.Kind, s.Cli ? "[green]yes[/]" : "[grey]menu only[/]", key, Markup.Escape(s.Notes));
            }

            AnsiConsole.Write(table);
            return 0;
        });
        return command;
    }

    private static Command ConfigCommand()
    {
        var pathOnly = new Option<bool>("--path") { Description = "Only print the config file path" };
        var init = new Option<bool>("--init") { Description = "Create the config file with defaults if it doesn't exist" };
        var command = new Command("config", "Show the config file location and effective settings") { pathOnly, init };

        command.SetAction(parse => Guard(() =>
        {
            if (parse.GetValue(pathOnly))
            {
                Console.WriteLine(ConfigStore.ConfigPath);
                return Task.FromResult(0);
            }

            if (parse.GetValue(init))
            {
                if (File.Exists(ConfigStore.ConfigPath))
                    Log.Warn($"{ConfigStore.ConfigPath} already exists; left untouched.");
                else
                {
                    ConfigStore.Save(new AppConfig());
                    Log.Info($"Created {ConfigStore.ConfigPath}");
                }
            }

            AppConfig c = ConfigStore.Current;
            var grid = new Grid().AddColumn().AddColumn();
            grid.AddRow("[grey]config file[/]", Markup.Escape(ConfigStore.ConfigPath) +
                (File.Exists(ConfigStore.ConfigPath) ? "" : " [yellow](not created, defaults in use)[/]"));
            grid.AddRow("[grey]download dir[/]", Markup.Escape(c.WallpaperRoot));
            grid.AddRow("[grey]default source[/]", Markup.Escape(c.DefaultSource));
            grid.AddRow("[grey]setter[/]", Markup.Escape(c.Setter ?? $"auto ({WallpaperSetters.Detect().Name})"));
            grid.AddRow("[grey]wallhaven key[/]", Masked(c.Wallhaven.ApiKey));
            grid.AddRow("[grey]wallhaven filters[/]", Markup.Escape(
                $"categories={c.Wallhaven.Categories} purity={c.Wallhaven.Purity} minResolution={c.Wallhaven.MinResolution ?? "-"} ratios={c.Wallhaven.Ratios ?? "-"}"));
            grid.AddRow("[grey]pexels key[/]", Masked(c.Pexels.ApiKey));
            AnsiConsole.Write(grid);
            return Task.FromResult(0);
        }).GetAwaiter().GetResult());

        return command;
    }

    private static async Task<int> SetAndReportAsync(string path, ParseResult parse, AppConfig config, CancellationToken ct)
    {
        SetResult result = await WallpaperSetters.SetAsync(path, parse.GetValue(SetterOption) ?? config.Setter, ct);
        if (result.Success)
            Log.Info(result.Message);
        else
            Log.Error(result.Message);
        return result.Success ? 0 : 1;
    }

    /// <summary>Turns expected failures into a one-line error and exit code 1 instead of a stack trace.</summary>
    private const string ProxyHint =
        "If the site is blocked or filtered on your network, wally honors HTTPS_PROXY / ALL_PROXY.";

    private static async Task<int> Guard(Func<Task<int>> action, CancellationToken ct = default)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (ex is SourceException or ConfigException)
        {
            Log.Error(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"Network error: {ex.Message}. {ProxyHint}");
        }
        catch (OperationCanceledException ex) when (ex.InnerException is TimeoutException || !ct.IsCancellationRequested)
        {
            // SocketsHttpHandler.ConnectTimeout / HttpClient.Timeout surface as cancellations.
            Log.Error($"Timed out talking to the server. {ProxyHint}");
        }
        catch (OperationCanceledException)
        {
            Log.Warn("Cancelled.");
            return 130;
        }
        catch (IOException ex)
        {
            Log.Error(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error(ex.Message);
        }

        return 1;
    }

    private static bool HasKey(AppConfig config, string source) => source switch
    {
        "pexels" => !string.IsNullOrWhiteSpace(config.Pexels.ApiKey),
        "wallhaven" => !string.IsNullOrWhiteSpace(config.Wallhaven.ApiKey),
        _ => false
    };

    private static string Masked(string? key) =>
        string.IsNullOrWhiteSpace(key) ? "[grey]not set[/]"
        : key.Length <= 4 ? "[green]set[/]"
        : $"[green]set[/] [grey]({Markup.Escape(key[..2])}…{Markup.Escape(key[^2..])})[/]";
}
