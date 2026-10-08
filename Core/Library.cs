namespace wally.Core;

/// <summary>The local wallpaper collection (download dir or any folder).</summary>
public static class Library
{
    public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif", ".jxl", ".avif", ".tif", ".tiff"
    };

    public static bool IsImage(string path) => ImageExtensions.Contains(Path.GetExtension(path));

    /// <summary>All images under <paramref name="root"/>, recursively, skipping hidden files and partial downloads.</summary>
    public static List<string> EnumerateImages(string root)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
        };

        return Directory.EnumerateFiles(root, "*", options).Where(IsImage).ToList();
    }
}
