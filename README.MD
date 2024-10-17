# BrudvikStackedChest

![Display of the mod in use](https://raw.githubusercontent.com/brudvik/BrudvikStackedChest/refs/heads/master/mod-example-chests.png)

## Overview

BrudvikStackedChest is a mod for Valheim that enhances the build process of the game by adding custom chests that is automatically adding various build items when placed
in the world. Each chest has a unique color and icon, and the contents will automatically be re-stocked when you either take something out of the chests
or use any of the items in the chest. Including when you use mods that allows for building from chests.

## Chests

The everlasting chests available for build are:

- Wood Chest (wood materials)
- Stone Chest (stone materials)
- Metal Chest (metal materials)
- Food Chest (food materials)
- Material Chest (various build materials)
- Animal Chest (various animal build materials)
- Seed Chest (seeds)
- Trophy Chest (all the trophies)
- Treasure Chest (treasures)
- Tools Chest (barber kit, megingjord..)
- Empty Chest (add what you want)

When you decide to remove a chest, all the contents will be gone - so it'll be safe to move the chests around. However, if you do it on the empty chest, be aware that all the contents will also be gone.

## Thank you, community!

As a thank you for the community in assisting me to learn about mod development in Valheim, I figured that the least I could do was to share the source of my findings and the result of the learning process. The mod itself isn’t that great, it was more about the adventure in reaching the goal. A working mod, that could assist me in what I love the most – simply exploring and building. And yes, I know it could be done using “debugmode” – but that takes away the fun in exploring a new world, though I hate grinding – thus this mod. Basically, create various chests that spawns in materials that never runs out.

Now this mod showcases a few things – some are simple, some require a bit more understanding of C# - but all very much learnable. The MOD is based on the [JotunnModStub](https://github.com/Valheim-Modding/JotunnModStub) project:

- The use of JVL (Jötunn, the Valheim Library) 
-	The use of Harmony patching
-	The use of events
-	The use of extension methods
-	The use of Sprites
-	The use of embedded resources
-	The use of extended classes to add new methods

Regarding the actual mod – it creates a custom chest, based on the “piece_chest” prefab. The code gives you the ability to change the color of the chest, add an icon and place it in front of the chest, adjust the tooltip and adjust the number of columns and rows. Perhaps ordeal to experienced mod-developers, but somethings that I had to learn by putting together various examples and a lot of trial-and-error.

The Harmony patches focus on altering two methods of the original “Container” class. The “CheckForChanges” method and the “DropAllItems” method. The first will alter what happens after the method is performed, the other will alter the method before it’s performed. Showcasing how this can be implemented to achieve different things. Instead of cluttering the methods, those patches instead issues events, which can then be handled elsewhere.

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
