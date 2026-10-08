using wally.Core;

namespace wally.Setters;

/// <summary>
/// Xfce keeps one backdrop property per screen/monitor/workspace, e.g.
/// /backdrop/screen0/monitorDP-1/workspace0/last-image. Set every one that exists.
/// </summary>
public sealed class XfceSetter : IWallpaperSetter
{
    private const string Channel = "xfce4-desktop";
    private const string FallbackProperty = "/backdrop/screen0/monitor0/workspace0/last-image";

    public string Name => "Xfce";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        var list = await ProcessRunner.RunAsync("xfconf-query", ct, "--channel", Channel, "--list");
        if (!list.Success)
            return SetResult.Fail($"xfconf-query --list: {list.Error}");

        string[] properties = list.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Xfce 4.12+ uses last-image; older versions used image-path.
        List<string> targets = properties.Where(p => p.EndsWith("/last-image", StringComparison.Ordinal)).ToList();
        if (targets.Count == 0)
            targets = properties.Where(p => p.EndsWith("/image-path", StringComparison.Ordinal)).ToList();

        if (targets.Count == 0)
        {
            var create = await ProcessRunner.RunAsync("xfconf-query", ct, "--channel", Channel,
                "--property", FallbackProperty, "--create", "--type", "string", "--set", imagePath);
            return create.Success
                ? SetResult.Ok($"Xfce: created {FallbackProperty}")
                : SetResult.Fail($"xfconf-query: {create.Error}");
        }

        foreach (string property in targets)
        {
            var set = await ProcessRunner.RunAsync("xfconf-query", ct, "--channel", Channel,
                "--property", property, "--set", imagePath);
            if (!set.Success)
                return SetResult.Fail($"xfconf-query {property}: {set.Error}");
        }

        return SetResult.Ok($"Xfce: set {targets.Count} backdrop(s)");
    }
}
