namespace wally.Setters;

/// <summary>Picks the right <see cref="IWallpaperSetter"/> for the running desktop.</summary>
public static class WallpaperSetters
{
    /// <summary>Names accepted by WALLY_SETTER / the "setter" config value to force a backend.</summary>
    public static readonly string[] Names = ["gnome", "kde", "xfce", "mate", "cinnamon", "wlroots", "feh", "windows"];

    /// <param name="forced">Optional backend name that overrides auto-detection.</param>
    public static IWallpaperSetter Detect(string? forced = null)
    {
        forced ??= Environment.GetEnvironmentVariable("WALLY_SETTER");
        if (!string.IsNullOrWhiteSpace(forced))
            return FromName(forced) ?? new UnsupportedSetter($"unknown setter '{forced}' (use one of: {string.Join(", ", Names)})");

        if (OperatingSystem.IsWindows())
            return new WindowsSetter();

        // XDG_CURRENT_DESKTOP is a colon-separated list, e.g. "ubuntu:GNOME" or "Budgie:GNOME".
        HashSet<string> desktops = new(StringComparer.OrdinalIgnoreCase);
        foreach (string variable in new[] { "XDG_CURRENT_DESKTOP", "XDG_SESSION_DESKTOP", "DESKTOP_SESSION" })
        foreach (string part in (Environment.GetEnvironmentVariable(variable) ?? "").Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            desktops.Add(part);

        bool Has(params string[] names) => names.Any(desktops.Contains);

        if (Has("KDE", "plasma", "plasmawayland")) return new KdeSetter();
        if (Has("X-Cinnamon", "cinnamon")) return new CinnamonSetter();
        if (Has("MATE")) return new MateSetter();
        if (Has("XFCE", "xfce4")) return new XfceSetter();
        if (Has("GNOME", "ubuntu", "pop", "Budgie", "Unity", "GNOME-Classic", "GNOME-Flashback", "Zorin"))
            return new GnomeSetter();
        if (Has("Hyprland", "sway", "river", "wayfire", "niri", "labwc")
            || Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE") is not null
            || Environment.GetEnvironmentVariable("SWAYSOCK") is not null
            || Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
            return new WlrootsSetter();
        if (Environment.GetEnvironmentVariable("DISPLAY") is not null)
            return new FehSetter();

        return new UnsupportedSetter(desktops.Count > 0
            ? $"unsupported desktop '{string.Join(":", desktops)}'"
            : "could not detect a desktop session (no XDG_CURRENT_DESKTOP, WAYLAND_DISPLAY or DISPLAY)");
    }

    public static IWallpaperSetter? FromName(string name) => name.Trim().ToLowerInvariant() switch
    {
        "gnome" => new GnomeSetter(),
        "kde" or "plasma" => new KdeSetter(),
        "xfce" => new XfceSetter(),
        "mate" => new MateSetter(),
        "cinnamon" => new CinnamonSetter(),
        "wlroots" or "hyprland" or "sway" or "wayland" => new WlrootsSetter(),
        "feh" or "x11" => new FehSetter(),
        "windows" when OperatingSystem.IsWindows() => new WindowsSetter(),
        _ => null
    };

    /// <summary>Validates the file and sets it with the detected backend.</summary>
    public static async Task<SetResult> SetAsync(string imagePath, string? forced = null, CancellationToken ct = default)
    {
        string fullPath = Path.GetFullPath(imagePath);
        if (!File.Exists(fullPath))
            return SetResult.Fail($"file not found: {fullPath}");

        return await Detect(forced).SetAsync(fullPath, ct);
    }
}

internal sealed class UnsupportedSetter(string reason) : IWallpaperSetter
{
    public string Name => "unsupported";

    public Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default) =>
        Task.FromResult(SetResult.Fail($"Can't set the wallpaper: {reason}. Set WALLY_SETTER to force a backend."));
}
