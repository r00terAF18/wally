namespace wally.Interfaces;

public interface IDownloader
{
    public void Download(bool random = false);
    public void MultiDownload(bool random = false);
}