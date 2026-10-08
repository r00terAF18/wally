namespace wally.Interfaces;

public interface IDownloader
{
    public Task DownloadAsync(bool random = false);
    public Task MultiDownloadAsync(bool random = false);
}