using System.Text.Json;
using System.Text.Json.Serialization;

namespace wally.Core;

/// <summary>
/// ~/.config/wally/config.json (or %APPDATA%\wally\config.json on Windows).
/// Every value is optional; environment variables override the file.
/// </summary>
public sealed class AppConfig
{
    /// <summary>Root folder for downloads. Default: &lt;Pictures&gt;/Wallpaper. "~" is expanded.</summary>
    public string? DownloadDir { get; set; }

    /// <summary>Source used by "wally get" when --source is not given.</summary>
    public string DefaultSource { get; set; } = "wallhaven";

    /// <summary>Force a wallpaper backend (gnome, kde, xfce, mate, cinnamon, wlroots, feh, windows).</summary>
    public string? Setter { get; set; }

    public WallhavenConfig Wallhaven { get; set; } = new();
    public PexelsConfig Pexels { get; set; } = new();

    [JsonIgnore]
    public string WallpaperRoot => string.IsNullOrWhiteSpace(DownloadDir)
        ? Paths.DefaultWallpaperRoot
        : Paths.ExpandHome(DownloadDir);
}

public sealed class WallhavenConfig
{
    /// <summary>Only needed for NSFW/sketchy results or account settings. Env: WALLY_WALLHAVEN_KEY.</summary>
    public string? ApiKey { get; set; }

    /// <summary>general/anime/people as 3 bits, e.g. "111" = all, "100" = general only.</summary>
    public string Categories { get; set; } = "111";

    /// <summary>sfw/sketchy/nsfw as 3 bits. Anything but "100" needs an API key.</summary>
    public string Purity { get; set; } = "100";

    /// <summary>Default minimum resolution, e.g. "1920x1080". Overridden by --resolution.</summary>
    public string? MinResolution { get; set; }

    /// <summary>Default aspect ratios, e.g. "16x9,16x10". Overridden by --ratio.</summary>
    public string? Ratios { get; set; }
}

public sealed class PexelsConfig
{
    /// <summary>Free key from https://www.pexels.com/api/. Env: WALLY_PEXELS_KEY.</summary>
    public string? ApiKey { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppConfig))]
internal sealed partial class ConfigJsonContext : JsonSerializerContext;

public sealed class ConfigException(string message, Exception? inner = null) : Exception(message, inner);

public static class ConfigStore
{
    private static readonly Lazy<AppConfig> LazyCurrent = new(Load);

    /// <summary>The effective config (file + environment overrides), loaded once.</summary>
    public static AppConfig Current => LazyCurrent.Value;

    public static string ConfigDirectory
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wally");

            string? xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            string baseDir = string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config")
                : xdg;
            return Path.Combine(baseDir, "wally");
        }
    }

    public static string ConfigPath => Path.Combine(ConfigDirectory, "config.json");

    /// <summary>Only what's in the file (no environment overrides). Use this before saving.</summary>
    public static AppConfig LoadFile()
    {
        if (!File.Exists(ConfigPath))
            return new AppConfig();

        try
        {
            string json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize(json, ConfigJsonContext.Default.AppConfig) ?? new AppConfig();
        }
        catch (JsonException ex)
        {
            throw new ConfigException($"Invalid config file {ConfigPath}: {ex.Message}", ex);
        }
    }

    public static AppConfig Load()
    {
        AppConfig config = LoadFile();

        // Environment variables win over the file, so secrets don't have to live on disk.
        config.DownloadDir = Env("WALLY_DOWNLOAD_DIR") ?? config.DownloadDir;
        config.DefaultSource = Env("WALLY_DEFAULT_SOURCE") ?? config.DefaultSource;
        config.Setter = Env("WALLY_SETTER") ?? config.Setter;
        config.Wallhaven.ApiKey = Env("WALLY_WALLHAVEN_KEY") ?? config.Wallhaven.ApiKey;
        config.Pexels.ApiKey = Env("WALLY_PEXELS_KEY") ?? config.Pexels.ApiKey;
        return config;
    }

    public static void Save(AppConfig config)
    {
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, ConfigJsonContext.Default.AppConfig));

        // The file can hold API keys, so keep it private to the user.
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(ConfigPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private static string? Env(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
