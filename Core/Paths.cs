namespace wally.Core;

public static class Paths
{
    /// <summary>
    /// &lt;Pictures&gt;/Wallpaper. Uses the real Pictures folder (XDG_PICTURES_DIR on Linux,
    /// the possibly redirected known folder on Windows) instead of guessing /home/&lt;user&gt;.
    /// </summary>
    public static string DefaultWallpaperRoot
    {
        get
        {
            string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (string.IsNullOrEmpty(pictures))
                pictures = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pictures");
            return Path.Combine(pictures, "Wallpaper");
        }
    }

    public static string ExpandHome(string path)
    {
        if (path == "~" || path.StartsWith("~/", StringComparison.Ordinal) || path.StartsWith("~\\", StringComparison.Ordinal))
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path.Length > 2 ? path[2..] : "");
        return Path.GetFullPath(path);
    }

    /// <summary>Strips characters that are invalid in file names on any OS.</summary>
    public static string SafeFileName(string name)
    {
        char[] invalid = [.. Path.GetInvalidFileNameChars(), '<', '>', ':', '"', '/', '\\', '|', '?', '*'];
        string cleaned = new(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "wallpaper" : cleaned;
    }
}
