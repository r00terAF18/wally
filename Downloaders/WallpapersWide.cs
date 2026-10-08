using Spectre.Console;
using wally.Interfaces;

namespace wally.Downloaders;

public class WallpapersWide : BaseClass, IDownloader
{
    public WallpapersWide(string query)
    {
        BaseUrl = "http://wallpaperswide.com";
        SearchBaseUrl = "http://wallpaperswide.com/search.html?q=";
        SearchUrl = SearchBaseUrl + query;
        FolderName = "WallpapersWide";
        LinksXPath = "//*[@id=\"hudtitle\"]/a";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resolution"></param>
    /// <param name="random"></param>
    public async Task DownloadAsync(bool random = false)
    {
        RandomDownload = random;
        Nodes = await SingleResolutionAsync("//*[@id=\"wallpaper-resolutions\"]/a");

        foreach (var item in Nodes)
            if (item.InnerText.Trim().Replace(" ", "") == Resolution)
            {
                string link = BaseUrl + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine("[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                await DownloadFileAsync(link);
            }
    }

    public async Task MultiDownloadAsync(bool random = false)
    {
        RandomDownload = random;
        Nodes = await MultiResolutionAsync("//*[@id=\"wallpaper-resolutions\"]/a");

        foreach (var item in Nodes)
        foreach (var res in MultiResolutionList)
            if (item.InnerText.Trim().Replace(" ", "") == res)
            {
                string link = BaseUrl + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine("[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                await DownloadFileAsync(link);
            }
    }
}