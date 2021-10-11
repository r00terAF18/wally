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

// WallpapersWide w = new("Synthwave");
// w.Download();
// w.SetWallpaper(w.destFile);

Unsplash u = new("Synthwave");
u.Download();
u.SetWallpaper(u.destFile);