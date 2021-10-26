# wally - The wallpaper downloader you’ve been waiting for (yeah right 🤣)

## Why?

Because there has to be one built using C# and .Net 6.

## No, Seriously why though 😕?

Because I, like many other chad linux users, like to rice my OS, and having a lot of wallpapers downloaded from different sites is usefull.

## So what gives?

At the moment, this “tool” probably only works on xfce based distros. In order to set wallpaper without having to restart the system, some DE specific files need to change. Maybe I’ll install a few VM’s and test other DE’s as well, but for now only xfce.

* * *

The BaseClass is the heart of it all, it handles Folder, Files and downloading the actuall images, Class specific sites just adjust the URL’s and names.

# TODO

- [ ] Add support for more websites
    - [x] WallpapersWide
    - [x] HdWallpapers
    - [ ] Unsplash
    - [ ] Pexels
    - [ ] and more
- [x] Auto set latest downloaded Wallpaper
- [x] More Resolutions
    - [x] show a list to select from
- [ ] More DE support
    - [ ] Cutefish
    - [x] XFCE
    - [ ] KDE
    - [ ] Gnome
    - [ ] Mate
    - [ ] Cinamon
- [ ] Write a propper README
    - [ ] and maybe some documentation

# License

MIT