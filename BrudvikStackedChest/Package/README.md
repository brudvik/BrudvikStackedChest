# BrudvikStackedChest

![Display of the mod in use](https://raw.githubusercontent.com/brudvik/BrudvikStackedChest/refs/heads/master/mod-example-chests.png)

## Overview

BrudvikStackedChest is a mod for Valheim that enhances the building experience by adding custom chests that automatically spawn and restock items. Each chest has a unique color and icon, and the contents will automatically be refilled to maximum stack size whenever you:

- Take items out of the chest
- Use items directly from the chest
- Use mods that allow building from chests (like EAQS or similar)

Perfect for builders who want to focus on creativity rather than grinding for materials!

## How It Works

### Building a Chest

All chests are built using the **Hammer** and can be found in the **"Chests"** category. Each chest requires:

| Resource | Amount | Recoverable |
|----------|--------|-------------|
| Wood     | 10     | Yes         |

### Automatic Restocking

Once placed, the chest will:
1. **Spawn initial items** - All configured items appear at maximum stack size
2. **Monitor changes** - The chest watches for inventory changes via Harmony patching
3. **Refill automatically** - When items are removed, they are instantly refilled to max stack

### Removing Chests

When you destroy or remove a chest:
- **All unlimited contents are automatically deleted** (no item drops)
- In the Linear and Discovered modes, items you stored yourself are dropped as with any other chest
- This prevents cluttering your world with unwanted items
- The chest can be safely relocated without spawning duplicate items

> **Warning**: In Full mode, custom items stored in the "Everlasting Chest" are lost when the chest is removed!

### Chest Modes

The `Mode` setting decides how generous the chests are:

| Mode | Behavior |
|------|----------|
| Full | Every item is always available, as in earlier versions |
| Linear | A chest works like a normal chest until it holds a full stack of an item. From then on the item is unlimited in every chest of that type in the world. You play normally, but the grinding stops once you have gathered enough of something |
| Discovered | An item is unlimited as soon as any player in the world has discovered it |

In Linear and Discovered:
- Unlocked and discovered items are shared by everyone in the world and saved by the server in `BepInEx/config/BrudvikStackedChest/`
- Items that do not stack, like weapons and armor, are never duplicated; those chests work as normal chests
- The hover text of a chest shows how many of its items are unlimited
- The Everlasting Chest makes any stackable item unlimited once it holds a full stack (Linear) or once the item is discovered (Discovered)

### Seeing Your Progress

- Unlimited stacks show a gold **∞** instead of their amount, in every mode
- In Linear mode, items that can still be unlocked get a blue bar under the slot that fills up as you store more of them
- The item tooltip tells whether the item is unlimited, how many more you need to store, or why it is stored normally
- The chest title and hover text show how many of the chest's items are unlimited, and the hover text lists the three items closest to being unlocked
- Everyone on the server is told when a player makes an item unlimited
- A chest glows brighter the more of its items are unlimited, turns gold when all of them are, and plays an effect when it is completed
- The `bsc_progress` command (console or chat) lists the progress of every chest

Switching to a less generous mode removes the unlimited items the new mode no longer supplies, for example everything that is not unlocked when going from Full to Linear.

---

## Available Chests

The contents of every chest are generated automatically when a world loads. The mod looks at every item in the game, including items added by other mods, and sorts it by its type and by where it comes from. New items from game updates show up in the right chest without the mod needing an update.

| Chest | Color | Contents |
|-------|-------|----------|
| Wood Chest | Black | Everything that drops from trees and logs, plus what is made from it only (Coal) |
| Stone Chest | Gray | Everything that drops from rocks and mineable deposits, plus what is crafted from stone only |
| Metal Chest | Dark Red | Ores, scrap and bars from the smelter and blast furnace, plus what is crafted from metal only (Bronze, nails, chains) |
| Food Chest | Brown | All food, fish, raw and uncooked ingredients for the cooking station and oven, and anything used in a food recipe |
| Material Chest | Dark Blue | All remaining crafting materials |
| Animal Chest | Yellow | Materials dropped by creatures (hides, bones, scales, feathers, and so on) |
| Seed Chest | Green | Seeds, cones and nuts that are planted but not used as ingredients |
| Trophy Chest | Teal | All trophies |
| Treasure Chest | Gold/Yellow | Items with a trade value (Coins, Amber, Ruby, and so on), keys, eggs and boss rewards |
| Tools Chest | Purple | Tools, torches, lanterns, pickaxes, fishing rods and bait, utility items, saddles and other crafted gear |
| Armor Chest | Brown | All helmets, chest pieces, legs, capes and trinkets |
| Weapon Chest | Red | All weapons, shields, arrows, bolts and bombs |
| Potion Chest | Pink/Purple | All meads and mead bases |
| Everlasting Chest | Dark Gray | Empty - add your own items and they will be restocked automatically! |

Items that cannot be obtained in normal play (creature attacks, test items and unused variants) are left out.

## Configuration

The configuration file is `BepInEx/config/com.jotunn.BrudvikStackedChest.cfg`. The chest settings are synchronized from the server, so every player on a server sees the same chests.

| Section | Setting | Description |
|---------|---------|-------------|
| General | DumpItemLists | Writes the generated item list of every chest to the BepInEx log, including items that could not be sorted |
| General | Mode | Full, Linear or Discovered, see [Chest Modes](#chest-modes) |
| General | UnlockStacks | Linear mode: the number of full stacks a chest must hold to unlock an item (1-10, default 1) |
| Chest.&lt;Name&gt; | Include | Comma-separated prefab names that are always placed in this chest, overriding the automatic sorting |
| Chest.&lt;Name&gt; | Exclude | Comma-separated prefab names that are never placed in this chest |

Changes take effect without restarting the game. A chest never removes items that are already in it; remove unwanted items by hand.

---

## Visual Effects

All custom chests now feature:

- **Glow Effect**: Each chest emits a subtle glow in its respective color, making them easy to identify. Like the game's own lights, the glow switches off at a distance
- **Particle Effects**: A sparkle effect appears when items are automatically refilled (at most once every 10 seconds per chest), and an unlock effect plays when an item becomes unlimited in Linear mode

---

## Technical Details

### Chest Specifications

| Property | Value |
|----------|-------|
| Inventory Size | 8 x 8 (64 slots), grows automatically when a chest needs more room |
| Base Prefab | piece_chest |
| Build Category | Chests |
| Build Tool | Hammer |

### Architecture

The mod uses several key technologies:

1. **Jotunn (JVL)** - For piece registration and prefab management
2. **Harmony Patching** - To intercept container events
3. **Event-Driven Design** - Clean separation of concerns

#### Harmony Patches

| Method | Patch Type | Purpose |
|--------|------------|---------|
| Container.CheckForChanges | Postfix | Triggers item refill after inventory changes |
| Container.DropAllItems | Prefix | Removes unlimited items before removal to prevent item drops |
| Container.GetHoverText | Postfix | Shows how many of the chest's items are unlimited |
| Player.OnSpawned | Postfix | Fetches the world progress from the server and shares discovered items |
| Player.AddKnownItem | Postfix | Shares newly discovered items in Discovered mode |
| InventoryGrid.UpdateGui | Postfix | Shows ∞ on unlimited stacks and unlock progress bars |
| InventoryGrid.CreateItemTooltip | Postfix | Adds the item's status in the chest to its tooltip |
| InventoryGui.UpdateContainer | Postfix | Adds the chest's progress to the container title |

---

## Installation (Manual)

1. Download the latest release from the [releases page](https://github.com/brudvik/BrudvikStackedChest/releases)
2. Extract the contents of the zip file
3. Copy BrudvikStackedChest.dll to BepInEx/plugins folder
4. Launch Valheim

### Requirements

- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) (5.4.2351 or later)
- [Jotunn (JVL)](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/) (2.30.x or later)

---

## Compilation

Please notice that it will not be possible to compile this mod out of the box. Make sure you read up on [Valheim Mod Development](https://github.com/Valheim-Modding/JotunnModStub).

There is also a directory removed from the source. The Assets folder is not part of the public source. The icons I have bought from [Graphicriver.net](https://graphicriver.net/item/fantasy-strategy-skills/35481040) and the license only allows it to be shipped in the pre-built mod file. If you want to make a similar mod you would need to buy a license from there.

---

## Thank You, Community!

As a thank you for the community in assisting me to learn about mod development in Valheim, I figured that the least I could do was to share the source of my findings and the result of the learning process. The mod itself is not that great, it was more about the adventure in reaching the goal. A working mod, that could assist me in what I love the most - simply exploring and building. And yes, I know it could be done using debugmode - but that takes away the fun in exploring a new world, though I hate grinding - thus this mod.

### Learning Showcase

This mod demonstrates several modding techniques:

- Using JVL (Jotunn, the Valheim Library)
- Harmony prefix/postfix patching
- Event-driven architecture
- C# extension methods
- Embedded resources (sprites/icons)
- Prefab cloning and modification
- Custom piece configuration

---

## Changelog

### v0.2.0

- Added: Chest contents are generated automatically from the game data, so new items no longer need a mod update
- Added: Configuration file with Include and Exclude lists per chest, synchronized from the server
- Added: DumpItemLists setting that writes the generated chest contents to the log
- Added: Linear mode - store a full stack of an item to make it unlimited for the whole world
- Added: Discovered mode - items are unlimited once any player in the world has discovered them
- Added: UnlockStacks setting for how many full stacks Linear mode requires
- Added: Chest hover text shows how many items are unlimited in the Linear and Discovered modes
- Added: Unlimited stacks show ∞, and Linear mode shows unlock progress bars, tooltips and the items closest to being unlocked
- Added: All players are told when someone makes an item unlimited
- Added: Chest glow follows the progress, turns gold when a chest is complete, and plays an effect when it gets there
- Added: bsc_progress command that lists the progress of every chest
- Changed: In the Linear and Discovered modes, destroyed chests drop the items players stored themselves
- Changed: Chests grow automatically when their contents need more than the default number of rows
- Changed: Items that cannot be obtained in normal play are no longer placed in chests
- Changed: Chest glow lights switch off at a distance, like the game's own lights
- Fixed: The refill sparkle effect listed since v0.1.0 was never implemented; it now plays when a chest refills
- Fixed: The Wood Chest and the Everlasting Chest gave little or no glow because of their dark color
- Fixed: Chests saved every second even when nothing changed, which caused needless network traffic
- Fixed: In multiplayer, only the player that owns a chest restocks it, so clients no longer overwrite each other

### v0.1.1

- Updated: Compiled against Valheim 1.0.16 (Unity 6), Jotunn 2.30.2 and BepInExPack 5.4.2351
- Fixed: Misspelled prefab names (MushroomJotunPuffs, Fiddleheadfern, TrophyGoblinShaman)
- Fixed: Removed prefab names that do not exist in the game, which kept chests from ever counting as full and caused them to reload and save constantly

### v0.1.0

- Added: Armor Chest - complete armor sets from all tiers (Leather, Troll, Bronze, Iron, Wolf, Padded, Carapace, Flametal, Mage, Fenris, Root)
- Added: Weapon Chest - weapons from all biomes (swords, axes, maces, atgeirs, spears, knives, bows, crossbows, staffs, arrows, bolts, shields)
- Added: Potion Chest - all meads and potions (health, stamina, eitr, resistance meads + mead bases)
- Added: Glow effect - all chests now glow subtly in their respective color
- Added: Particle effects - sparkle effect when items are automatically refilled

### v0.0.4

- Fixed problem with setting correct hover text on chests.
- Fixed SpawnItems issue when new chests where created, but not yet placed.
- Removed Awake patch call, as it is no longer needed.

### v0.0.3

- Removed chests will not clutter the world anymore.
- Added an empty chest, for when you want to store your own things.

### v0.0.2

- Added missing SurtlingCore to materials chest.

### v0.0.1

- Initial release.

## Known Issues

- No known issues at this time. Please report any issues on the [GitHub issues page](https://github.com/brudvik/BrudvikStackedChest/issues).

---

You can find the GitHub repository at: [https://github.com/brudvik/BrudvikStackedChest](https://github.com/brudvik/BrudvikStackedChest)
