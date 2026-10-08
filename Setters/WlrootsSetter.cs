using System.Diagnostics;
using wally.Core;

namespace wally.Setters;

/// <summary>
/// Hyprland, Sway and other wlroots-style compositors have no wallpaper setting of their own,
/// so use whichever wallpaper daemon is around: swww/awww (if its daemon runs), hyprpaper (via
/// hyprctl), swaymsg (on Sway), and finally a detached swaybg.
/// </summary>
public sealed class WlrootsSetter : IWallpaperSetter
{
    public string Name => "Wayland compositor (swww / hyprpaper / swaybg)";

    public async Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        // swww was renamed to awww; support both. Only use it if its daemon is up.
        foreach (string swww in new[] { "awww", "swww" })
        {
            if (!ProcessRunner.IsOnPath(swww) || !(await ProcessRunner.RunAsync(swww, ct, "query")).Success)
                continue;
            var img = await ProcessRunner.RunAsync(swww, ct, "img", imagePath);
            return img.Success ? SetResult.Ok($"{swww} img") : SetResult.Fail($"{swww}: {img.Error}");
        }

        if (Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE") is not null
            && ProcessRunner.IsOnPath("hyprctl") && ProcessRunner.IsRunning("hyprpaper"))
        {
            // preload + wallpaper ",path" (empty monitor = all monitors), then free old images.
            await ProcessRunner.RunAsync("hyprctl", ct, "hyprpaper", "preload", imagePath);
            var wallpaper = await ProcessRunner.RunAsync("hyprctl", ct, "hyprpaper", "wallpaper", "," + imagePath);
            if (wallpaper.Success && !wallpaper.StdOut.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessRunner.RunAsync("hyprctl", ct, "hyprpaper", "unload", "unused");
                return SetResult.Ok("Hyprland: hyprpaper via hyprctl");
            }
        }

        if (Environment.GetEnvironmentVariable("SWAYSOCK") is not null && ProcessRunner.IsOnPath("swaymsg"))
        {
            var sway = await ProcessRunner.RunAsync("swaymsg", ct, "output", "*", "bg", imagePath, "fill");
            if (sway.Success)
                return SetResult.Ok("Sway: swaymsg output * bg");
        }

        if (ProcessRunner.IsOnPath("swaybg"))
        {
            int[] previous = GetPids("swaybg");
            var start = await ProcessRunner.StartDetachedAsync("swaybg", ct, "--mode", "fill", "--image", imagePath);
            if (!start.Success)
                return SetResult.Fail($"swaybg: {start.Error}");

            // Give the new instance a moment to draw before removing the old one (avoids a flash).
            await Task.Delay(500, ct);
            foreach (int pid in previous)
            {
                try { using var p = Process.GetProcessById(pid); p.Kill(); } catch { /* already gone */ }
            }

            return SetResult.Ok("swaybg (detached)");
        }

        return SetResult.Fail("No wallpaper tool found. Install swww (or awww), hyprpaper or swaybg.");
    }

    private static int[] GetPids(string name)
    {
        Process[] processes = Process.GetProcessesByName(name);
        int[] pids = processes.Select(p => p.Id).ToArray();
        foreach (Process p in processes) p.Dispose();
        return pids;
    }
}
