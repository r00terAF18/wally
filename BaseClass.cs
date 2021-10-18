using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using Spectre.Console;
using System.Net;
using Flurl;
using Flurl.Http;

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
        public string DE { get; set; }
        public bool IsLaptop { get; set; }
        public bool random_download { get; set; }

        public BaseClass()
        {
            GetSysInfo();
        }

        public override string ToString()
        {
            return "Base Downloader Class";
        }

        private void GetSysInfo()
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "hostnamectl",
                    Arguments = "--json=pretty",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string cmd_output = proc.StandardOutput.ReadToEnd();
            JObject jobj = JObject.Parse(cmd_output);
            if (jobj["Chassis"].Value<string>() == "laptop")
            {
                IsLaptop = true;
            }
            else
            {
                IsLaptop = false;
            }
            proc = new Process
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

        public List<string> Links()
        {
            return wallpaper_list;
        }

        private string SetPath()
        {
            path = $"/home/{Environment.UserName}/Pictures/Wallpaper/{folder_name}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string GetDestFile(string fileName)
        {
            SetPath();
            file_name = fileName;
            destFile = path + $"/{fileName}";
            return destFile;
        }

        public void SetWallpaper(string file_path)
        {
            string arg;
            if (IsLaptop)
            {
                arg = $"-c {DE}-desktop -p /backdrop/screen0/monitoreDP-1/workspace0/last-image -s {file_path}";
            }
            else
            {
                arg = $"-c {DE}-desktop -p /backdrop/screen0/monitor0/workspace0/last-image -s {file_path}";
            }
            // xfconf-query -c xfce4-desktop -p /backdrop/screen0/monitoreDP-1/workspace0/last-image -s /home/amirs/Pictures/Wallpaper/007.jpg
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xfconf-query",
                    Arguments = arg,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }

        public HtmlDocument GetDownloadHtmlDocument()
        {
            web = new HtmlWeb();
            htmlDoc = web.Load(GetLinkForDownload());
            return htmlDoc;
        }

        private string GetLinkForDownload()
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
            return wallpaper_list[index];
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