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
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more Websites)[/]")
        .AddChoices(new[] {
            "WallpapersWide.com",
            "HdWallpapers.in",
            "[strikethrough]Pexels.com[/]"
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
    case "WallpapersWide.com":
        WallpapersWide w = new(query);
        if (res == "S")
            w.Download(random: drandom);
        else
            w.MultiDownload();
        break;
    case "HdWallpapers.in":
        HdWallpaper h = new(query);
        if (res == "S")
            h.Download(random: drandom);
        else
            h.MultiDownload();
        break;
    default:
        break;
}