using wally.Interfaces;

namespace wally.Downloaders;

public class KonachanSfw : BaseClass, IDownloader
{
    private const string BaseRandom = "https://konachan.net/post/random";

    public KonachanSfw(string query = "")
    {
        BaseUrl = "https://konachan.net/post";
        SearchBaseUrl = "https://konachan.net/post?tags=";
        SearchUrl = SearchBaseUrl + query;
        FolderName = "KonachanSFW";
        // GetLinks("//*[@class=\"thumb\"]");
    }

    public async Task DownloadAsync(bool random = false)
    {
        if (random)
        {
            await GetLinksAsync("//*[@class=\"thumb\"]", BaseRandom);
            foreach (string link in WallpaperLinks) Console.WriteLine(link);
        }
        // GetLinks("//*[@class=\"image\" and @id=\"image\"]", BaseRandom);
        // Nodes = SingleResolution("//*[@class=\"image\" and @id=\"image\"]");
        //
        // foreach (var item in Nodes)
        // {
        //     string link = BaseUrl + item.Attributes["href"].Value;
        //     AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
        //     string fileName = link.Split("/")[4];
        //     DestFile = GetDestFile(fileName);
        // }
    }

    public Task MultiDownloadAsync(bool random = false)
    {
        throw new NotImplementedException();
    }
}