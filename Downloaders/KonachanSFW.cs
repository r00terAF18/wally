using HtmlAgilityPack;
using Spectre.Console;
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

    public virtual void GetLinks(string xpath, string url = "")
    {
        AnsiConsole.MarkupLine("[green][[+]] Loading Content...[/]");
        Web = new HtmlWeb();
        if (string.IsNullOrEmpty(url))
            HtmlDoc = Web.Load(SearchUrl);
        else
            HtmlDoc = Web.Load(url);
        AnsiConsole.MarkupLine("[green][[+]] Scraping Data...[/]");
        try
        {
            Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
            if (Nodes.Count != 0)
            {
                AnsiConsole.MarkupLine("[green][[+]] Storing link temporarely...[/]");
                foreach (var item in Nodes)
                {
                    string link = $"{BaseUrl}{item.Attributes["href"].Value}";
                    Console.WriteLine(link);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("No results found");
        }
    }

    public void Download(bool random = false)
    {
        if (random) GetLinks("//*[@class=\"thumb\"]", BaseRandom);
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

    public void MultiDownload(bool random = false)
    {
        throw new NotImplementedException();
    }
}