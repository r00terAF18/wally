using Spectre.Console;
using wally;

const string logo = @"
               _ _       
__      ____ _| | |_   _ 
\ \ /\ / / _` | | | | | |
 \ V  V / (_| | | | |_| |
  \_/\_/ \__,_|_|_|\__, |
                   |___/ 

";
Console.Clear();
AnsiConsole.WriteLine(logo);

WallpapersWide w = new("Car");
w.Download(custom_resolution: new List<string>{"1920x1080", "2560x1440"});
// w.SetWallpaper(w.destFile);

// Unsplash u = new("Anime");
// u.Download();
// u.SetWallpaper(u.destFile);