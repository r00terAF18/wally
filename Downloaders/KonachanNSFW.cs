using wally.Interfaces;

namespace wally.Downloaders;

public class KonachanNsfw : BaseClass, IDownloader
{
    private const string BaseRandom = "https://konachan.net/post/random";

    public KonachanNsfw(string query = "")
    {
    }

    public Task DownloadAsync(bool random = false)
    {
        throw new NotImplementedException();
    }

    public Task MultiDownloadAsync(bool random = false)
    {
        throw new NotImplementedException();
    }
}