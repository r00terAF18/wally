using Spectre.Console;
using PexelsDotNetSDK.Api;

namespace wally
{
    public class Pexels : BaseClass
    {
        public PexelsClient pClient;
        public Pexels()
        {
            pClient = new(readApiKey());
            folder_name = "Pexels";
        }

        private string readApiKey()
        {
            // reads content of api_key.txt
            string api_key = "";
            string path = "api_key.txt";
            if (File.Exists(path))
            {
                using (StreamReader sr = new(path))
                {
                    api_key = sr.ReadToEnd().Trim();
                }
            }
            else
            {
                Console.WriteLine("api_key.txt not found");
                Console.WriteLine("Please paste in your api key: ");
                api_key = Console.ReadLine().Trim();
                File.Create(path).Close();
                // write api_key to file
                using (StreamWriter sw = new(path))
                {
                    sw.Write(api_key);
                }
            }
            return api_key;
        }

        public async Task<string> search(string query)
        {
            var results = await pClient.SearchPhotosAsync(query);
            List<string> urls = new();
            foreach (var image in results.photos)
            {
                urls.Add(image.url.Split("/")[4]);
            }
            // https://www.pexels.com/photo/decorative-alien-character-near-disposable-glass-and-box-5558236/
            var images = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which Image do you want?")
                    .PageSize(5)
                    .MoreChoicesText("[grey](Move up and down to reveal more Images)[/]")
                    .AddChoices(urls.ToArray()));

            var img = await pClient.GetPhotoAsync(int.Parse(images.Split("-").Last()));
            return img.url;
        }

        public void download()
        {

        }

        // public void Download(bool random = false)
        // {
        //     random_download = random;
        //     nodes = SingleResolution("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        //     foreach (var item in nodes)
        //     {
        //         if (item.InnerText.Trim().Replace(" ", "") == resolution)
        //         {
        //             string link = baseURL + "/" + item.Attributes["href"].Value;
        //             AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
        //             string fileName = link.Split("/")[4];
        //             destFile = GetDestFile(fileName);
        //             base.Download(link);
        //         }
        //     }
        // }

        // public void MultiDownload(bool random = false)
        // {
        //     random_download = random;
        //     htmlDoc = GetDownloadPage();
        //     nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[3]/article/div[2]/a");

        //     foreach (var item in nodes)
        //     {
        //         foreach (var res in multi_resolution)
        //         {
        //             if (item.InnerText.Trim().Replace(" ", "") == res)
        //             {
        //                 string link = baseURL + "/" + item.Attributes["href"].Value;
        //                 AnsiConsole.MarkupLine($"[green][[+]] Checking Folder and file name...[/]");
        //                 string fileName = link.Split("/")[4];
        //                 destFile = GetDestFile(fileName);
        //                 base.Download(link);
        //             }
        //         }
        //     }
        // }
    }
}