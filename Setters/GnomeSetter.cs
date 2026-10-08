namespace wally.Setters;

/// <summary>GNOME and GNOME-based desktops (Ubuntu, Pop!_OS, Budgie, Zorin...).</summary>
public sealed class GnomeSetter : IWallpaperSetter
{
    private const string Schema = "org.gnome.desktop.background";

    public string Name => "GNOME";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        string uri = GSettings.FileUri(imagePath);

        var light = await GSettings.SetAsync(Schema, "picture-uri", uri, ct);
        if (!light.Success)
            return SetResult.Fail($"gsettings picture-uri: {light.Error}");

        // GNOME 42+ shows picture-uri-dark while the dark style is on. Older GNOME has no such key.
        var dark = await GSettings.SetAsync(Schema, "picture-uri-dark", uri, ct);
        if (!dark.Success)
        {
            if (!await GSettings.HasKeyAsync(Schema, "picture-uri-dark", ct))
                return SetResult.Ok("GNOME: set picture-uri (this GNOME has no dark-mode wallpaper key)");
            return SetResult.Fail($"gsettings picture-uri-dark: {dark.Error}");
        }

        return SetResult.Ok("GNOME: set picture-uri and picture-uri-dark");
    }
}
