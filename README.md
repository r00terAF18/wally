# wally - the wallpaper manager you've been waiting for (yeah right 🤣)

A terminal-first wallpaper downloader and setter for every Linux desktop (and Windows).
Search Wallhaven or Pexels from the command line, download in one go, set it as your
wallpaper, or pick a random one from your collection. Prefer menus? Run `wally` on its own
for the interactive TUI.

## Usage

```sh
wally                                   # interactive menu (same as: wally tui)
wally get mountains                     # best Wallhaven match -> ~/Pictures/Wallpaper/Wallhaven/
wally get mountains --set               # ...and set it as wallpaper
wally get "city night" -r --resolution 2560x1440 --ratio 16x9,21x9
wally get -s pexels forest              # Pexels (needs a free API key)
wally get --random --set                # something random from Wallhaven
wally set ~/Pictures/some-image.jpg     # set any image
wally random                            # random wallpaper from your downloads
wally random --dir ~/Pictures/Favs      # ...or from any folder (recursive)
wally sources                           # list sources and whether they need a key
wally config                            # where the config is and what's in effect
wally --version
```

`get`, `set` and `random` never prompt. Status messages go to stderr and the resulting
file path goes to stdout, so wally is easy to script:

```sh
wally set "$(wally get -r aurora)"
```

Any command accepts `--setter <gnome|kde|xfce|mate|cinnamon|wlroots|feh|windows>` to skip
desktop auto-detection.

## Sources

| Source         | Type    | `wally get` | API key                       |
|----------------|---------|-------------|-------------------------------|
| Wallhaven      | API     | yes (default) | only for sketchy/NSFW purity |
| Pexels         | API     | yes         | free: https://www.pexels.com/api/ |
| WallpapersWide | scraper | menu only   | -                             |
| HdWallpapers   | scraper | menu only   | -                             |
| Konachan       | scraper | menu only (work in progress) | -            |

## Supported desktops

| Desktop                          | How wally sets the wallpaper                                     |
|----------------------------------|------------------------------------------------------------------|
| GNOME, Ubuntu, Pop!_OS, Budgie   | `gsettings` `picture-uri` **and** `picture-uri-dark` (dark mode) |
| KDE Plasma 5/6                   | `plasma-apply-wallpaperimage`, else `qdbus` script               |
| Xfce                             | `xfconf-query` on every monitor/workspace backdrop               |
| MATE / Cinnamon                  | `gsettings`                                                      |
| Hyprland, Sway, other wlroots    | `swww`/`awww` (if running), else `hyprpaper` via `hyprctl`, else `swaymsg`, else `swaybg` |
| Plain X11 WMs (i3, bspwm, ...)   | `feh --bg-fill`                                                  |
| Windows                          | `SystemParametersInfo`                                           |

Detection uses `XDG_CURRENT_DESKTOP` (and friends); override with `--setter`,
`WALLY_SETTER` or `"setter"` in the config.

## Configuration

Optional. `wally config --init` creates `~/.config/wally/config.json`
(`%APPDATA%\wally\config.json` on Windows):

```json
{
  "downloadDir": null,            // default: <Pictures>/Wallpaper
  "defaultSource": "wallhaven",
  "setter": null,                 // force a backend, e.g. "kde"
  "wallhaven": {
    "apiKey": null,
    "categories": "111",          // general/anime/people
    "purity": "100",              // sfw/sketchy/nsfw (non-SFW needs apiKey)
    "minResolution": null,        // e.g. "1920x1080"
    "ratios": null                // e.g. "16x9,21x9"
  },
  "pexels": { "apiKey": null }
}
```

Environment variables override the file: `WALLY_DOWNLOAD_DIR`, `WALLY_DEFAULT_SOURCE`,
`WALLY_SETTER`, `WALLY_WALLHAVEN_KEY`, `WALLY_PEXELS_KEY`. The standard `HTTPS_PROXY` /
`ALL_PROXY` variables are honored too, which helps if a source is blocked on your network.

## Building

Requires the .NET 10 SDK.

```sh
dotnet run -- --help                    # run from source
dotnet publish -c Release               # Native AOT single binary in bin/Release/net10.0/<rid>/publish/
```

Native AOT needs `clang` (and zlib) installed. Everything in wally is reflection-free
(System.CommandLine, source-generated JSON and P/Invoke), so it trims and AOT-compiles cleanly.

## For devs

- `Cli/` - commands (System.CommandLine)
- `Tui/` - the interactive menu
- `Sources/` - API sources (`IWallpaperSource`): Wallhaven, Pexels
- `Downloaders/` - the older HTML scrapers used by the menu
- `Setters/` - one `IWallpaperSetter` per desktop, picked at runtime
- `Core/` - config, shared `HttpClient`, process runner, paths

Adding a source: implement `IWallpaperSource` and register it in `SourceCatalog`.
Adding a desktop: implement `IWallpaperSetter` and add it to `WallpaperSetters.Detect`.

## Roadmap

- [x] CLI commands alongside the interactive menu
- [x] Wallhaven API source with resolution/ratio filters
- [x] Pexels via its API (no more Selenium)
- [x] Wallpaper setters for GNOME (light + dark), KDE, Xfce, MATE, Cinnamon, Hyprland/Sway/wlroots, X11, Windows
- [x] Config file + environment variables
- [ ] Library: favorites, tags, history, duplicate detection (`wally fav`, `wally history`, `wally next/prev`)
- [ ] Rotation: `wally daemon` and a systemd user timer to change the wallpaper every N minutes or daily
- [ ] Theming hook: run wallust / pywal (or any command) after a wallpaper changes
- [ ] Multi-monitor: different wallpaper per output
- [ ] More sources: Unsplash API, Bing image of the day, NASA APOD, Reddit (r/wallpapers & co.), Konachan API
- [ ] Packaging: AUR package and prebuilt AOT binaries on GitHub releases
- [ ] Shell completions (bash, fish, zsh)

# License

MIT
