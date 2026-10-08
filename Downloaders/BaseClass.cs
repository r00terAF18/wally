using HtmlAgilityPack;
using Spectre.Console;
using wally.Core;
using wally.Setters;

namespace wally.Downloaders;

public class BaseClass
{
    public HtmlDocument HtmlDoc { get; set; } = new();
    public HtmlNodeCollection? Nodes { get; set; }
    public string Path { get; set; } = "";
    public string FolderName { get; set; } = "";
    public string DestFile { get; set; } = "";
    public string FileName { get; set; } = "";
    /// <summary>Detail-page links collected from the search page.</summary>
    protected List<string> WallpaperLinks { get; } = new();
    /// <summary>XPath for the result links on the search page; loaded lazily on first use.</summary>
    protected string? LinksXPath { get; set; }
    protected string Resolution { get; set; } = "";
    protected List<string> MultiResolutionList { get; set; } = [];
    public bool RandomDownload { get; set; }
    public string SearchTerm { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string SearchBaseUrl { get; set; } = "";
    public string SearchUrl { get; set; } = "";

    public override string ToString()
    {
        return "Base Downloader Class";
    }

    protected static async Task<HtmlDocument> LoadHtmlAsync(string url)
    {
        string html = await Http.Client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    protected async Task GetLinksAsync(string xpath, string url = "")
    {
        AnsiConsole.MarkupLine("[green][[+]] Loading Content...[/]");
        HtmlDoc = await LoadHtmlAsync(string.IsNullOrEmpty(url) ? SearchUrl : url);
        AnsiConsole.MarkupLine("[green][[+]] Scraping Data...[/]");
        try
        {
            Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
            if (Nodes is { Count: > 0 })
            {
                AnsiConsole.MarkupLine("[green][[+]] Storing link temporarely...[/]");
                foreach (var item in Nodes)
                {
                    string link = $"{BaseUrl}{item.Attributes["href"].Value}";
                    WallpaperLinks.Add(link);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("No results found");
        }
    }

    protected async Task<HtmlNodeCollection> SingleResolutionAsync(string xpath)
    {
        HtmlDoc = await GetDownloadPageAsync();
        Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath)
                ?? throw new InvalidOperationException("No resolutions found on the wallpaper page.");
        List<string> temp = new();
        foreach (HtmlNode item in Nodes) temp.Add(item.InnerText.Trim().Replace(" ", ""));
        Resolution = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which Resolution do you want?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more Resolutions)[/]")
                .AddChoices(temp.ToArray()));
        return Nodes;
    }

    protected async Task<HtmlNodeCollection> MultiResolutionAsync(string xpath)
    {
        HtmlDoc = await GetDownloadPageAsync();
        Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath)
                ?? throw new InvalidOperationException("No resolutions found on the wallpaper page.");
        List<string> temp = new();
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
        // <download dir from config, default Pictures/Wallpaper>/<source folder>
        Path = System.IO.Path.Combine(ConfigStore.Current.WallpaperRoot, FolderName);
        Directory.CreateDirectory(Path);
    }

    /// <summary>Sets the last downloaded file as wallpaper using the detected desktop backend.</summary>
    public Task<SetResult> SetAsWallpaperAsync() => WallpaperSetters.SetAsync(DestFile, ConfigStore.Current.Setter);

    public string GetDestFile(string fileName)
    {
        SetPath();
        FileName = fileName;
        DestFile = System.IO.Path.Combine(Path, fileName);
        return DestFile;
    }

    public async Task<HtmlDocument> GetDownloadPageAsync()
    {
        if (WallpaperLinks.Count == 0 && LinksXPath is not null)
            await GetLinksAsync(LinksXPath);

        if (WallpaperLinks.Count == 0)
            throw new InvalidOperationException("No wallpapers found for this search.");

        int index;
        if (RandomDownload == false)
        {
            index = 0;
        }
        else
        {
            // Next's upper bound is exclusive, so Count (not Count - 1) keeps the last item in play.
            index = Random.Shared.Next(WallpaperLinks.Count);
            AnsiConsole.MarkupLine("[green][[+]] Selecting random Wallpaper...[/]");
        }

        HtmlDoc = await LoadHtmlAsync(WallpaperLinks[index]);
        return HtmlDoc;
    }

    protected virtual async Task DownloadFileAsync(string url)
    {
        AnsiConsole.MarkupLine($"[yellow]>>> Downloading {FileName} <<<[/]");
        await Http.DownloadFileAsync(url, DestFile);

        AnsiConsole.MarkupLine($"[green]>>> Downloaded {FileName} <<<[/]");
    }
}