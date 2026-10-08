using Spectre.Console;

namespace wally.Downloaders;

public class HdWallpaper : BaseClass
{
    public HdWallpaper(string query)
    {
        BaseUrl = "https://www.hdwallpapers.in";
        SearchBaseUrl = "https://www.hdwallpapers.in/search.html?q=";
        SearchUrl = SearchBaseUrl + query;
        FolderName = "HdWallpapers";
        LinksXPath = "//*[@id=\"content\"]/div[3]/ul/li/div/a";
    }

    public async Task DownloadAsync(bool random = false)
    {
        RandomDownload = random;
        Nodes = await SingleResolutionAsync("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        foreach (var item in Nodes)
            if (item.InnerText.Trim().Replace(" ", "") == Resolution)
            {
                string link = BaseUrl + "/" + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine("[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                await DownloadFileAsync(link);
            }
    }

    public async Task MultiDownloadAsync(bool random = false)
    {
        RandomDownload = random;
        Nodes = await MultiResolutionAsync("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        foreach (var item in Nodes)
        foreach (var res in MultiResolutionList)
            if (item.InnerText.Trim().Replace(" ", "") == res)
            {
                string link = BaseUrl + "/" + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine("[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                await DownloadFileAsync(link);
            }
    }
}