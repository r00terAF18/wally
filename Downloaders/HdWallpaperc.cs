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
        GetLinks("//*[@id=\"content\"]/div[3]/ul/li/div/a");
    }

    public void Download(bool random = false)
    {
        RandomDownload = random;
        Nodes = SingleResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        foreach (var item in Nodes)
            if (item.InnerText.Trim().Replace(" ", "") == Resolution)
            {
                string link = BaseUrl + "/" + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                base.Download(link);
            }
    }

    public void MultiDownload(bool random = false)
    {
        RandomDownload = random;
        Nodes = MultiResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        foreach (var item in Nodes)
        foreach (var res in MultiResolutionList)
            if (item.InnerText.Trim().Replace(" ", "") == res)
            {
                string link = BaseUrl + "/" + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                base.Download(link);
            }
    }
}