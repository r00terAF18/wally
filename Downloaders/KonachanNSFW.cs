using wally.Interfaces;

namespace wally.Downloaders;

public class KonachanNsfw : BaseClass, IDownloader
{
    private const string BaseRandom = "https://konachan.net/post/random";

    public KonachanNsfw(string query = "")
    {
        
    }

    public void Download(bool random = false)
    {
        throw new NotImplementedException();
    }

    public void MultiDownload(bool random = false)
    {
        throw new NotImplementedException();
    }
}