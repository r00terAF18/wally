using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using wally.Core;

namespace wally.Sources;

/// <summary>
/// https://wallhaven.cc/help/api - public search API. No key needed for SFW results;
/// a key (WALLY_WALLHAVEN_KEY or wallhaven.apiKey) unlocks sketchy/NSFW purity.
/// </summary>
public sealed class WallhavenSource(WallhavenConfig config) : IWallpaperSource
{
    private const string SearchEndpoint = "https://wallhaven.cc/api/v1/search";

    public string Name => "wallhaven";
    public string FolderName => "Wallhaven";

    public async Task<IReadOnlyList<WallpaperResult>> SearchAsync(SearchOptions options, CancellationToken ct = default)
    {
        string query = options.Query?.Trim() ?? "";
        string sorting = options.Random ? "random" : query.Length == 0 ? "toplist" : "relevance";

        var url = new StringBuilder(SearchEndpoint)
            .Append("?q=").Append(Uri.EscapeDataString(query))
            .Append("&categories=").Append(Uri.EscapeDataString(config.Categories))
            .Append("&purity=").Append(Uri.EscapeDataString(config.Purity))
            .Append("&sorting=").Append(sorting);

        string? atLeast = options.MinResolution?.ToString() ?? config.MinResolution;
        if (!string.IsNullOrWhiteSpace(atLeast))
            url.Append("&atleast=").Append(Uri.EscapeDataString(atLeast));

        string? ratios = options.Ratios ?? config.Ratios;
        if (!string.IsNullOrWhiteSpace(ratios))
            url.Append("&ratios=").Append(Uri.EscapeDataString(ratios));

        using var request = new HttpRequestMessage(HttpMethod.Get, url.ToString());
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            request.Headers.Add("X-API-Key", config.ApiKey);

        using var response = await Http.Client.SendAsync(request, ct);
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new SourceException("Wallhaven rejected the request (401). Check your API key, or use purity 100 (SFW) without one.");
            case HttpStatusCode.TooManyRequests:
                throw new SourceException("Wallhaven rate limit hit (45 requests/minute). Try again in a minute.");
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await JsonSerializer.DeserializeAsync(stream, WallhavenJsonContext.Default.WallhavenSearchResponse, ct);

        return (result?.Data ?? [])
            .Where(w => !string.IsNullOrEmpty(w.Path))
            .Select(w => new WallpaperResult(
                w.Id,
                w.Path,
                System.IO.Path.GetFileName(new Uri(w.Path).AbsolutePath),
                w.DimensionX,
                w.DimensionY,
                w.Url))
            .ToList();
    }
}

internal sealed class WallhavenSearchResponse
{
    [JsonPropertyName("data")] public List<WallhavenWallpaper> Data { get; set; } = [];
}

internal sealed class WallhavenWallpaper
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("path")] public string Path { get; set; } = "";
    [JsonPropertyName("dimension_x")] public int DimensionX { get; set; }
    [JsonPropertyName("dimension_y")] public int DimensionY { get; set; }
}

[JsonSerializable(typeof(WallhavenSearchResponse))]
internal sealed partial class WallhavenJsonContext : JsonSerializerContext;
