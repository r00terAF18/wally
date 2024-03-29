// using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using Spectre.Console;
using System.Net;
using System.Runtime.InteropServices;

namespace wally
{
    public class BaseClass
    {
        public HtmlWeb web { get; set; }
        public HtmlDocument htmlDoc { get; set; }
        public HtmlNodeCollection nodes { get; set; }
        public string path { get; set; }
        public string folder_name { get; set; }
        public string destFile { get; set; }
        public string file_name { get; set; }
        public List<string> wallpaper_list = new List<string> { };
        public string resolution { get; set; }
        public List<string> multi_resolution { get; set; }
        public string DE { get; set; }
        public bool IsLaptop { get; set; }
        public bool IsWindows { get; set; }
        public bool random_download { get; set; }
        public string searchTerm { get; set; }
        public string baseURL { get; set; }
        public string searchBaseURL { get; set; }
        public string searchURL { get; set; }

        public BaseClass()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public override string ToString()
        {
            return "Base Downloader Class";
        }

        protected void GetLinks(string xpath)
        {
            AnsiConsole.MarkupLine($"[green][[+]] Loading Content...[/]");
            web = new HtmlWeb();
            htmlDoc = web.Load(searchURL);
            AnsiConsole.MarkupLine($"[green][[+]] Scraping Data...[/]");
            try
            {
                nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
                if (nodes.Count != 0)
                {
                    AnsiConsole.MarkupLine($"[green][[+]] Storing link temporarely...[/]");
                    foreach (var item in nodes)
                    {
                        string link = $"{baseURL}{item.Attributes["href"].Value}";
                        wallpaper_list.Add(link);
                    }
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("No results found");
            }
        }

        protected HtmlNodeCollection SingleResolution(string xpath)
        {
            htmlDoc = GetDownloadPage();
            nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
            List<string> temp = new List<string> { };
            foreach (HtmlNode item in nodes)
            {
                temp.Add(item.InnerText.Trim().Replace(" ", ""));
            }
            resolution = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which Resolution do you want?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more Resolutions)[/]")
                    .AddChoices(temp.ToArray()));
            return nodes;
        }

        protected HtmlNodeCollection MultiResolution(string xpath)
        {
            htmlDoc = GetDownloadPage();
            nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
            List<string> temp = new List<string> { };
            foreach (HtmlNode item in nodes)
            {
                temp.Add(item.InnerText.Trim().Replace(" ", ""));
            }

            List<string> selected_resoltions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                .Title("How many Resolutions do you want?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more Resoltions)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a Resolution, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(temp.ToArray())
            );

            multi_resolution = selected_resoltions;

            return nodes;
        }

        private void SetPath()
        {
            if (!IsWindows)
                path = $"/home/{Environment.UserName}/Pictures/Wallpaper/{folder_name}";
            else
                path = @$"C:\Users\{Environment.UserName}\Pictures\Wallpaper\{folder_name}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string GetDestFile(string fileName)
        {
            SetPath();
            file_name = fileName;
            if (!IsWindows)
                destFile = path + $"/{fileName}";
            else
                destFile = path + @$"\{fileName}";
            return destFile;
        }

        public HtmlDocument GetDownloadPage()
        {
            int index;
            if (random_download == false)
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
            return htmlDoc;
        }

        protected virtual void Download(string URL)
        {
            AnsiConsole.MarkupLine($"[yellow]>>> Downloading {file_name} <<<[/]");
            using (WebClient w = new())
            {
                Uri u = new Uri(URL);
                w.DownloadFile(u, destFile);
            }
            AnsiConsole.MarkupLine($"[green]>>> Downloaded {file_name} <<<[/]");
        }

    }
}