using System.Xml;
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


// BaseClass b = new();

WallpapersWide w = new("Car");
w.Download(random: true, resolution: "2560x1440");
w.SetWallpaper(w.destFile);