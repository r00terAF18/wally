using System.Net;

namespace wally.Core;

/// <summary>
/// One HttpClient for the whole app. Reusing a single instance avoids socket exhaustion, and
/// SocketsHttpHandler's pooled-connection lifetime keeps DNS changes from going stale.
/// The default proxy settings are honored, so HTTPS_PROXY/ALL_PROXY work out of the box.
/// </summary>
public static class Http
{
    public const string UserAgent = "wally (+https://github.com/r00terAF18/wally)";

    public static HttpClient Client { get; } = Create();

    private static HttpClient Create()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(20)
        };

        var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        return client;
    }

    /// <summary>
    /// Streams <paramref name="url"/> to <paramref name="destination"/>. The data goes to a
    /// ".part" file first, so an interrupted download never leaves a broken image behind.
    /// </summary>
    public static async Task DownloadFileAsync(string url, string destination, CancellationToken ct = default)
    {
        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        string partial = destination + ".part";
        try
        {
            await using (var source = await response.Content.ReadAsStreamAsync(ct))
            await using (var target = File.Create(partial))
            {
                await source.CopyToAsync(target, ct);
            }

            File.Move(partial, destination, overwrite: true);
        }
        catch
        {
            try { File.Delete(partial); } catch { /* best effort */ }
            throw;
        }
    }
}
