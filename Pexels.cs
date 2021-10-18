using HtmlAgilityPack;
using Spectre.Console;

namespace wally
{
    public class Pexels : BaseClass
    {
        private const string _baseUrl = "https://www.pexels.com";
        private const string _basUrlSearch = "https://www.pexels.com/search/";
        public Pexels(string search_term)
        {
            string searchUrl = _basUrlSearch + search_term;
            folder_name = "Pexels";
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
                // /html/body/div[1]/div[5]/div[6]/div[1]/div[1]/div[1]/article/div/a
                nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class=\"photos\"]/div");
                if (nodes.Count != 0)
                {
                    AnsiConsole.MarkupLine($"[green][[+]] Storing link temporarely...[/]");
                    foreach (var item in nodes)
                    {
                        Console.WriteLine(item.SelectSingleNode("//a[@class=\"js-photo-link photo-item__link\"]"));
                        // string link = $"{_baseUrl}{item.Attributes["href"].Value}";
                        // wallpaper_list.Add(link);
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
            htmlDoc = GetDownloadPage();
            nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[3]/article/div[2]/a");


            foreach (var item in nodes)
            {
                if (item.InnerText.Trim().Replace(" ", "") == resolution)
                {
                    string link = _baseUrl + "/" + item.Attributes["href"].Value;
                    AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
                    string fileName = link.Split("/")[4];
                    destFile = GetDestFile(fileName);
                    base.Download(link);
                }
            }
        }

        public void Download(List<string> custom_resolution, bool random = false)
        {
            random_download = random;
            htmlDoc = GetDownloadPage();
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
                        base.Download(link);
                    }
                }
            }
        }
    }
}