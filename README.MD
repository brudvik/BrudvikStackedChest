# BrudvikStackedChest

## Overview

BrudvikStackedChest is a mod for Valheim that enhances the build process of the game by adding custom chests that is automatically adding various build items when placed
in the world. Each chest has a unique color and icon, and the contents will automatically be re-stocked when you either take something out of the chests
or use any of the items in the chest. Including when you use mods that allows for building from chests.

## Installation (manual)

1. Download the latest release of the mod from the [releases page](https://github.com/brudvik/BrudvikStackedChest/releases).
2. Extract the contents of the zip file.
3. Copy the `BrudvikStackedChest.dll` file into the `BepInEx/plugins` folder of your game directory.
4. Launch the game.

## Features

- **Custom Chests**: Adds new custom chests with which automatically spawns and re-stock items.

## Compilation

Please notice that it won't be possible to compile this `mod` out of the box. Make sure you read up on [Valheim Mod Development](https://github.com/Valheim-Modding/JotunnModStub).
There's also a directory removed from the source. The `Assets` folder is not part of the public source. The icon's I've bought from [Graphicriver.net](https://graphicriver.net/item/fantasy-strategy-skills/35481040) and
the license only allows it to be shipped in the pre-built mod file. If you want to make a similar `mod` you would need to buy a license from there.

## Changelog

### v0.0.4

- Fixed problem with setting correct hover text on chests.
- Fixed SpawnItems issue when new chests where created, but not yet placed.
- Removed Awake patch call, as it's no longer needed.

### v0.0.3

- Removed chests won't clutter the world anymore.
- Added an empty chest, for when you want to store your own things.

### v0.0.2

- Added missing SurtlingCore to materials chest.

### v0.0.1

- Initial release.

## Known issues

- No known issues at this time. Please report any issues on the [GitHub issues page](https://github.com/brudvik/BrudvikStackedChest/issues).

You can find the GitHub repository at: [https://github.com/brudvik/BrudvikStackedChest](https://github.com/brudvik/BrudvikStackedChest)
