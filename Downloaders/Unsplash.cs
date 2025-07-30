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
        GetLinks("//[@class=\"photo-item__img\"]");
    }

    public void Download(bool random = false)
    {
        RandomDownload = random;
        Nodes = SingleResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        foreach (var item in Nodes)
            if (item.InnerText.Trim().Replace(" ", "") == Resolution)
            {
                string link = BaseUrl + item.Attributes["href"].Value;
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
                string link = BaseUrl + item.Attributes["href"].Value;
                AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                string fileName = link.Split("/")[4];
                DestFile = GetDestFile(fileName);
                base.Download(link);
            }
    }
}