# BrudvikStackedChest

> **⚠️ This mod is outdated and no longer maintained.** The chests are now part of [BrudvikWhiteHilt](https://github.com/brudvik/BrudvikWhiteHilt) (from v0.29.0), see [Restocking chests](https://github.com/brudvik/BrudvikWhiteHilt/blob/master/docs/chests.md).
>
> Placed chests and their contents carry over. To switch:
> 1. Back up your world.
> 2. Install BrudvikWhiteHilt 0.29.0 or newer on the server and every client while BrudvikStackedChest is still installed, and start the world once.
> 3. Remove BrudvikStackedChest, but **keep** `BepInEx/config/com.jotunn.BrudvikStackedChest.cfg` and the `BepInEx/config/BrudvikStackedChest/` folder. White Hilt takes over the settings (above all `Mode`) and the world progress from them.
>
> Do not run both mods side by side for longer than the switch: while BrudvikStackedChest is loaded, White Hilt leaves the chests to it.

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
1. **Spawn initial items** - All configured items appear at maximum stack size, filled row by row from the top
2. **Monitor changes** - The chest watches for inventory changes via Harmony patching
3. **Refill automatically** - As soon as items are removed, they are refilled to max stack

Every unlimited item takes up one full stack. When a single recipe or build piece needs more than one stack (for example more than 50 wood), the chest keeps enough full stacks for it, so anything you can build from the chest actually gets built.

Putting an item into a chest that already holds it without limit (ctrl-click, drag and drop or the "Place stacks" button) removes the item from your inventory instead of creating yet another stack. The chest still holds the item, so you can take it out again at any time. This makes "Place stacks" a quick way to empty your pockets.

The "Take all" button only takes the items you stored yourself and leaves the unlimited stacks in the chest.

Chests keep their contents sorted from the top left: unlimited items first, then the items you stored yourself, each by item type and name. Turn off `SortContents` to arrange the chests yourself.

### Removing Chests

When you destroy or remove a chest:
- **All unlimited contents are automatically deleted** (no item drops)
- Items you stored yourself are dropped as with any other chest. In Full mode, stackable items are always unlimited and are deleted as well, while weapons, armor and other gear that is upgraded or crafted by a player is dropped
- This prevents cluttering your world with unwanted items
- The chest can be safely relocated without spawning duplicate items

> **Warning**: In Full mode, stackable items stored in the "Everlasting Chest" are lost when the chest is removed!

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

### Seeing What Is in a Chest

- **On the front of the chest**: the icon turns grey when the chest is empty. A light bar under the icon shows how many of the chest's slots are used, and in the Linear and Discovered modes a gold bar shows how many of the chest's items are unlimited
- **Looking at a chest**: a panel below the crosshair shows the chest's contents as item icons with their amount (∞ for unlimited stacks), how many slots are used and how many items are unlimited

Both can be switched off in the `Display` section of the configuration. Every player sees the same thing, without any extra network traffic.

### Learning Items

Items you take out of a chest teach your character new recipes and build pieces, just like items you pick up. New players on a server often have not touched most of the chest contents yet:

- Items in an open chest that you have not learned yet are marked with a yellow **!**, and their tooltip says so
- A **Learn all** button next to "Place stacks" teaches you every item in the chest at once, with one message instead of one per item and recipe
- Learning is stored on your character, not in the world
- The `LearnAll` setting removes the button, and `LearnTrophies` decides whether it also adds trophies to your trophy list

### Gathering Progress

The player screen (Tab) has an extra button next to the Skills, Compendium, Trophies and Achievements buttons. It opens a panel that lists what can be gathered in the biome you are in:

- Every item that trees, rocks, plants and creatures in the biome drop, plus items found in its dungeons. Crafted items are left out
- Linear mode: a bar with the amount the closest chest in the world holds and the amount still missing. The server remembers the amount in every chest, so chests far away count as well
- Discovered mode: whether the item has been discovered
- A gold **∞** for items that are already unlimited
- How many you carry in your inventory
- Items that do not stack are greyed out, because they are stored normally
- The arrows browse the other biomes you have visited

The amounts are saved by the server in `BepInEx/config/BrudvikStackedChest/`. After updating the mod, a chest counts once it has been loaded, which happens when a player comes near it.

### Languages

The mod follows the language chosen in the game settings. English and Norwegian are included; other languages show English.

Switching to a less generous mode removes the unlimited items the new mode no longer supplies, for example everything that is not unlocked when going from Full to Linear.

---

## Available Chests

The contents of every chest are generated automatically when a world loads. The mod looks at every item in the game, including items added by other mods, and sorts it by its type and by where it comes from. New items from game updates show up in the right chest without the mod needing an update.

| Chest | Color | Contents |
|-------|-------|----------|
| Wood Chest | Black | Everything that drops from trees and logs (wood, resin, bark), plus Coal |
| Stone Chest | Gray | Everything that is mined from rocks and deposits (stone, flint, obsidian, crystal, sulfur), plus what is crafted from stone only |
| Metal Chest | Dark Red | Ores, scrap and bars from the smelter and blast furnace, plus what is crafted from metal only (nails) and Chain |
| Food Chest | Brown | All food, fish, raw and uncooked ingredients for the cooking station and oven, and anything used in a food recipe |
| Material Chest | Dark Blue | All remaining crafting materials, including Surtling Core, Ectoplasm, Flax, casts and moulds |
| Animal Chest | Yellow | Materials dropped by creatures (hides, bones, scales, feathers, leather scraps, and so on) |
| Seed Chest | Green | Seeds, cones and nuts that are planted but not used as ingredients |
| Trophy Chest | Teal | All trophies |
| Treasure Chest | Gold/Yellow | Items with a trade value (Coins, Amber, Ruby, and so on), gemstones, keys, eggs and boss rewards |
| Tools Chest | Purple | Pickaxes, building and farming tools, fishing rods and bait, torches, lanterns, saddles, utility items and other crafted gear |
| Armor Chest | Brown | All helmets, chest pieces, legs, capes and trinkets |
| Weapon Chest | Red | All weapons, shields, arrows, bolts and bombs |
| Potion Chest | Pink/Purple | All meads and mead bases |
| Everlasting Chest | Dark Gray | Empty - add your own items and they will be restocked automatically! |

Items that cannot be obtained in normal play (creature attacks, test items and unused variants) are left out.

See [Chest Contents](#chest-contents) for every item in every chest.

## Configuration

The configuration file is `BepInEx/config/com.jotunn.BrudvikStackedChest.cfg`. The chest settings are synchronized from the server, so every player on a server sees the same chests.

| Section | Setting | Description |
|---------|---------|-------------|
| General | DumpItemLists | Writes the generated item list of every chest to the BepInEx log, including items that could not be sorted. The items of every biome are written when the gathering panel is first opened |
| General | Mode | Full, Linear or Discovered, see [Chest Modes](#chest-modes) |
| General | UnlockStacks | Linear mode: the number of full stacks a chest must hold to unlock an item (1-10, default 1) |
| General | SortContents | Keeps the chest contents sorted from the top left, unlimited items first (default on). When off, new items are placed in the first free slot from the top |
| General | LearnAll | Shows a Learn all button on chests that hold items the player has not learned yet (default on) |
| General | LearnTrophies | Lets the Learn all button learn trophies as well, which adds them to the player's trophy list (default on) |
| Chest.&lt;Name&gt; | Include | Comma-separated prefab names that are always placed in this chest, overriding the automatic sorting |
| Chest.&lt;Name&gt; | Exclude | Comma-separated prefab names that are never placed in this chest |
| Display | ShowIndicators | Greys out the icon of empty chests and shows the slot and unlimited bars on the front of chests (default on, per player) |
| Display | ShowHoverPanel | Shows the contents of the chest you look at as item icons (default on, per player) |

Changes take effect without restarting the game. The `General` and `Chest` settings are synchronized from the server; the `Display` settings are chosen by each player. A chest never removes items that are already in it; remove unwanted items by hand.

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
| Container.CheckForChanges | Postfix | Refills the chest every second |
| Container.OnContainerChanged | Postfix | Refills the chest as soon as its contents change |
| Container.DropAllItems | Prefix | Removes unlimited items before removal to prevent item drops |
| Container.GetHoverText | Postfix | Shows how many of the chest's items are unlimited |
| Inventory.AddItem | Prefix | Absorbs items put into a chest that already holds them without limit (ctrl-click, "Place stacks") |
| Inventory.MoveItemToThis | Prefix | Absorbs unlimited items dropped on a chest slot |
| Inventory.MoveAll | Prefix | Makes "Take all" leave the unlimited stacks in the chest |
| Player.OnSpawned | Postfix | Fetches the world progress from the server and shares discovered items |
| Player.AddKnownItem | Postfix | Shares newly discovered items in Discovered mode |
| InventoryGrid.UpdateGui | Postfix | Shows ∞ on unlimited stacks and unlock progress bars |
| InventoryGrid.CreateItemTooltip | Postfix | Adds the item's status in the chest to its tooltip |
| InventoryGui.UpdateContainer | Postfix | Adds the chest's progress to the container title and shows the Learn all button |
| InventoryGui.Show | Postfix | Adds the gathering button and panel to the player screen |
| InventoryGui.Hide | Postfix | Closes the gathering panel |
| InventoryGui.OnOpenSkills, OnOpenTexts, OnOpenTrophies, OnOpenAchievements | Postfix | Closes the gathering panel when another panel opens |
| MessageHud.QueueUnlockMsg | Prefix | Leaves out the message for every single item and recipe while Learn all runs |

---

## Chest Contents

These are the contents of every chest with the default settings in Valheim 1.0.16 without other mods. Items from other mods and from later game updates are sorted into the chests as well. Use the prefab name in the Include and Exclude settings to move an item to another chest. In the Linear and Discovered modes a chest only restocks the items that are unlocked or discovered.

### Wood Chest (11 items)

| Item | Prefab name |
|------|-------------|
| Ancient Bark | ElderBark |
| Ashwood | Blackwood |
| Charcoal Resin | CharcoalResin |
| Coal | Coal |
| Corewood | RoundLog |
| Finewood | FineWood |
| Resin | Resin |
| Root | Root |
| Timberwood | Frostwood |
| Wood | Wood |
| Yggdrasil Wood | YggdrasilWood |

### Stone Chest (12 items)

| Item | Prefab name |
|------|-------------|
| Black Marble | BlackMarble |
| Ceramic Plate | CeramicPlate |
| Crystal | Crystal |
| Flint | Flint |
| Grausten | Grausten |
| Grausten Payload | Catapult_ammo |
| Ice | Ice |
| Obsidian | Obsidian |
| Rock | StoneRock |
| Sharpening Stone | SharpeningStone |
| Stone | Stone |
| Sulfur | SulfurStone |

### Metal Chest (21 items)

| Item | Prefab name |
|------|-------------|
| Black Metal | BlackMetal |
| Black Metal Scrap | BlackMetalScrap |
| Bloodgold | Gold |
| Bronze | Bronze |
| Bronze Nails | BronzeNails |
| Chain | Chain |
| Copper | Copper |
| Copper Ore | CopperOre |
| Copper Scrap | CopperScrap |
| Flametal | FlametalNew |
| Flametal Ore | FlametalOreNew |
| Iron | Iron |
| Iron Nails | IronNails |
| Iron Ore | IronOre |
| Petrified Tissue | GoldOre |
| Scrap Bronze | BronzeScrap |
| Scrap Iron | IronScrap |
| Silver | Silver |
| Silver Ore | SilverOre |
| Tin | Tin |
| Tin Ore | TinOre |

### Food Chest (137 items)

| Item | Prefab name |
|------|-------------|
| Anglerfish | Fish9 |
| Asksvin Tail | AsksvinMeat |
| Baked Poteitr | BakedPoteitr |
| Barley | Barley |
| Barley Flour | BarleyFlour |
| Bear Meat | BjornMeat |
| Black Soup | BlackSoup |
| Blood Pudding | BloodPudding |
| Blue Mushroom | MushroomBlue |
| Blueberries | Blueberries |
| Boar Jerky | BoarJerky |
| Boar Meat | RawMeat |
| Bonemaw Meat | BoneMawSerpentMeat |
| Bread | Bread |
| Bread Dough | BreadDough |
| Carrot | Carrot |
| Carrot Soup | CarrotSoup |
| Chicken Meat | ChickenMeat |
| Cloudberries | Cloudberry |
| Cooked Asksvin Tail | CookedAsksvinMeat |
| Cooked Bear Meat | CookedBjornMeat |
| Cooked Boar Meat | CookedMeat |
| Cooked Bonemaw Meat | CookedBoneMawSerpentMeat |
| Cooked Chicken Meat | CookedChickenMeat |
| Cooked Deer Meat | CookedDeerMeat |
| Cooked Egg | CookedEgg |
| Cooked Fish | FishCooked |
| Cooked Hare Meat | CookedHareMeat |
| Cooked Lox Meat | CookedLoxMeat |
| Cooked Moose Meat | CookedMooseMeat |
| Cooked Seal Blubber | CookedSealBlubber |
| Cooked Seeker Meat | CookedBugMeat |
| Cooked Serpent Meat | SerpentMeatCooked |
| Cooked Volture Meat | CookedVoltureMeat |
| Cooked Wolf Meat | CookedWolfMeat |
| Coral Cod | Fish8 |
| Deer Meat | DeerMeat |
| Deer Stew | DeerStew |
| Egg | ChickenEgg |
| Eyescream | Eyescream |
| Fiddlehead | Fiddleheadfern |
| Fiery Svinstew | FierySvinstew |
| Fish 'n' Bread | FishAndBread |
| Fish Soup | FishSoup |
| Fish Wraps | FishWraps |
| Frosted Sweetbread | VikingCupcake |
| Giant Herring | Fish6 |
| Grilled Neck Tail | NeckTailGrilled |
| Grouper | Fish7 |
| Hare Meat | HareMeat |
| Honey | Honey |
| Honey Glazed Chicken | HoneyGlazedChicken |
| Jotun Puffs | MushroomJotunPuffs |
| Kale | Kale |
| Kale Chips | KaleChips |
| Lingonberries | Lingonberry |
| Lingonberry Juice | Lingondricka |
| Lox Meat | LoxMeat |
| Lox Meat Pie | LoxPie |
| Luminous Larva | GlowWorm |
| Magecap | MushroomMagecap |
| Magmafish | Fish11 |
| Marinated Greens | MarinatedGreens |
| Mashed Meat | MashedMeat |
| Meat In Bread | MooseKebab |
| Meat Platter | MeatPlatter |
| Meatballs and Poteitr | MeatballsMashedPoteitr |
| Minced Meat Sauce | MinceMeatSauce |
| Misthare Supreme | MisthareSupreme |
| Moose Meat | MooseMeat |
| Muckshake | ShocklateSmoothie |
| Mushroom | Mushroom |
| Mushroom Omelette | MushroomOmelette |
| Neck Tail | NeckTail |
| Northern Salmon | Fish10 |
| Oat Flour | OatFlour |
| Oat Milk | OatMilk |
| Oatmeal | OatmealLingonberryJam |
| Oats | Oat |
| Onion | Onion |
| Onion Soup | OnionSoup |
| Oven Pancake | OvenPancake |
| Oven Pancake Batter | OvenPancakeUncooked |
| Pancakes | Pancakes |
| Perch | Fish1 |
| Pike | Fish2 |
| Piquant Pie | PiquantPie |
| Poteitr | Poteitr |
| Pufferfish | Fish12 |
| Pulled Bear | PulledBear |
| Queen's Jam | QueensJam |
| Raspberries | Raspberry |
| Raw Fish | FishRaw |
| Raw Kale Chips | KaleChipsUncooked |
| Roasted Crust Pie | RoastedCrustPie |
| Royal Jelly | RoyalJelly |
| Salad | Salad |
| Sap | Sap |
| Sausages | Sausages |
| Scorching Medley | ScorchingMedley |
| Seal Blubber | SealBlubber |
| Seal Meat Soup | SealSoup |
| Seeker Aspic | SeekerAspic |
| Seeker Meat | BugMeat |
| Serpent Meat | SerpentMeat |
| Serpent Stew | SerpentStew |
| Sizzling Berry Broth | SizzlingBerryBroth |
| Smoke Puff | MushroomSmokePuff |
| Smoked Fish | SmokedFish |
| Smoked Moose Meat | SmokedMooseMeat |
| Sparkling Shroomshake | SparklingShroomshake |
| Spicy Marmalade | SpicyMarmalade |
| Stuffed Mushroom | MagicallyStuffedShroom |
| Tetra | Fish4_cave |
| Thistle | Thistle |
| Trollfish | Fish5 |
| Tuna | Fish3 |
| Turnip | Turnip |
| Turnip Stew | TurnipStew |
| Unbaked Lox Pie | LoxPieUncooked |
| Unbaked Poteitr | BakedPoteitrUncooked |
| Unbaked Sweetbread | VikingCupcakeUncooked |
| Uncooked Fish 'n' Bread | FishAndBreadUncooked |
| Uncooked Honey Glazed Chicken | HoneyGlazedChickenUncooked |
| Uncooked Meat Platter | MeatPlatterUncooked |
| Uncooked Misthare Supreme | MisthareSupremeUncooked |
| Uncooked Piquant Pie | PiquantPieUncooked |
| Uncooked Roasted Crust Pie | RoastedCrustPieUncooked |
| Uncooked Stuffed Mushroom | MagicallyStuffedShroomUncooked |
| Vineberry Cluster | Vineberry |
| Volture Egg | VoltureEgg |
| Volture Meat | VoltureMeat |
| Wolf Jerky | WolfJerky |
| Wolf Meat | WolfMeat |
| Wolf Skewer | WolfMeatSkewer |
| Yellow Mushroom | MushroomYellow |
| Yggdrasil Porridge | YggdrasilPorridge |

### Material Chest (138 items)

| Item | Prefab name |
|------|-------------|
| Ashlands Gourmet Bowl | FeastAshlands_Material |
| Basic Fireworks | FireworksRocket_White |
| Bell Fragment | BellFragment |
| Black Core | BlackCore |
| Black Forest Buffet Platter | FeastBlackforest_Material |
| Black Marble Battle Idol | Upgrader5Weapon |
| Black Marble Protection Idol | Upgrader5Armor |
| Black Metal Battle Idol | Upgrader4Weapon |
| Black Metal Protection Idol | Upgrader4Armor |
| Bloodgold Battle Idol | Upgrader7Weapon |
| Bloodgold Payload | Catapult_Ammo_BloodGold |
| Bloodgold Protection Idol | Upgrader7Armor |
| Blue Fireworks | FireworksRocket_Blue |
| Blue Jute | JuteBlue |
| Bronze Battle Idol | Upgrader1Weapon |
| Bronze Protection Idol | Upgrader1Armor |
| Candle Wick | CandleWick |
| Cast: Breastplate of the Protector | ArmorGoldHeavyChestUncooked |
| Cast: Chestpiece of the Vanguard | ArmorGoldMediumChestUncooked |
| Cast: Echo Spike | StaffOrbofAhriUncooked |
| Cast: Headdress of the Caller | ArmorGoldMageHelmetUncooked |
| Cast: Helmet of the Protector | ArmorGoldHeavyHelmetUncooked |
| Cast: Hood of the Vanguard | ArmorGoldMediumHelmetUncooked |
| Cast: Intricate Key | KeysGoldUncooked |
| Cast: Lightning Strike | StaffThunderbloodUncooked |
| Cast: Nord Atgeir | AtgeirGoldUncooked |
| Cast: Nord Axe | AxeGoldUncooked |
| Cast: Nord Bow | BowGoldUncooked |
| Cast: Nord Buckler | ShieldBucklerGoldUncooked |
| Cast: Nord Crossbow | CrossbowGoldUncooked |
| Cast: Nord Dagger | KnifeGoldUncooked |
| Cast: Nord Greataxe | BattleaxeGoldUncooked |
| Cast: Nord Greatshield | ShieldTowerGoldUncooked |
| Cast: Nord Greatsword | THSwordGoldUncooked |
| Cast: Nord Knucklechains | FistGoldUncooked |
| Cast: Nord Mace | MaceGoldUncooked |
| Cast: Nord Shield | ShieldRoundGoldUncooked |
| Cast: Nord Sledge | SledgeGoldUncooked |
| Cast: Nord Spear | SpearGoldUncooked |
| Cast: Nord Sword | SwordGoldUncooked |
| Cast: Northern Vengeance | StaffFrostOrbsUncooked |
| Cast: Robes of the Caller | ArmorGoldMageChestUncooked |
| Cast: Spirit Caller | StaffSpiritCallerUncooked |
| Cast: Trousers of the Caller | ArmorGoldMageLegsUncooked |
| Cast: Trousers of the Protector | ArmorGoldHeavyLegsUncooked |
| Cast: Trousers of the Vanguard | ArmorGoldMediumLegsUncooked |
| Charred Cogwheel | CharredCogwheel |
| Charred Skull | Charredskull |
| Corked Vial | BlobVial |
| Crown Jewel | CrownJewel |
| Cured Squirrel Hamstring | CuredSquirrelHamstring |
| Curious Axe Head | AxeHead1 |
| Cyan Fireworks | FireworksRocket_Cyan |
| Dandelion | Dandelion |
| Dvergr Extractor | DvergrNeedle |
| Dyrnwyn Blade Fragment | DyrnwynBladeFragment |
| Dyrnwyn Hilt Fragment | DyrnwynHiltFragment |
| Dyrnwyn Tip Fragment | DyrnwynTipFragment |
| Ectoplasm | Ectoplasm |
| Embers | FaderEmber |
| Explosive Payload | BombSiege |
| Fiery Spice Powder | SpiceAshlands |
| Flametal Battle Idol | Upgrader6Weapon |
| Flametal Protection Idol | Upgrader6Armor |
| Flax | Flax |
| Fragrant Bundle | FragrantBundle |
| Fresh Seaweed | FreshSeaweed |
| Grasslands Herbalist Harvest | SpicePlains |
| Green Fireworks | FireworksRocket_Green |
| Hearty Mountain Logger's Stew | FeastMountains_Material |
| Herbs of the Hidden Hills | SpiceMistlands |
| Hook | Hook |
| Iron Battle Idol | Upgrader2Weapon |
| Iron Pit | Ironpit |
| Iron Protection Idol | Upgrader2Armor |
| Linen Thread | LinenThread |
| Mechanical Spring | MechanicalSpring |
| Molten Core | MoltenCore |
| Mould: Breastplate of the Protector | MoldArmorGoldChest |
| Mould: Chestpiece of the Vanguard | MoldArmormediumChest |
| Mould: Echo Spike | MoldStaffOrbofAhri |
| Mould: Headdress of the Caller | MoldArmorMageHelmet |
| Mould: Helmet of the Protector | MoldArmorGoldHelmet |
| Mould: Hood of the Vanguard | MoldArmorMediumHelmet |
| Mould: Intricate Key | MoldKeys |
| Mould: Lightning Strike | MoldStaffthunderblood |
| Mould: Nord Atgeir | MoldAtgeir |
| Mould: Nord Axe | MoldAxe |
| Mould: Nord Bow | MoldBow |
| Mould: Nord Buckler | MoldShieldBuckler |
| Mould: Nord Crossbow | MoldCrossbow |
| Mould: Nord Dagger | MoldKnife |
| Mould: Nord Greataxe | MoldAxe2H |
| Mould: Nord Greatshield | MoldShieldTower |
| Mould: Nord Greatsword | MoldSword2H |
| Mould: Nord Knucklechains | MoldFistweapon |
| Mould: Nord Mace | MoldMace |
| Mould: Nord Shield | MoldShieldRound |
| Mould: Nord Sledge | MoldMace2H |
| Mould: Nord Spear | MoldSpear |
| Mould: Nord Sword | MoldSword |
| Mould: Northern Vengeance | MoldStafffrostorbs |
| Mould: Robes of the Caller | MoldArmorMageChest |
| Mould: Spirit Caller | MoldStaffspiritcaller |
| Mould: Trousers of the Caller | MoldArmorMageLegs |
| Mould: Trousers of the Protector | MoldArmorGoldLegs |
| Mould: Trousers of the Vanguard | MoldArmorMediumLegs |
| Mountain Peak Pepper Powder | SpiceMountains |
| Mushrooms Galore à la Mistlands | FeastMistlands_Material |
| Mysterious Axe Head | AxeHead2 |
| Northern Morning Fare | FeastDeepNorth_Material |
| Plains Pie Picnic | FeastPlains_Material |
| Pot Shard | Pot_Shard_Green |
| Powdered Dragon Eggshells | PowderedDragonEgg |
| Pungent Pebbles | PungentPebbles |
| Purple Fireworks | FireworksRocket_Purple |
| Queen Bee | QueenBee |
| Red Fireworks | FireworksRocket_Red |
| Red Jute | JuteRed |
| Refined Eitr | Eitr |
| Sailor's Bounty | FeastOceans_Material |
| Seafarer's Herbs | SpiceOceans |
| Seasoning of the Gourd | SpiceDeepNorth |
| Shield Core | ShieldCore |
| Silver Battle Idol | Upgrader3Weapon |
| Silver Protection Idol | Upgrader3Armor |
| Soft Tissue | Softtissue |
| Surtling Core | SurtlingCore |
| Swamp Dweller's Delight | FeastSwamps_Material |
| Tar | Tar |
| Thunder Stone | Thunderstone |
| Whole Roasted Meadow Boar | FeastMeadows_Material |
| Wisp | Wisp |
| Wooden Battle Idol | Upgrader0Weapon |
| Wooden Protection Idol | Upgrader0Armor |
| Woodland Herb Blend | SpiceForests |
| Yellow Fireworks | FireworksRocket_Yellow |
| Ymir Flesh | YmirRemains |

### Animal Chest (54 items)

| Item | Prefab name |
|------|-------------|
| Asksvin Bladder | AskBladder |
| Asksvin Hide | AskHide |
| Asksvin Neck | AsksvinCarrionNeck |
| Asksvin Pelvis | AsksvinCarrionPelvic |
| Asksvin Ribcage | AsksvinCarrionRibcage |
| Asksvin Skull | AsksvinCarrionSkull |
| Bear Hide | BjornHide |
| Bear Paw | BjornPaw |
| Bilebag | Bilebag |
| Blood Clot | GiantBloodSack |
| Bloodbag | Bloodbag |
| Bone Fragments | BoneFragments |
| Bonemaw Tooth | BonemawSerpentTooth |
| Carapace | Carapace |
| Celestial Feather | CelestialFeather |
| Charred Bone | CharredBone |
| Chitin | Chitin |
| Dead Pulp | OozeMork |
| Deer Hide | DeerHide |
| Elaking Hair Bundle | ElakingHairBundle |
| Entrails | Entrails |
| Feathers | Feathers |
| Fenris Hair | WolfHairBundle |
| Freeze Gland | FreezeGland |
| Frostcore | FrostCore |
| Frostfire Essence | OrbFrostFire |
| Frozen Branch | BarkaBranch |
| Greydwarf Eye | GreydwarfEye |
| Guck | Guck |
| Hard Antler | HardAntler |
| Leather Scraps | LeatherScraps |
| Leather Straps | Leatherstraps |
| Long Claws | MoleClaws |
| Lox Pelt | LoxPelt |
| Mandible | Mandible |
| Memorial Coal | MemorialCoal |
| Moose Hide | MooseHide |
| Moose Sinew | MooseSinew |
| Morgen Heart | MorgenHeart |
| Morgen Sinew | MorgenSinew |
| Needle | Needle |
| Nornathread | NornThread |
| Ooze | Ooze |
| Proustite Powder | ProustitePowder |
| Scale Hide | ScaleHide |
| Seal Pelt | SealHide |
| Serpent Scale | SerpentScale |
| Thunderblood Essence | OrbThunderBlood |
| Troll Hide | TrollHide |
| Vile Ribcage | UndeadBjornRibcage |
| Withered Bone | WitheredBone |
| Wolf Fang | WolfFang |
| Wolf Pelt | WolfPelt |
| Writhan Roots | WrithanRoots |

### Seed Chest (15 items)

| Item | Prefab name |
|------|-------------|
| Acorns | Acorn |
| Ancient Seed | AncientSeed |
| Beech Seeds | BeechSeeds |
| Birch Seeds | BirchSeeds |
| Carrot Seeds | CarrotSeeds |
| Fir Cone | FirCone |
| Ivy Seeds | VineGreenSeeds |
| Kale Seeds | KaleSeeds |
| Oat Seeds | OatSeeds |
| Onion Seeds | OnionSeeds |
| Pine Cone | PineCone |
| Seed Poteitr | PoteitrSeeds |
| Timberwood Cone | FirConeFrost |
| Turnip Seeds | TurnipSeeds |
| Vineberry Seeds | VineberrySeeds |

### Trophy Chest (70 items)

| Item | Prefab name |
|------|-------------|
| Abomination Trophy | TrophyAbomination |
| Asksvin Trophy | TrophyAsksvin |
| Barka Trophy | TrophyBarka |
| Bear Trophy | TrophyBjorn |
| Blob Trophy | TrophyBlob |
| Boar Trophy | TrophyBoar |
| Bonemass Trophy | TrophyBonemass |
| Bonemaw Trophy | TrophyBonemawSerpent |
| Brenna Trophy | TrophySkeletonHildir |
| Cultist Trophy | TrophyCultist |
| Deathsquito Trophy | TrophyDeathsquito |
| Deer Trophy | TrophyDeer |
| Drake Trophy | TrophyHatchling |
| Draugr Elite Trophy | TrophyDraugrElite |
| Draugr Trophy | TrophyDraugr |
| Dvergr Trophy | TrophyDvergr |
| Eikthyr Trophy | TrophyEikthyr |
| Elaking Trophy | TrophyElaking |
| Eyeless One Trophy | TrophyMole |
| Fader Trophy | TrophyFader |
| Fallen Valkyrie Trophy | TrophyFallenValkyrie |
| Fenring Trophy | TrophyFenring |
| Frost Blob Trophy | TrophyBlob_Frost |
| Fuling Berserker Trophy | TrophyGoblinBrute |
| Fuling Shaman Trophy | TrophyGoblinShaman |
| Fuling Trophy | TrophyGoblin |
| Geirrhafa Trophy | TrophyCultist_Hildir |
| Ghost Trophy | TrophyGhost |
| Gjall Trophy | TrophyGjall |
| Greydwarf Brute Trophy | TrophyGreydwarfBrute |
| Greydwarf Shaman Trophy | TrophyGreydwarfShaman |
| Greydwarf Trophy | TrophyGreydwarf |
| Growth Trophy | TrophyGrowth |
| Hare Trophy | TrophyHare |
| Hexen Trophy | TrophyJotunWitch |
| Krigen Trophy | TrophyJotunWarrior |
| Kvastur | TrophyKvastur |
| Lava Blob Trophy | TrophyBlob_Lava |
| Leech Trophy | TrophyLeech |
| Lox Trophy | TrophyLox |
| Marksman Trophy | TrophyCharredArcher |
| Moder Trophy | TrophyDragonQueen |
| Moose Trophy | TrophyMoose |
| Morgen Trophy | TrophyMorgen |
| Neck Trophy | TrophyNeck |
| Pulp Trophy | TrophyBlob_Morkhalla |
| Rancid Remains Trophy | TrophySkeletonPoison |
| Seal Trophy | TrophySeal |
| Seeker Soldier Trophy | TrophySeekerBrute |
| Seeker Trophy | TrophySeeker |
| Serpent Trophy | TrophySerpent |
| Skeleton Trophy | TrophySkeleton |
| Stone Golem Trophy | TrophySGolem |
| Surtling Trophy | TrophySurtling |
| The Elder Trophy | TrophyTheElder |
| The Queen Trophy | TrophySeekerQueen |
| Thungr Trophy | TrophyGoblinBruteBrosBrute |
| Tick Trophy | TrophyTick |
| Troll Trophy | TrophyFrostTroll |
| Ulv Trophy | TrophyUlv |
| Vile Trophy | TrophyBjornUndead |
| Volture Trophy | TrophyVolture |
| Warlock Trophy | TrophyCharredMage |
| Warrior Trophy | TrophyCharredMelee |
| White Deer Trophy | TrophyDeerWhite |
| Wolf Trophy | TrophyWolf |
| Wraith Trophy | TrophyWraith |
| Writhan Trophy | TrophyWrithan |
| Yagluth Trophy | TrophyGoblinKing |
| Zil Trophy | TrophyGoblinBruteBrosShaman |

### Treasure Chest (32 items)

| Item | Prefab name |
|------|-------------|
| Amber | Amber |
| Amber Pearl | AmberPearl |
| Ancient Coin | AncientCoin |
| Asksvin Egg | AsksvinEgg |
| Barber Kit | BarberKit |
| Barrel Hoops | BarrelRings |
| Bloodstone | GemstoneRed |
| Coins | Coins |
| Dragon Egg | DragonEgg |
| Dragon Tear | DragonTear |
| Draumyx | AncientGemstoneBlack |
| Fader Relic | FaderDrop |
| Fuling Totem | GoblinTotem |
| Grimvarn | AncientGemstoneGreen |
| Hildir's Brass Chest | chest_hildir1 |
| Hildir's Brass Key | HildirKey_forestcrypt |
| Hildir's Bronze Chest | chest_hildir3 |
| Hildir's Bronze Key | HildirKey_plainsfortress |
| Hildir's Silver Chest | chest_hildir2 |
| Hildir's Silver Key | HildirKey_mountaincave |
| Intricate Key | BloodGoldKey |
| Iolite | GemstoneBlue |
| Jade | GemstoneGreen |
| Majestic Carapace | QueenDrop |
| Ruby | Ruby |
| Sacrificial Blood | FrozenKingDrop |
| Scythe Handle | ScytheHandle |
| Silver Necklace | SilverNecklace |
| Solryth | AncientGemstoneOrange |
| Swamp Key | CryptKey |
| Torn Spirit | YagluthDrop |
| Veydris | AncientGemstonePurple |

### Tools Chest (32 items)

| Item | Prefab name |
|------|-------------|
| Antler Pickaxe | PickaxeAntler |
| Asksvin Saddle | SaddleAsksvin |
| Bell | Bell |
| Black Metal Pickaxe | PickaxeBlackMetal |
| Bronze Pickaxe | PickaxeBronze |
| Cold Fishing Bait | FishingBaitCave |
| Cultivator | Cultivator |
| Dvergr Lantern | Lantern |
| Fishing Bait | FishingBait |
| Fishing Rod | FishingRod |
| Frosty Fishing Bait | FishingBaitDeepNorth |
| Hammer | Hammer |
| Heavy Fishing Bait | FishingBaitOcean |
| Hoe | Hoe |
| Hot Fishing Bait | FishingBaitAshlands |
| Iron Pickaxe | PickaxeIron |
| Lox Saddle | SaddleLox |
| Megingjord | BeltStrength |
| Misty Fishing Bait | FishingBaitMistlands |
| Moose Saddle | SaddleMoose |
| Mossy Fishing Bait | FishingBaitForest |
| Salvaged Lantern | Lantern_DN |
| Scythe | Scythe |
| Sealbreaker | DvergrKey |
| Serving Tray | Feaster |
| Snow Shovel | Shovel |
| Sparkler | Sparkler |
| Sticky Fishing Bait | FishingBaitSwamp |
| Stingy Fishing Bait | FishingBaitPlains |
| Torch | Torch |
| Wishbone | Wishbone |
| Wisplight | Demister |

### Armor Chest (124 items)

| Item | Prefab name |
|------|-------------|
| Ashen Cape | CapeAsh |
| Asksvin Cloak | CapeAsksvin |
| Blue Dress with Beads | ArmorDress6 |
| Blue Dress with Shawl | ArmorDress5 |
| Blue Tied Headscarf | HelmetHat1 |
| Blue Tunic with Beads | ArmorTunic3 |
| Blue Tunic with Cape | ArmorTunic2 |
| Bracelets of the Brave | TrinketBlackDamageHealth |
| Breastplate of Ask | ArmorAshlandsMediumChest |
| Breastplate of the Protector | ArmorDeepNorthHeavyChest |
| Brimstone | TrinketFlametalStaminaHealth |
| Bronze Helmet | HelmetBronze |
| Bronze Pendant | TrinketBronzeStamina |
| Bronze Plate Leggings | ArmorBronzeLegs |
| Bronze Plate Tunic | ArmorBronzeChest |
| Brown Dress with Beads | ArmorDress3 |
| Brown Dress with Shawl | ArmorDress2 |
| Brown Fur Cap | HelmetHat3 |
| Cape of the Caller | CapeDeepNorthMage |
| Carapace Breastplate | ArmorCarapaceChest |
| Carapace Greaves | ArmorCarapaceLegs |
| Carapace Helmet | HelmetCarapace |
| Celebratory Cap | HelmetCelebration |
| Chestpiece of the Vanguard | ArmorDeepNorthMediumChest |
| Crown of Roots | HelmetRootCrown |
| Crown of Valheim | HelmetCrownofValheim |
| Crystal Heart | TrinketSilverResist |
| Deer Hide Cape | CapeDeerHide |
| Drake Helmet | HelmetDrake |
| Dverger Circlet | HelmetDverger |
| Eitr-weave Hood | HelmetMage |
| Eitr-weave Robe | ArmorMageChest |
| Eitr-weave Trousers | ArmorMageLegs |
| Evasion Mantle | TrinketBlackStamina |
| Extravagant Green Cap | HelmetHat4 |
| Extravagant Orange Cap | HelmetHat9 |
| Feather Cape | CapeFeather |
| Fenris Coat | ArmorFenringChest |
| Fenris Hood | HelmetFenring |
| Fenris Leggings | ArmorFenringLegs |
| Fins of Destiny | TrinketChitinSwim |
| Fishing Hat | HelmetFishingHat |
| Flametal Breastplate | ArmorFlametalChest |
| Flametal Greaves | ArmorFlametalLegs |
| Flametal Helmet | HelmetFlametal |
| Green Twisted Headscarf | HelmetHat2 |
| Grey Fur Cap | HelmetHat8 |
| Harvest Dress | ArmorHarvester2 |
| Harvest Tunic | ArmorHarvester1 |
| Headband | HelmetSweatBand |
| Headdress of the Bear | HelmetBerserkerHood |
| Headdress of the Caller | HelmetDNMage |
| Heart of the Forest | TrinketBronzeHealth |
| Helmet of the Protector | HelmetDNHeavy |
| Hood of Ask | HelmetAshlandsMediumHood |
| Hood of Embla | HelmetMage_Ashlands |
| Hood of the Vanguard | HelmetDNMediumHood |
| Iron Brooch | TrinketIronHealth |
| Iron Greaves | ArmorIronLegs |
| Iron Helmet | HelmetIron |
| Iron Scale Mail | ArmorIronChest |
| Jörmundling | TrinketFlametalEitr |
| Leather Helmet | HelmetLeather |
| Leather Trousers | ArmorLeatherLegs |
| Leather Tunic | ArmorLeatherChest |
| Linen Cape | CapeLinen |
| Loincloth of the Bear | ArmorBerserkerLegs |
| Lox Cape | CapeLox |
| Lox Fur Hood | HelmetLox |
| Lox Fur Jacket | ArmorLoxChest |
| Lox Fur Trousers | ArmorLoxLegs |
| Moose Hide Cape | CapeDeepNorth |
| Neckstabber | TrinketBloodGoldHealth |
| Nimble Anklet | TrinketIronStamina |
| Padded Cuirass | ArmorPaddedCuirass |
| Padded Greaves | ArmorPaddedGreaves |
| Padded Helmet | HelmetPadded |
| Patterns of the Bear | ArmorBerserkerChest |
| Plain Blue Dress | ArmorDress4 |
| Plain Blue Tunic | ArmorTunic1 |
| Plain Brown Dress | ArmorDress1 |
| Plain Red Tunic | ArmorTunic4 |
| Plain Yellow Dress | ArmorDress7 |
| Plain Yellow Tunic | ArmorTunic7 |
| Pulsating Earrings | TrinketCarapaceEitr |
| Rag Trousers | ArmorRagsLegs |
| Rag Tunic | ArmorRagsChest |
| Red Tunic with Beads | ArmorTunic6 |
| Red Tunic with Cape | ArmorTunic5 |
| Red Twisted Headscarf | HelmetHat7 |
| Resounding Shackle | TrinketScaleStaminaDamage |
| Robes of Embla | ArmorMageChest_Ashlands |
| Robes of the Caller | ArmorDeepNorthMageChest |
| Root Harnesk | ArmorRootChest |
| Root Leggings | ArmorRootLegs |
| Root Mask | HelmetRoot |
| Simple Purple Cap | HelmetHat10 |
| Simple Red Cap | HelmetHat5 |
| Simple Undyed Dress | ArmorDress10 |
| Simple Undyed Tunic | ArmorTunic10 |
| Straw Hat | HelmetStrawHat |
| Troll Hide Cape | CapeTrollHide |
| Troll Leather Hood | HelmetTrollLeather |
| Troll Leather Trousers | ArmorTrollLeatherLegs |
| Troll Leather Tunic | ArmorTrollLeatherChest |
| Trousers of Ask | ArmorAshlandsMediumlegs |
| Trousers of Embla | ArmorMageLegs_Ashlands |
| Trousers of the Caller | ArmorDeepNorthMagelegs |
| Trousers of the Protector | ArmorDeepNorthHeavylegs |
| Trousers of the Vanguard | ArmorDeepNorthMediumlegs |
| Vilebone Cage | ArmorBerserkerUndeadChest |
| Vilebone Drapes | ArmorBerserkerUndeadLegs |
| Vilebone Visage | HelmetBerserkerUndead |
| Witch Crown | TrinketBloodGoldStamina |
| Wolf Fur Cape | CapeWolf |
| Wolf Hide Chestpiece | ArmorWolfChest |
| Wolf Hide Trousers | ArmorWolfLegs |
| Wolf Sight | TrinketSilverDamage |
| Yellow Dress with Beads | ArmorDress9 |
| Yellow Dress with Shawl | ArmorDress8 |
| Yellow Tied Headscarf | HelmetHat6 |
| Yellow Tunic with Beads | ArmorTunic9 |
| Yellow Tunic with Cape | ArmorTunic8 |
| Yule Hat | HelmetYule |

### Weapon Chest (196 items)

| Item | Prefab name |
|------|-------------|
| Abyssal Harpoon | SpearChitin |
| Abyssal Razor | KnifeChitin |
| Ancient Bark Spear | SpearElderbark |
| Arbalest | CrossbowArbalest |
| Ash Fang | BowAshlands |
| Banded Shield | ShieldBanded |
| Basalt Bomb | BombLava |
| Battleaxe | Battleaxe |
| Berserkir Axes | AxeBerzerkr |
| Bile Bomb | BombBile |
| Black Metal Atgeir | AtgeirBlackmetal |
| Black Metal Axe | AxeBlackMetal |
| Black Metal Battleaxe | BattleaxeBlackmetal |
| Black Metal Bolt | BoltBlackmetal |
| Black Metal Knife | KnifeBlackMetal |
| Black Metal Missile | TurretBolt |
| Black Metal Shield | ShieldBlackmetal |
| Black Metal Sword | SwordBlackmetal |
| Black Metal Tower Shield | ShieldBlackmetalTower |
| Bleeding Berserkir Axes | AxeBerzerkrBlood |
| Blob Bomb: Elite Poison | BombBlob_PoisonElite |
| Blob Bomb: Frost | BombBlob_Frost |
| Blob Bomb: Lava | BombBlob_Lava |
| Blob Bomb: Poison | BombBlob_Poison |
| Blob Bomb: Pulp | BombBlob_Morkhalla |
| Blob Bomb: Tar | BombBlob_Tar |
| Blood Fang | BowAshlandsBlood |
| Bloodgeon | MaceEldnerBlood |
| Bloodgold Arrow | ArrowBloodGold |
| Bloodgold Bolt | BoltBloodGold |
| Bloodgold Missile | TurretBoltBloodgold |
| Bone Bolt | BoltBone |
| Bone Tower Shield | ShieldBoneTower |
| Bronze Atgeir | AtgeirBronze |
| Bronze Axe | AxeBronze |
| Bronze Buckler | ShieldBronzeBuckler |
| Bronze Mace | MaceBronze |
| Bronze Spear | SpearBronze |
| Bronze Sword | SwordBronze |
| Bronzehead Arrow | ArrowBronze |
| Brutal Slayer | THSwordSlayerBlood |
| Butcher Knife | KnifeButcher |
| Carapace Arrow | ArrowCarapace |
| Carapace Bolt | BoltCarapace |
| Carapace Buckler | ShieldCarapaceBuckler |
| Carapace Shield | ShieldCarapace |
| Carapace Spear | SpearCarapace |
| Charred Arrow | ArrowCharred |
| Charred Bolt | BoltCharred |
| Club | Club |
| Copper Knife | KnifeCopper |
| Crude Bow | Bow |
| Crystal Battleaxe | BattleaxeCrystal |
| Dead Raiser | StaffSkeleton |
| Demolisher | SledgeDemolisher |
| Draugr Fang | BowDraugrFang |
| Dundr | StaffLightning |
| Dvergr Tankard | Tankard_dvergr |
| Dyrnwyn | SwordDyrnwyn |
| Early Axes | AxeEarly |
| Echo Spike | StaffOrbofAhri |
| Ember Charge | BombDynamite |
| Fang Spear | SpearWolfFang |
| Finewood Bow | BowFineWood |
| Fire Arrow | ArrowFire |
| Flametal Mace | MaceEldner |
| Flametal Missile | TurretBoltFlametal |
| Flametal Shield | ShieldFlametal |
| Flametal Tower Shield | ShieldFlametalTower |
| Flesh Rippers | FistFenrirClaw |
| Flint Axe | AxeFlint |
| Flint Knife | KnifeFlint |
| Flint Spear | SpearFlint |
| Flinthead Arrow | ArrowFlint |
| Frost Arrow | ArrowFrost |
| Frostfire Atgeir | AtgeirGold_FrostFire |
| Frostfire Axe | AxeGold_FrostFire |
| Frostfire Bow | BowGold_FrostFire |
| Frostfire Crossbow | CrossbowGold_FrostFire |
| Frostfire Dagger | KnifeGold_FrostFire |
| Frostfire Greataxe | BattleaxeGold_FrostFire |
| Frostfire Greatsword | THSwordGold_FrostFire |
| Frostfire Knucklechains | FistGold_FrostFire |
| Frostfire Mace | MaceGold_FrostFire |
| Frostfire Sledge | SledgeGold_FrostFire |
| Frostfire Spear | SpearGold_FrostFire |
| Frostfire Sword | SwordGold_FrostFire |
| Frostner | MaceSilver |
| Grappling Hook | GrapplingHook |
| Himminafl | AtgeirHimminAfl |
| Horn of Celebration | TankardAnniversary |
| Huntsman Bow | BowHuntsman |
| Iron Atgeir | AtgeirIron |
| Iron Axe | AxeIron |
| Iron Bolt | BoltIron |
| Iron Buckler | ShieldIronBuckler |
| Iron Mace | MaceIron |
| Iron Sledge | SledgeIron |
| Iron Sword | SwordIron |
| Iron Tower Shield | ShieldIronTower |
| Ironhead Arrow | ArrowIron |
| Jotun Bane | AxeJotunBane |
| Klossen | MaceEldnerNature |
| Krom | THSwordKrom |
| Lightning Strike | StaffThunderBlood |
| Mistwalker | SwordMistwalker |
| Needle Arrow | ArrowNeedle |
| Nidhögg | SwordNiedhogg |
| Nidhögg the Bleeding | SwordNiedhoggBlood |
| Nidhögg the Primal | SwordNiedhoggNature |
| Nidhögg the Thundering | SwordNiedhoggLightning |
| Nord Atgeir | AtgeirGold |
| Nord Axe | AxeGold |
| Nord Bow | BowGold |
| Nord Buckler | ShieldGoldBuckler |
| Nord Crossbow | CrossbowGold |
| Nord Dagger | KnifeGold |
| Nord Greataxe | BattleaxeGold |
| Nord Greatshield | ShieldGoldTower |
| Nord Greatsword | THSwordGold |
| Nord Knucklechains | FistGold |
| Nord Mace | MaceGold |
| Nord Shield | ShieldGold |
| Nord Sledge | SledgeGold |
| Nord Spear | SpearGold |
| Nord Sword | SwordGold |
| Northern Vengeance | StaffFrostOrbs |
| Obsidian Arrow | ArrowObsidian |
| Ooze Bomb | BombOoze |
| Paws of the Bear | FistBjornClaw |
| Poison Arrow | ArrowPoison |
| Porcupine | MaceNeedle |
| Primal Berserkir Axes | AxeBerzerkrNature |
| Primal Slayer | THSwordSlayerNature |
| Ripper | CrossbowRipper |
| Root Fang | BowAshlandsRoot |
| Root Ripper | CrossbowRipperNature |
| Scourging Slayer | THSwordSlayerLightning |
| Serpent Scale Shield | ShieldSerpentscale |
| Shield of Roots | ShieldRoots |
| Silver Arrow | ArrowSilver |
| Silver Knife | KnifeSilver |
| Silver Shield | ShieldSilver |
| Silver Sword | SwordSilver |
| Skoll and Hati | KnifeSkollAndHati |
| Skull Splittur | BattleaxeSkullSplittur |
| Slayer | THSwordSlayer |
| Smoke Bomb | BombSmoke |
| Snowball | Snowball |
| Spinesnap | BowSpineSnap |
| Spirit Caller | StaffSpiritCaller |
| Splitnir | SpearSplitner |
| Splitnir the Bleeding | SpearSplitner_Blood |
| Splitnir the Primal | SpearSplitner_Nature |
| Splitnir the Storming | SpearSplitner_Lightning |
| Staff of Embers | StaffFireball |
| Staff of Fracturing | StaffClusterbomb |
| Staff of Frost | StaffIceShards |
| Staff of Protection | StaffShield |
| Staff of the Wild | StaffGreenRoots |
| Stagbreaker | SledgeStagbreaker |
| Stone Axe | AxeStone |
| Storm Fang | BowAshlandsStorm |
| Storm Ripper | CrossbowRipperLightning |
| Storm Star | MaceEldnerLightning |
| Tankard | Tankard |
| Thunderblood Atgeir | AtgeirGold_BloodLightning |
| Thunderblood Axe | AxeGold_BloodLightning |
| Thunderblood Bow | BowGold_BloodLightning |
| Thunderblood Crossbow | CrossbowGold_BloodLightning |
| Thunderblood Dagger | KnifeGold_BloodLightning |
| Thunderblood Greataxe | BattleaxeGold_BloodLightning |
| Thunderblood Greatsword | THSwordGold_BloodLightning |
| Thunderblood Knucklechains | FistGold_BloodLightning |
| Thunderblood Mace | MaceGold_BloodLightning |
| Thunderblood Sledge | SledgeGold_BloodLightning |
| Thunderblood Spear | SpearGold_BloodLightning |
| Thunderblood Sword | SwordGold_BloodLightning |
| Thundering Berserkir Axes | AxeBerzerkrLightning |
| Trollstav | StaffRedTroll |
| Vilebone Maulclaws | FistBjornUndeadClaw |
| Voidcaller | KnifeVoid |
| Wood Arrow | ArrowWood |
| Wood Shield | ShieldWood |
| Wood Tower Shield | ShieldWoodTower |
| Wooden Atgeir | AtgeirWood |
| Wooden Axe | AxeWood |
| Wooden Battleaxe | BattleaxeWood |
| Wooden Greatsword | THSwordWood |
| Wooden Knife | KnifeWood |
| Wooden Mace | MaceWood |
| Wooden Missile | TurretBoltWood |
| Wooden Sledge | SledgeWood |
| Wooden Spear | SpearWood |
| Wooden Sword | SwordWood |
| Wound Ripper | CrossbowRipperBlood |

### Potion Chest (44 items)

| Item | Prefab name |
|------|-------------|
| Anti-Sting Concoction | MeadBugRepellent |
| Barley Wine Base: Fire Resistance | BarleyWineBase |
| Berserkir Mead | MeadBzerker |
| Brew of Animal Whispers | MeadTamer |
| Bukeperries | Pukeberries |
| Draught of Vananidir | MeadSwimmer |
| Fire Resistance Barley Wine | BarleyWine |
| Frost Resistance Mead | MeadFrostResist |
| Lightfoot Mead | MeadLightfoot |
| Lingering Eitr Mead | MeadEitrLingering |
| Lingering Healing Mead | MeadHealthLingering |
| Lingering Stamina Mead | MeadStaminaLingering |
| Love Potion | MeadTrollPheromones |
| Major Healing Mead | MeadHealthMajor |
| Mead Base: Animal Whispers | MeadBaseTamer |
| Mead Base: Anti-Sting | MeadBaseBugRepellent |
| Mead Base: Berserkir | MeadBaseBzerker |
| Mead Base: Frost Resistance | MeadBaseFrostResist |
| Mead Base: Lightfoot | MeadBaseLightFoot |
| Mead Base: Lingering Eitr | MeadBaseEitrLingering |
| Mead Base: Lingering Health | MeadBaseHealthLingering |
| Mead Base: Lingering Stamina | MeadBaseStaminaLingering |
| Mead Base: Major Healing | MeadBaseHealthMajor |
| Mead Base: Medium Healing | MeadBaseHealthMedium |
| Mead Base: Medium Stamina | MeadBaseStaminaMedium |
| Mead Base: Minor Eitr | MeadBaseEitrMinor |
| Mead Base: Minor Healing | MeadBaseHealthMinor |
| Mead Base: Minor Stamina | MeadBaseStaminaMinor |
| Mead Base: Poison Resistance | MeadBasePoisonResist |
| Mead Base: Ratatosk | MeadBaseHasty |
| Mead Base: Tasty | MeadBaseTasty |
| Mead Base: Troll Endurance | MeadBaseStrength |
| Mead Base: Vananidir | MeadBaseSwimmer |
| Mead of Troll Endurance | MeadStrength |
| Medium Healing Mead | MeadHealthMedium |
| Medium Stamina Mead | MeadStaminaMedium |
| Minor Eitr Mead | MeadEitrMinor |
| Minor Healing Mead | MeadHealthMinor |
| Minor Stamina Mead | MeadStaminaMinor |
| Poison Resistance Mead | MeadPoisonResist |
| Rotten Meat | RottenMeat |
| Tasty Mead | MeadTasty |
| Toadstool | MushroomBzerker |
| Tonic of Ratatosk | MeadHasty |

### Everlasting Chest

Starts empty. Stackable items you put into it are restocked (Full mode), become unlimited once it holds a full stack of them (Linear), or once they are discovered (Discovered).

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

### v0.4.0

- Added: Gathering panel on the player screen, listing the items of the biome you are in with how far each is from being unlimited, the amount in your inventory, and arrows to browse other visited biomes
- Added: The server keeps track of how much every chest holds towards unlocking its items in Linear mode
- Added: Items in a chest that you have not learned yet are marked with a yellow !, and a Learn all button teaches them all at once
- Added: LearnAll and LearnTrophies settings
- Added: Norwegian translation; all texts follow the language chosen in the game
- Changed: The build menu shows the chest names instead of their prefab names

### v0.3.0

- Added: Items put into a chest that already holds them without limit disappear instead of forming new stacks, which kept multiplying the unlimited stacks
- Added: The icon on the front of a chest turns grey when the chest is empty, with bars under it for the used slots and the unlimited items
- Added: Looking at a chest shows its contents as item icons below the crosshair
- Added: ShowIndicators and ShowHoverPanel settings to switch these off
- Added: SortContents setting - chests keep their contents sorted from the top left, unlimited items first
- Added: Chests keep more than one full stack of an item when a single recipe or build piece needs more than one stack
- Changed: Chests fill from the top instead of from the bottom row
- Changed: Chests refill as soon as their contents change instead of up to a second later, so building several pieces in a row from a chest works
- Changed: Surplus unlimited stacks are removed and partial stacks of the same item are combined
- Changed: "Take all" only takes the items players stored themselves
- Fixed: In Full mode, destroying a chest or switching mode deleted weapons and armor that players had upgraded or crafted and stored in the chest
- Changed: About 40 items moved to a more fitting chest: Coal to Wood; Obsidian, Crystal and Sulfur to Stone; Chain to Metal; Feathers, Leather Scraps, Withered Bone, Hard Antler and Fenris Hair to Animal; Surtling Core, Ectoplasm, Flax, Tar and all moulds to Material; gemstones and boss drops to Treasure
- Fixed: Fishing rods and fishing bait were placed in the Weapon Chest instead of the Tools Chest
- Added: The README lists every item in every chest, with the prefab name to use in the Include and Exclude settings

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
