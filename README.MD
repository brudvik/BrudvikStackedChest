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
- **All contents are automatically deleted** (no item drops)
- This prevents cluttering your world with unwanted items
- The chest can be safely relocated without spawning duplicate items

> **Warning**: If you store custom items in the "Everlasting Chest", they will be lost when the chest is removed!

---

## Available Chests

### Wood Chest
*A chest filled with wood materials*

| Color | Items |
|-------|-------|
| Black | Wood, FineWood, RoundLog, Blackwood, Coal, AncientBark, Root, YggdrasilWood, ElderBark |

### Stone Chest
*A chest filled with stone materials*

| Color | Items |
|-------|-------|
| Gray | Flint, Obsidian, BlackMarble, Stone, Grausten, SharpeningStone, CeramicPlate |

### Metal Chest
*A chest filled with metal materials*

| Color | Items |
|-------|-------|
| Dark Red | Iron, IronNails, Bronze, BronzeNails, Silver, BlackMetal, Tin, Copper, Chain, CharredCogwheel, Flametal, FlametalNew, MechanicalSpring, IronScrap, CopperScrap, CopperOre, TinOre, IronOre, SilverOre, BlackMetalScrap, FlametalOre |

### Food Chest
*A chest filled with food ingredients*

| Color | Items |
|-------|-------|
| Brown | Raw ingredients: Barley, BarleyFlour, Blueberries, Carrot, Honey, Mushrooms, Onion, Raspberry, Turnip, and many more. Cooked foods: Bread, Sausages, QueensJam, CarrotSoup, TurnipStew, BloodPudding, FishWraps, various cooked meats, and more! |

### Material Chest
*A chest filled with various crafting materials*

| Color | Items |
|-------|-------|
| Dark Blue | BlackCore, Crystal, Eitr, RefinedEitr, Guck, LinenThread, MoltenCore, Needle, Ooze, Resin, Sap, SurtlingCore, Tar, Thunderstone, Wisp, Gemstones (Blue/Green/Red), and more! |

### Animal Chest
*A chest filled with animal materials*

| Color | Items |
|-------|-------|
| Yellow | BoneFragments, Carapace, Chitin, DeerHide, DragonTear, Feathers, GreydwarfEye, LeatherScraps, LoxPelt, TrollHide, WolfPelt, WolfFang, SerpentScale, and more! |

### Seed Chest
*A chest filled with seeds and saplings*

| Color | Items |
|-------|-------|
| Green | Acorn, AncientSeed, BeechSeeds, BirchSeeds, CarrotSeeds, OnionSeeds, PineCone, TurnipSeeds, VineberrySeeds, FirCone, Sap, BarleyWild, FlaxWild |

### Trophy Chest
*A chest filled with all trophies*

| Color | Items |
|-------|-------|
| Teal | All boss trophies (Eikthyr, TheElder, Bonemass, DragonQueen, GoblinKing, SeekerQueen, Fader) + all creature trophies (60+ different trophies!) |

### Treasure Chest
*A chest filled with treasures and riches*

| Color | Items |
|-------|-------|
| Gold/Yellow | Amber, AmberPearl, Coins, Ruby, SilverNecklace, Gemstones, DragonEgg, GoblinTotem, Hildir Keys |

### Tools Chest
*A chest filled with useful tools*

| Color | Items |
|-------|-------|
| Purple | BarberKit, BeltStrength (Megingjord), CryptKey, FishingRod, All fishing baits, Lantern, Wishbone, Hammer, Hoe, Cultivator, All pickaxes, SaddleLox, Demister, DvergrKey, and more! |

### Armor Chest
*A chest filled with armor sets from all tiers*

| Color | Items |
|-------|-------|
| Brown | Complete armor sets: Leather, Troll, Bronze, Iron, Wolf/Silver, Padded, Carapace, Flametal, Mage, Fenris, Root. Plus all capes! |

### Weapon Chest
*A chest filled with weapons from all biomes*

| Color | Items |
|-------|-------|
| Red | Swords, Axes, Maces, Atgeirs, Spears, Knives, Bows, Crossbows, Staffs, Arrows, Bolts, and Shields from all tiers! |

### Potion Chest
*A chest filled with meads and potions*

| Color | Items |
|-------|-------|
| Pink/Purple | Health meads (Minor/Medium/Major), Stamina meads, Eitr meads, Resistance meads (Frost/Poison/Bug), Tasty mead, and all mead bases for brewing! |

### Everlasting Chest
*An empty chest - add items to make them last forever*

| Color | Items |
|-------|-------|
| Dark Gray | Empty - add your own items and they will be restocked automatically! |

---

## Visual Effects

All custom chests now feature:

- **Glow Effect**: Each chest emits a subtle glow in its respective color, making them easy to identify
- **Particle Effects**: A sparkle effect appears when items are automatically refilled

---

## Technical Details

### Chest Specifications

| Property | Value |
|----------|-------|
| Inventory Size | 8 x 8 (64 slots) |
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
| Container.DropAllItems | Prefix | Empties chest before removal to prevent item drops |

---

## Installation (Manual)

1. Download the latest release from the [releases page](https://github.com/brudvik/BrudvikStackedChest/releases)
2. Extract the contents of the zip file
3. Copy BrudvikStackedChest.dll to BepInEx/plugins folder
4. Launch Valheim

### Requirements

- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) (5.4.x or later)
- [Jotunn (JVL)](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/) (2.20.x or later)

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
