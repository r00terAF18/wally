using Spectre.Console;

namespace wally
{
    public class WallpapersWide : BaseClass
    {
        public WallpapersWide(string query)
        {
            base.baseURL = "http://wallpaperswide.com";
            base.searchBaseURL = "http://wallpaperswide.com/search.html?q=";
            base.searchURL = base.searchBaseURL + query;
            folder_name = "WallpapersWide";
            GetLinks("//*[@id=\"hudtitle\"]/a");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="random"></param>
        public void Download(bool random = false)
        {
            random_download = random;
            nodes = SingleResolution("//*[@id=\"wallpaper-resolutions\"]/a");

            foreach (var item in nodes)
            {
                if (item.InnerText.Trim().Replace(" ", "") == resolution)
                {
                    string link = baseURL + item.Attributes["href"].Value;
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
            nodes = MultiResolution("//*[@id=\"wallpaper-resolutions\"]/a");

            foreach (var item in nodes)
            {
                foreach (var res in multi_resolution)
                {
                    if (item.InnerText.Trim().Replace(" ", "") == res)
                    {
                        string link = baseURL + item.Attributes["href"].Value;
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