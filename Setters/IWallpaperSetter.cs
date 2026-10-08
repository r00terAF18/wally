namespace wally.Setters;

/// <summary>Sets the desktop wallpaper for one desktop environment / compositor.</summary>
public interface IWallpaperSetter
{
    /// <summary>Short name shown to the user, e.g. "GNOME" or "Hyprland (hyprpaper)".</summary>
    string Name { get; }

    /// <param name="imagePath">Absolute path to an existing image file.</param>
    Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default);
}

public sealed record SetResult(bool Success, string Message)
{
    public static SetResult Ok(string message) => new(true, message);
    public static SetResult Fail(string message) => new(false, message);
}
