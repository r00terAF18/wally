using Spectre.Console;
using wally.Downloaders;

const string logo = """

                                   _ _       
                    __      ____ _| | |_   _ 
                    \ \ /\ / / _` | | | | | |
                     \ V  V / (_| | | | |_| |
                      \_/\_/ \__,_|_|_|\__, |
                                       |___/ 


                    """;
Console.Clear();
AnsiConsole.WriteLine(logo);

string website = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which Website to download from?")
        .PageSize(5)
        .MoreChoicesText("[grey](Move up and down to reveal more Websites)[/]")
        .MoreChoicesText("[grey]Pexels requires an API Key, get one at https://www.pexels.com/join/[/]")
        .AddChoices("Konachan (SFW)", "Konachan (NSFW)", "WallpapersWide", "HdWallpapers", "Pexels",
            "[strikethrough]unsplash.com[/]"));

string random_download = AnsiConsole.Prompt(
    new TextPrompt<string>("[grey][[Optional]][/] [green]Random or latest (R/L)[/]?")
        .AddChoice("R")
        .AddChoice("L")
        .DefaultValue("L")
        .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
        .AllowEmpty());
bool drandom;

if (random_download == "R")
    drandom = true;
else
    drandom = false;

string query = "";
if (!drandom)
    query = AnsiConsole.Ask<string>("What to search for?");

string res = AnsiConsole.Prompt(
    new TextPrompt<string>("Download single resolution(Single Resolution/Multiple Resolutions) (S/M)?")
        .InvalidChoiceMessage("[red]That's not a valid Choice[/]")
        .DefaultValue("S")
        .AddChoice("S")
        .AddChoice("M"));


switch (website)
{
    case "Konachan (SFW)":
        KonachanSfw k = new(query);
        if (res == "S")
            k.Download(drandom);
        else
            k.MultiDownload(drandom);
        break;
    case "Konachan (NSFW)":
        KonachanNsfw knswf = new(query);
        if (res == "S")
            knswf.Download(drandom);
        else
            knswf.MultiDownload(drandom);
        break;
    case "WallpapersWide":
        WallpapersWide w = new(query);
        if (res == "S")
            w.Download(drandom);
        else
            w.MultiDownload();
        break;
    case "HdWallpapers":
        HdWallpaper h = new(query);
        if (res == "S")
            h.Download(drandom);
        else
            h.MultiDownload();
        break;
    case "Pexels":
        Pexels p = new();
        await p.Search(query);
        if (res == "S")
            p.Download(drandom);
        // else
        //     p.MultiDownload();
        break;
}