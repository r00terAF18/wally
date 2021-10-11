using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using System.Xml;

namespace wally
{
    public class BaseClass
    {
        public HtmlWeb web;
        public HtmlDocument htmlDoc;
        public HtmlNodeCollection nodes;
        public static bool OSisWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public string path { get; set; }
        public string destFile { get; set; }
        public List<string> wallpaper_list = new List<string> { };
        public const string default_resolution = "1920x1080";
        public string resolution;
        public string DE;
        public bool IsLaptop;

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
            Console.WriteLine("Got Sys Info");
        }

        public List<string> Links()
        {
            return wallpaper_list;
        }

        public string GetPath(string folder_name)
        {
            if (OSisWindows)
            {
                path = $"C:\\Users\\{Environment.UserName}\\Pictures\\Wallpapers\\{folder_name}";
            }
            else
            {
                path = $"/home/{Environment.UserName}/Pictures/Wallpaper/{folder_name}";
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string GetDestFile(string fileName, string folder_name)
        {
            path = GetPath(folder_name);
            if (OSisWindows)
                destFile = path + $"\\{fileName}";
            else
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

    }
}