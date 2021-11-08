using Spectre.Console;

namespace wally
{
    public class HdWallpaper : BaseClass
    {
        public HdWallpaper(string query)
        {
            base.baseURL = "https://www.hdwallpapers.in";
            base.searchBaseURL = "https://www.hdwallpapers.in/search.html?q=";
            base.searchURL = base.searchBaseURL + query;
            folder_name = "HdWallpapers";
            GetLinks("//*[@id=\"content\"]/div[3]/ul/li/div/a");
        }

        public void Download(bool random = false)
        {
            random_download = random;
            nodes = SingleResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

            foreach (var item in nodes)
            {
                if (item.InnerText.Trim().Replace(" ", "") == resolution)
                {
                    string link = baseURL + "/" + item.Attributes["href"].Value;
                    AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                    string fileName = link.Split("/")[4];
                    destFile = GetDestFile(fileName);
                    base.Download(link);
                }
            }
        }

        public void MultiDownload(bool random = false)
        {
            random_download = random;
            nodes = MultiResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

            foreach (var item in nodes)
            {
                foreach (var res in multi_resolution)
                {
                    if (item.InnerText.Trim().Replace(" ", "") == res)
                    {
                        string link = baseURL + "/" + item.Attributes["href"].Value;
                        AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                        string fileName = link.Split("/")[4];
                        destFile = GetDestFile(fileName);
                        base.Download(link);
                    }
                }
            }
        }
    }
}