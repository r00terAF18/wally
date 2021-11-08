using Spectre.Console;

namespace wally
{
    public class Unsplash : BaseClass
    {
        public Unsplash(string query)
        {
            base.baseURL = "https://unsplash.com/";
            base.searchBaseURL = "https://unsplash.com/s/photos/";
            base.searchURL = base.searchBaseURL + query;
            folder_name = "Unsplash";
            GetLinks("//[@class=\"photo-item__img\"]");
        }

        public void Download(bool random = false)
        {
            random_download = random;
            nodes = SingleResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

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
            nodes = MultiResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

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