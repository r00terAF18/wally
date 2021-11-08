using System.Runtime.InteropServices;
// using Newtonsoft.Json.Linq;
using System.Diagnostics;
using HtmlAgilityPack;
using Spectre.Console;
using System.Net;

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
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? true : false;
            GetSysInfo();
        }

        public override string ToString()
        {
            return "Base Downloader Class";
        }

        private void GetSysInfo()
        {
            if (!IsWindows)
            {
                // Process proc = new Process
                // {
                //     StartInfo = new ProcessStartInfo
                //     {
                //         FileName = "hostnamectl",
                //         Arguments = "--json=pretty",
                //         UseShellExecute = false,
                //         RedirectStandardOutput = true,
                //         CreateNoWindow = true
                //     }
                // };
                // proc.Start();
                // string cmd_output = proc.StandardOutput.ReadToEnd();
                // JObject jobj = JObject.Parse(cmd_output);
                // if (jobj["Chassis"].Value<string>() == "laptop")
                // {
                //     IsLaptop = true;
                // }
                // else
                // {
                //     IsLaptop = false;
                // }
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "screenfetch",
                        Arguments = "-Nn",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string temp = proc.StandardOutput.ReadLine();
                    if (temp.Split(":")[0].Trim() == "DE")
                    {
                        DE = temp.Split(":")[1].Trim().ToLower();
                    }
                    if (temp.Split(":")[0].Trim() == "Resolution")
                    {
                        resolution = temp.Split(":")[1].Trim();
                    }
                }
            }
            else
            {
                DE = "Windows WM";
            }

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

        // public void SetWallpaper(string file_path)
        // {
        //     string arg = "";
        //     string cmd = "";
        //     switch (DE)
        //     {
        //         case "xfce4":
        //             cmd = "xfconf-query";
        //             if (IsLaptop)
        //             {
        //                 arg = $"-c {DE}-desktop -p /backdrop/screen0/monitoreDP-1/workspace0/last-image -s {file_path}";
        //             }
        //             else
        //             {
        //                 arg = $"-c {DE}-desktop -p /backdrop/screen0/monitor0/workspace0/last-image -s {file_path}";
        //             }
        //             break;
        //         case "Windows WM":
        //             cmd = "reg add";
        //             arg = $"\"HKEY_CURRENT_USER\\Control Panel\\Desktop\" /v Wallpaper /t REG_SZ /d {file_path} /f";
        //             break;
        //         default:
        //             throw new Exception("No valid DE was found");
        //     }

        //     // xfconf-query -c xfce4-desktop -p /backdrop/screen0/monitoreDP-1/workspace0/last-image -s /home/amirs/Pictures/Wallpaper/007.jpg
        //     Process proc = new Process
        //     {
        //         StartInfo = new ProcessStartInfo
        //         {
        //             FileName = cmd,
        //             Arguments = arg,
        //             UseShellExecute = false,
        //             RedirectStandardOutput = true,
        //             CreateNoWindow = true
        //         }
        //     };
        //     proc.Start();

        //     // apply the changes immediately
        //     if (IsWindows)
        //     {
        //         proc = new Process
        //         {
        //             StartInfo = new ProcessStartInfo
        //             {
        //                 FileName = "RUNDLL32.EXE",
        //                 Arguments = "user32.dll,UpdatePerUserSystemParameters",
        //                 UseShellExecute = false,
        //                 RedirectStandardOutput = true,
        //                 CreateNoWindow = true
        //             }
        //         };
        //         proc.Start();
        //     }
        // }

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