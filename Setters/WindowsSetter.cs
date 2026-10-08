using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace wally.Setters;

[SupportedOSPlatform("windows")]
public sealed partial class WindowsSetter : IWallpaperSetter
{
    private const uint SpiSetDeskWallpaper = 0x0014;
    private const uint SpifUpdateIniFile = 0x01;
    private const uint SpifSendWinIniChange = 0x02;

    public string Name => "Windows";

    public Task<SetResult> SetAsync(string imagePath, CancellationToken ct = default)
    {
        bool ok = SystemParametersInfo(SpiSetDeskWallpaper, 0, imagePath, SpifUpdateIniFile | SpifSendWinIniChange);
        return Task.FromResult(ok
            ? SetResult.Ok("Windows: SystemParametersInfo")
            : SetResult.Fail($"SystemParametersInfo failed (error {Marshal.GetLastPInvokeError()})"));
    }

    // LibraryImport generates the marshalling code at compile time, so this stays Native-AOT/trim safe.
    [LibraryImport("user32.dll", EntryPoint = "SystemParametersInfoW", StringMarshalling = StringMarshalling.Utf16,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
}
