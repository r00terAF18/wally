using wally.Core;

namespace wally.Setters;

/// <summary>KDE Plasma 5/6.</summary>
public sealed class KdeSetter : IWallpaperSetter
{
    private static readonly string[] QdbusCandidates = ["qdbus6", "qdbus", "qdbus-qt6", "qdbus-qt5"];

    public string Name => "KDE Plasma";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        // Plasma 5.26+ ships a dedicated tool; it handles every desktop/monitor for us.
        if (ProcessRunner.IsOnPath("plasma-apply-wallpaperimage"))
        {
            var apply = await ProcessRunner.RunAsync("plasma-apply-wallpaperimage", ct, imagePath);
            if (apply.Success)
                return SetResult.Ok("KDE Plasma: plasma-apply-wallpaperimage");
        }

        string? qdbus = QdbusCandidates.FirstOrDefault(ProcessRunner.IsOnPath);
        if (qdbus is null)
            return SetResult.Fail("KDE Plasma: neither plasma-apply-wallpaperimage nor qdbus was found");

        // $$ raw string: {{...}} interpolates, single braces are literal JavaScript.
        string script = $$"""
                          const allDesktops = desktops();
                          for (let i = 0; i < allDesktops.length; i++) {
                              const d = allDesktops[i];
                              d.wallpaperPlugin = "org.kde.image";
                              d.currentConfigGroup = ["Wallpaper", "org.kde.image", "General"];
                              d.writeConfig("Image", {{JsString(GSettings.FileUri(imagePath))}});
                          }
                          """;

        var result = await ProcessRunner.RunAsync(qdbus, ct, "org.kde.plasmashell", "/PlasmaShell",
            "org.kde.PlasmaShell.evaluateScript", script);
        return result.Success
            ? SetResult.Ok($"KDE Plasma: {qdbus} evaluateScript")
            : SetResult.Fail($"{qdbus}: {result.Error}");
    }

    /// <summary>Quotes a value as a JavaScript string literal.</summary>
    private static string JsString(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
}
