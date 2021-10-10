using System.Runtime.InteropServices;
using HtmlAgilityPack;


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
        public const string resolution = "1920x1080";

        public override string ToString()
        {
            return "Base Downloader Class";
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

    }
}