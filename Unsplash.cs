using HtmlAgilityPack;
using Spectre.Console;

namespace wally
{
    public class Unsplash : BaseClass
    {
        private const string _baseUrl = "http://wallpaperswide.com";
        private const string _basUrlSearch = "http://wallpaperswide.com/search.html?q=";
        public Unsplash(string search_term)
        {
            string searchUrl = _basUrlSearch + search_term;
            folder_name = "Unsplash";
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

        public void Download(string custom_resolution = "1920x1080", bool random = false)
        {
            random_download = random;
            resolution = custom_resolution;
            htmlDoc = GetDownloadHtmlDocument();
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"wallpaper-resolutions\"]/a");

            foreach (var item in nodes)
            {
                if (item.InnerText == resolution)
                {
                    string link = _baseUrl + item.Attributes["href"].Value;
                    AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                    // Class Specific Code
                    string fileName = link.Split("/")[4];
                    destFile = GetDestFile(fileName);
                    // General Code
                    base.Download(link);
                }
            }
        }

        public void Download(List<string> custom_resolution, bool random = false)
        {
            random_download = random;
            htmlDoc = GetDownloadHtmlDocument();
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"wallpaper-resolutions\"]/a");

            foreach (var item in nodes)
            {
                foreach (var res in custom_resolution)
                {
                    if (item.InnerText == res)
                    {
                        string link = _baseUrl + item.Attributes["href"].Value;
                        AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                        string fileName = link.Split("/")[4];
                        destFile = GetDestFile(fileName);
                        // DownloadWallpaper(link);
                        base.Download(link);
                    }
                }
            }
        }
    }
}