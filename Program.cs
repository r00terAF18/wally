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

HdWallpaper hd = new("Synthwave");
hd.Download(random: true, custom_resolution: new List<string> { "1280x720", "1920x1080"});