using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using wally.Core;

namespace wally.Sources;

/// <summary>
/// https://www.pexels.com/api/documentation/ - needs a free API key
/// (WALLY_PEXELS_KEY or pexels.apiKey). Downloads the original-size photo.
/// </summary>
public sealed class PexelsSource(PexelsConfig config) : IWallpaperSource
{
    private const string Api = "https://api.pexels.com/v1/";
    private const int PerPage = 40;

    public string Name => "pexels";
    public string FolderName => "Pexels";

    public async Task<IReadOnlyList<WallpaperResult>> SearchAsync(SearchOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new SourceException(
                $"Pexels needs an API key: set WALLY_PEXELS_KEY or pexels.apiKey in {ConfigStore.ConfigPath} (free: https://www.pexels.com/api/).");

        string query = options.Query?.Trim() ?? "";
        // A random page gives variety for --random; without a query use the curated feed.
        int page = options.Random ? Random.Shared.Next(1, 6) : 1;
        string url = query.Length == 0
            ? $"{Api}curated?per_page={PerPage}&page={page}"
            : $"{Api}search?query={Uri.EscapeDataString(query)}&orientation=landscape&size=large&per_page={PerPage}&page={page}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Authorization", config.ApiKey);

        using var response = await Http.Client.SendAsync(request, ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new SourceException("Pexels rejected the API key.");
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new SourceException("Pexels rate limit reached. Try again later.");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var result = await JsonSerializer.DeserializeAsync(stream, PexelsJsonContext.Default.PexelsResponse, ct);

        IEnumerable<PexelsPhoto> photos = result?.Photos ?? [];
        if (options.MinResolution is { } min)
            photos = photos.Where(p => p.Width >= min.Width && p.Height >= min.Height);
        if (!string.IsNullOrWhiteSpace(options.Ratios))
        {
            double[] wanted = ParseRatios(options.Ratios);
            photos = photos.Where(p => p.Height > 0 && wanted.Any(r => Math.Abs((double)p.Width / p.Height - r) / r < 0.03));
        }

        return photos
            .Where(p => !string.IsNullOrEmpty(p.Src.Original))
            .Select(p => new WallpaperResult(
                p.Id.ToString(),
                p.Src.Original,
                $"pexels-{p.Id}{Path.GetExtension(new Uri(p.Src.Original).AbsolutePath)}",
                p.Width,
                p.Height,
                p.Url,
                p.Photographer))
            .ToList();
    }

    private static double[] ParseRatios(string ratios) =>
        ratios.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(r => Resolution.TryParse(r, out var res) ? (double)res.Value.Width / res.Value.Height : 0)
            .Where(r => r > 0)
            .ToArray();
}

internal sealed class PexelsResponse
{
    [JsonPropertyName("photos")] public List<PexelsPhoto> Photos { get; set; } = [];
}

internal sealed class PexelsPhoto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("photographer")] public string? Photographer { get; set; }
    [JsonPropertyName("src")] public PexelsSrc Src { get; set; } = new();
}

internal sealed class PexelsSrc
{
    [JsonPropertyName("original")] public string Original { get; set; } = "";
}

[JsonSerializable(typeof(PexelsResponse))]
internal sealed partial class PexelsJsonContext : JsonSerializerContext;
