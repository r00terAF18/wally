namespace wally.Setters;

public sealed class MateSetter : IWallpaperSetter
{
    public string Name => "MATE";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        // MATE stores a plain path here, not a URI.
        var result = await GSettings.SetAsync("org.mate.background", "picture-filename", imagePath, ct);
        return result.Success
            ? SetResult.Ok("MATE: set org.mate.background picture-filename")
            : SetResult.Fail($"gsettings: {result.Error}");
    }
}
