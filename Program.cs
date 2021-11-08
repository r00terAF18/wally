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

var website = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which Website to download from?")
        .PageSize(5)
        .MoreChoicesText("[grey](Move up and down to reveal more Websites)[/]")
        .MoreChoicesText("[grey]Pexels requires an API Key, get one at https://www.pexels.com/join/[/]")
        .AddChoices(new[] {
            "WallpapersWide",
            "HdWallpapers",
            "Pexels",
            "[strikethrough]unsplash.com[/]"
        }));

string query = AnsiConsole.Ask<string>("What to search for?");

var res = AnsiConsole.Prompt(
    new TextPrompt<string>("Download single resolution(Single Resolution/Multiple Resolutions)?")
        .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
        .DefaultValue("S")
        .AddChoice("M"));

var random_download = AnsiConsole.Prompt(
    new TextPrompt<string>("[grey][[Optional]][/] [green]Random or latest(R/L)[/]?")
        .DefaultValue("L")
        .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
        .AllowEmpty());
bool drandom;

if (random_download == "R")
    drandom = true;
else
    drandom = false;

switch (website)
{
    case "WallpapersWide":
        WallpapersWide w = new(query);
        if (res == "S")
            w.Download(random: drandom);
        else
            w.MultiDownload();
        break;
    case "HdWallpapers":
        HdWallpaper h = new(query);
        if (res == "S")
            h.Download(random: drandom);
        else
            h.MultiDownload();
        break;
    case "Pexels":
        Pexels p = new();
        await p.search(query);
        // if (res == "S")
        //     p.Download(random: drandom);
        // else
        //     p.MultiDownload();
        break;
    default:
        break;
}