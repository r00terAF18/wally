using System.Net;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using Spectre.Console;

namespace wally.Downloaders;

public class BaseClass
{
    public HtmlWeb Web { get; set; }
    public HtmlDocument HtmlDoc { get; set; }
    public HtmlNodeCollection Nodes { get; set; }
    public string Path { get; set; }
    public string FolderName { get; set; }
    public string DestFile { get; set; }
    public string FileName { get; set; }
    private List<string> _wallpaperList = new() { };
    protected string Resolution { get; set; }
    protected List<string> MultiResolutionList { get; set; }
    public string De { get; set; }
    public bool IsLaptop { get; set; }
    public bool IsWindows { get; set; }
    public bool RandomDownload { get; set; }
    public string SearchTerm { get; set; }
    public string BaseUrl { get; set; }
    public string SearchBaseUrl { get; set; }
    public string SearchUrl { get; set; }

    public BaseClass()
    {
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public override string ToString()
    {
        return "Base Downloader Class";
    }

    protected void GetLinks(string xpath, string url = "")
    {
        AnsiConsole.MarkupLine($"[green][[+]] Loading Content...[/]");
        Web = new HtmlWeb();
        if (string.IsNullOrEmpty(url))
            HtmlDoc = Web.Load(SearchUrl);
        else
            HtmlDoc = Web.Load(url);
        AnsiConsole.MarkupLine($"[green][[+]] Scraping Data...[/]");
        try
        {
            Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
            if (Nodes.Count != 0)
            {
                AnsiConsole.MarkupLine($"[green][[+]] Storing link temporarely...[/]");
                foreach (var item in Nodes)
                {
                    string link = $"{BaseUrl}{item.Attributes["href"].Value}";
                    _wallpaperList.Add(link);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("No results found");
        }
    }

    protected HtmlNodeCollection SingleResolution(string xpath)
    {
        HtmlDoc = GetDownloadPage();
        Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
        List<string> temp = new() { };
        foreach (HtmlNode item in Nodes) temp.Add(item.InnerText.Trim().Replace(" ", ""));
        Resolution = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which Resolution do you want?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more Resolutions)[/]")
                .AddChoices(temp.ToArray()));
        return Nodes;
    }

    protected HtmlNodeCollection MultiResolution(string xpath)
    {
        HtmlDoc = GetDownloadPage();
        Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
        List<string> temp = new() { };
        foreach (HtmlNode item in Nodes) temp.Add(item.InnerText.Trim().Replace(" ", ""));

        List<string> selectedResoltions = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("How many Resolutions do you want?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more Resoltions)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a Resolution, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(temp.ToArray())
        );

        MultiResolutionList = selectedResoltions;

        return Nodes;
    }

    private void SetPath()
    {
        if (!IsWindows)
            Path = $"/home/{Environment.UserName}/Pictures/Wallpaper/{FolderName}";
        else
            Path = @$"C:\Users\{Environment.UserName}\Pictures\Wallpaper\{FolderName}";
        if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
    }

    public string GetDestFile(string fileName)
    {
        SetPath();
        FileName = fileName;
        if (!IsWindows)
            DestFile = Path + $"/{fileName}";
        else
            DestFile = Path + @$"\{fileName}";
        return DestFile;
    }

    public HtmlDocument GetDownloadPage()
    {
        int index;
        if (RandomDownload == false)
        {
            index = 0;
        }
        else
        {
            Random r = new();
            index = r.Next(0, _wallpaperList.Count - 1);
            AnsiConsole.MarkupLine($"[green][[+]] Selecting random Wallpaper...[/]");
        }

        Web = new HtmlWeb();
        HtmlDoc = Web.Load(_wallpaperList[index]);
        return HtmlDoc;
    }

    protected virtual void Download(string url)
    {
        AnsiConsole.MarkupLine($"[yellow]>>> Downloading {FileName} <<<[/]");
        using (WebClient w = new())
        {
            Uri u = new(url);
            w.DownloadFile(u, DestFile);
        }

        AnsiConsole.MarkupLine($"[green]>>> Downloaded {FileName} <<<[/]");
    }
}