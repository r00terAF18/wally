using wally.Core;

namespace wally.Setters;

internal static class GSettings
{
    public static Task<ProcessResult> SetAsync(string schema, string key, string value, CancellationToken ct) =>
        ProcessRunner.RunAsync("gsettings", ct, "set", schema, key, value);

    public static async Task<bool> HasKeyAsync(string schema, string key, CancellationToken ct) =>
        (await ProcessRunner.RunAsync("gsettings", ct, "writable", schema, key)).Success;

    /// <summary>A correctly percent-encoded file:// URI (spaces, #, %, non-ASCII...).</summary>
    public static string FileUri(string path) => new Uri(Path.GetFullPath(path)).AbsoluteUri;
}
