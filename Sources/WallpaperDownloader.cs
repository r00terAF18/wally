using wally.Core;

namespace wally.Sources;

public static class WallpaperDownloader
{
    /// <summary>
    /// Downloads <paramref name="wallpaper"/> into &lt;download dir&gt;/&lt;source folder&gt;/ and returns
    /// the file path. A file that's already there is reused instead of downloaded again.
    /// </summary>
    public static async Task<(string Path, bool AlreadyExisted)> DownloadAsync(
        WallpaperResult wallpaper, IWallpaperSource source, CancellationToken ct = default)
    {
        string directory = Path.Combine(ConfigStore.Current.WallpaperRoot, source.FolderName);
        Directory.CreateDirectory(directory);

        string destination = Path.Combine(directory, Paths.SafeFileName(wallpaper.FileName));
        if (File.Exists(destination) && new FileInfo(destination).Length > 0)
            return (destination, true);

        await Http.DownloadFileAsync(wallpaper.DownloadUrl, destination, ct);
        return (destination, false);
    }
}
