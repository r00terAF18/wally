using HtmlAgilityPack;
using Spectre.Console;

namespace wally
{
    public class HdWallpaper : BaseClass
    {
        private const string _baseUrl = "https://www.hdwallpapers.in";
        private const string _basUrlSearch = "https://www.hdwallpapers.in/search.html?q=";
        public HdWallpaper(string search_term)
        {
            string searchUrl = _basUrlSearch + search_term;
            folder_name = "HdWallpapers";
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
                nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[3]/ul/li/div/a");
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
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[3]/article/div[2]/a");


            foreach (var item in nodes)
            {
                if (item.InnerText.Trim().Replace(" ", "") == resolution)
                {
                    string link = _baseUrl + "/" + item.Attributes["href"].Value;
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
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[3]/article/div[2]/a");

            foreach (var item in nodes)
            {
                foreach (var res in custom_resolution)
                {
                    if (item.InnerText.Trim().Replace(" ", "") == res)
                    {
                        string link = _baseUrl + "/" + item.Attributes["href"].Value;
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