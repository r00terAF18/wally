using wally.Core;

namespace wally.Setters;

/// <summary>Plain X11 window managers (i3, bspwm, openbox...). feh also writes ~/.fehbg for restoring at login.</summary>
public sealed class FehSetter : IWallpaperSetter
{
    public string Name => "X11 (feh)";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        if (!ProcessRunner.IsOnPath("feh"))
            return SetResult.Fail("feh is not installed (needed to set wallpapers on plain X11 window managers)");

        var result = await ProcessRunner.RunAsync("feh", ct, "--bg-fill", imagePath);
        return result.Success ? SetResult.Ok("feh --bg-fill") : SetResult.Fail($"feh: {result.Error}");
    }
}
