using Spectre.Console;

namespace wally.Downloaders;

public class Unsplash : BaseClass
{
    public Unsplash(string query)
    {
        BaseUrl = "https://unsplash.com/";
        SearchBaseUrl = "https://unsplash.com/s/photos/";
        SearchUrl = SearchBaseUrl + query;
        FolderName = "Unsplash";
        LinksXPath = "//[@class=\"photo-item__img\"]";
    }

    public async Task DownloadAsync(bool random = false)
    {
        RandomDownload = random;
        Nodes = await SingleResolutionAsync("//*[@id=\"content\"]/div[3]/article/div[2]/a");

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
        Nodes = await MultiResolutionAsync("//*[@id=\"content\"]/div[3]/article/div[2]/a");

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