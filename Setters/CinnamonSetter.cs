namespace wally.Setters;

public sealed class CinnamonSetter : IWallpaperSetter
{
    public string Name => "Cinnamon";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        var result = await GSettings.SetAsync("org.cinnamon.desktop.background", "picture-uri",
            GSettings.FileUri(imagePath), ct);
        return result.Success
            ? SetResult.Ok("Cinnamon: set org.cinnamon.desktop.background picture-uri")
            : SetResult.Fail($"gsettings: {result.Error}");
    }
}
