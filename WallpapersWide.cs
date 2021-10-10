using HtmlAgilityPack;
using Spectre.Console;
using System.Net;

namespace wally
{
    /// <summary>
    /// Helper Class to scrape and download wallpapers form WallpapersWide.com
    /// </summary>
    /// <param name="search_term">Search Query, for example: Synthwave</param>
    public class WallpapersWide : BaseClass
    {
        private const string _baseUrl = "http://wallpaperswide.com";
        private const string _basUrlSearch = "http://wallpaperswide.com/search.html?q=";

        public WallpapersWide(string search_term)
        {
            string searchUrl = _basUrlSearch + search_term;
            GetLinks(searchUrl);
        }

        private void GetLinks(string searchUrl)
        {
            web = new HtmlWeb();
            htmlDoc = web.Load(searchUrl);
            AnsiConsole.MarkupLine($"[green][[+]] Loading Content...[/]");
            try
            {
                AnsiConsole.MarkupLine($"[green][[+]] Scraping Data...[/]");
                nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"hudtitle\"]/a");
                if (nodes.Count != 0)
                {
                    AnsiConsole.MarkupLine($"[green][[+]] Storing link temporarely...[/]");
                    foreach (var item in nodes)
                    {
                        string link = $"{_baseUrl}{item.Attributes["href"].Value}";
                        wallpaper_list.Add(link);
                    }
                }
                else
                    Console.WriteLine("No Wallpapers could be found, maybe try searching for something else");
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException();
            }
        }

        /// <summary>
        /// By default, it will try to find a 1920x1080 image, onless resolution is given
        /// By default, it will download the latest, i.e. the first image, onless specified else
        /// </summary>
        /// <parama name="resolution">Resolution of image to try and download, default is "1920x1080"</param>
        /// <param name="random">Download random image from found images, or the latest, i.e. first one, default is no</param>
        public void Download(string resolution = resolution, bool random = false)
        {
            int index;
            if (random == false)
            {
                index = 0;
            }
            else
            {
                Random r = new();
                index = r.Next(0, wallpaper_list.Count - 1);
                AnsiConsole.MarkupLine($"[green][[+]] Selecting random Wallpaper...[/]");
            }

            web = new HtmlWeb();
            htmlDoc = web.Load(wallpaper_list[index]);
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"wallpaper-resolutions\"]/a");

            foreach (var item in nodes)
            {
                if (item.InnerText == resolution)
                {
                    string link = _baseUrl + item.Attributes["href"].Value;
                    _downlaod(link);
                }
            }
        }

        private void _downlaod(string URL)
        {
            AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
            string fileName = URL.Split("/")[4];
            string destFile = GetDestFile(fileName, "WallpapersWide");

            AnsiConsole.MarkupLine($"[yellow]>>> Downloading {fileName} <<<[/]");
            using (WebClient w = new())
            {
                Uri u = new Uri(URL);
                w.DownloadFile(u, destFile);
            }
            AnsiConsole.MarkupLine($"[green]>>> Downloaded {fileName} <<<[/]");
        }
    }
}