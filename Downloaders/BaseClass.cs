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
    private List<string> _wallpaperList = new();
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

    protected HtmlNodeCollection MultiResolution(string xpath)
    {
        HtmlDoc = GetDownloadPage();
        Nodes = HtmlDoc.DocumentNode.SelectNodes(xpath);
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
        if (!IsWindows)
            Path = $"/home/{Environment.UserName}/Pictures/Wallpaper/{FolderName}";
        else
            Path = @$"C:\Users\{Environment.UserName}\Pictures\Wallpaper\{FolderName}";
        if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
    }

    private void ExecuteCommand(string command)
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("bash", $"-c \"{command}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                AnsiConsole.MarkupLine($"[red][[x]] Command failed with error: {error}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][[x]] Failed to execute command: {ex.Message}[/]");
        }
    }


    private void SetWallpaper()
    {
        if (IsWindows)
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

            // Setting the wallpaper on Windows
            const int SPI_SETDESKWALLPAPER  = 0x0014;
            const int SPIF_UPDATEINIFILE    = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            try
            {
                if (!File.Exists(DestFile))
                {
                    AnsiConsole.MarkupLine("[red][[x]] Wallpaper file not found![/]");
                    return;
                }

                // Setting the wallpaper
                bool success = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, DestFile,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (success)
                    AnsiConsole.MarkupLine($"[green][[+]] Wallpaper set successfully: {DestFile}[/]");
                else
                    AnsiConsole.MarkupLine("[red][[x]] Failed to set wallpaper.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red][[x]] Error: {ex.Message}[/]");
            }
        }
        else
        {
            try
            {
                if (!File.Exists(DestFile))
                {
                    AnsiConsole.MarkupLine("[red][[x]] Wallpaper file not found![/]");
                    return;
                }

                // Detect desktop environment on Linux
                string desktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? string.Empty;

                if (string.IsNullOrWhiteSpace(desktopEnvironment))
                {
                    AnsiConsole.MarkupLine("[red][[x]] Unable to detect the desktop environment.[/]");
                    return;
                }

                string command = string.Empty;

                switch (desktopEnvironment.ToLower())
                {
                    case "gnome":
                    case "ubuntu":
                        command = $"gsettings set org.gnome.desktop.background picture-uri 'file://{DestFile}'";
                        break;

                    case "kde":
                    case "plasma":
                        command = """
                                  qdbus org.kde.plasmashell /PlasmaShell org.kde.PlasmaShell.evaluateScript '
                                  var allDesktops = desktops();
                                  for (let i = 0; i < allDesktops.length; i++) {{
                                      d = allDesktops[i];
                                      d.wallpaperPlugin = "org.kde.image";
                                      d.currentConfigGroup = [\"Wallpaper\", \"org.kde.image\", \"General\"];
                                      d.writeConfig(\"Image\", \"file://{DestFile}\");
                                  }}'
                                  """;
                        break;

                    case "xfce":
                        command =
                            $"xfconf-query --channel xfce4-desktop --property /backdrop/screen0/monitor0/image-path --set {DestFile}";
                        break;

                    case "mate":
                        command = $"gsettings set org.mate.background picture-filename '{DestFile}'";
                        break;

                    default:
                        AnsiConsole.MarkupLine(
                            $"[yellow][[!]] Unsupported or undetected desktop environment: {desktopEnvironment}[/]");
                        return;
                }

                ExecuteCommand(command);
                AnsiConsole.MarkupLine(
                    $"[green][[+]] Wallpaper set successfully for desktop environment: {desktopEnvironment}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red][[x]] Error setting wallpaper: {ex.Message}[/]");
            }
        }
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
            AnsiConsole.MarkupLine("[green][[+]] Selecting random Wallpaper...[/]");
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