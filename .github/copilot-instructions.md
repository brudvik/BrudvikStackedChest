# Copilot Instructions for BrudvikStackedChest

This document provides guidelines for GitHub Copilot when working on the BrudvikStackedChest Valheim mod project.

## Project Overview

This is a Valheim mod built with:
- **BepInEx** - Plugin framework
- **Jötunn (JVL)** - Valheim modding library
- **Harmony** - Runtime patching
- **C# / .NET Framework 4.6.2**

---

## Code Style & Documentation

### XML Documentation Comments

All public classes, methods, and properties MUST have XML documentation comments:

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <param name="paramName">Description of the parameter.</param>
/// <returns>Description of the return value.</returns>
public ReturnType MethodName(ParamType paramName)
```

### Inline Comments

- Use inline comments sparingly, only when the code is not self-explanatory
- Explain **why** something is done, not **what** is being done
- Use `//` for single-line comments, placed above the code line

### Naming Conventions

- **Classes**: PascalCase (e.g., `CustomChestManager`)
- **Methods**: PascalCase (e.g., `AddCustomChest`)
- **Private fields**: camelCase with no prefix (e.g., `pieceManager`)
- **Constants**: PascalCase (e.g., `PluginGUID`)
- **Properties**: PascalCase (e.g., `SpawnItems`)

---

## Version Management

### Version Locations

The version number is stored in **THREE** locations that must ALL be updated together:

1. **BrudvikStackedChest.cs** - Line ~29:
   ```csharp
   public const string PluginVersion = "X.Y.Z";
   ```

2. **AssemblyInfo.cs** - Lines ~34-35:
   ```csharp
   [assembly: AssemblyVersion("X.Y.Z.0")]
   [assembly: AssemblyFileVersion("X.Y.Z.0")]
   ```

3. **Package/manifest.json** - Line ~4:
   ```json
   "version_number": "X.Y.Z",
   ```

### Version Increment Rules

Use semantic versioning (MAJOR.MINOR.PATCH):

| Change Type | Increment | Example |
|-------------|-----------|---------|
| Breaking changes | MAJOR | 0.0.6 → 1.0.0 |
| New features, new chests | MINOR | 0.0.6 → 0.1.0 |
| Bug fixes, small tweaks | PATCH | 0.0.6 → 0.0.7 |

### When to Increment Version

Increment the version when:
- Adding a new chest type
- Fixing a bug
- Adding new items to existing chests
- Changing behavior of existing functionality
- Any change that affects the compiled DLL

---

## README.md Updates

### When to Update README.md

Update the README.md file when:
- Adding new chests (add to "Available Chests" section)
- Changing chest contents (update the items list)
- Adding new features (add to appropriate section)
- Fixing bugs (add to changelog)
- Changing build requirements

### Changelog Format

Always add changelog entries at the TOP of the Changelog section, using this format:

```markdown
### vX.Y.Z

- Added: New feature or chest
- Changed: Modified behavior
- Fixed: Bug that was fixed
- Removed: Feature that was removed
```

### Changelog Entry Guidelines

- Start each line with a verb: Added, Changed, Fixed, Removed, Updated
- Be specific but concise
- Reference the chest name or feature affected
- Group related changes together

---

## Adding New Chests

When adding a new chest, follow this checklist:

### 1. Code Changes (BrudvikStackedChest.cs)

Add the new chest in the `AddClonedItems()` method:

```csharp
customPieces.Add(customChestManager.AddCustomChest(
    new CustomChestModel()
    {
        Name = "BSNewChest",           // Must start with "BS"
        DisplayName = "New Chest",      // Shown in-game
        Description = "A chest filled with...",
        Icon = "strg_XXX_round.png",    // Icon from Assets
        Color = SharedUtils.ColorFromRGB(R, G, B, 0.8f),
        SpawnItems = new List<string>
        {
            "ItemPrefabName1",
            "ItemPrefabName2"
        }
    }
));
```

### 2. Update README.md

Add a new section under "Available Chests":

```markdown
### New Chest
*A chest filled with description*

| Color | Items |
|-------|-------|
| Color Name | Item1, Item2, Item3 |
```

### 3. Update Version

Increment MINOR version (new feature).

### 4. Update Changelog

Add entry describing the new chest.

---

## Harmony Patching Guidelines

### Postfix Patches

Use for actions that should happen AFTER the original method:
- Refilling items after changes are detected
- Logging or analytics

### Prefix Patches

Use for actions that should happen BEFORE the original method:
- Preventing default behavior
- Modifying input parameters
- Emptying containers before drop

### Event Pattern

Always use events to decouple patches from business logic:

```csharp
// In patch class
public static event EventHandler<CustomEventArgs>? SomePatched;

// Trigger event
SomePatched?.Invoke(null, new CustomEventArgs { ... });
```

---

## File Organization

```
BrudvikStackedChest/
├── BrudvikStackedChest.cs    # Main plugin class
├── Constants/                 # Static values
├── Events/                    # Event argument classes
├── Extensions/                # Extension methods
├── Helpers/                   # Utility classes
├── Models/                    # Data models
├── Patches/                   # Harmony patches
├── Piece/                     # Custom piece classes
├── Properties/                # Assembly info
└── Utils/                     # Shared utilities
```

---

## Pre-Release Checklist

Before releasing a new version:

- [ ] All three version locations updated
- [ ] README.md changelog updated
- [ ] All new public members have XML documentation
- [ ] Code compiles without warnings
- [ ] Tested in-game (if possible)
- [ ] Package/manifest.json dependencies are current

---

## Common Prefab Names Reference

When adding items to chests, use the correct prefab names. Common examples:

| Category | Examples |
|----------|----------|
| Wood | Wood, FineWood, RoundLog, Blackwood, YggdrasilWood |
| Stone | Stone, Flint, Obsidian, BlackMarble, Grausten |
| Metal | Iron, Bronze, Silver, BlackMetal, Flametal, Copper, Tin |
| Food | Honey, Bread, Sausages, Carrot, Turnip, Onion |
| Animal | DeerHide, LeatherScraps, TrollHide, WolfPelt, LoxPelt |

---

## Error Handling

- Always null-check container and inventory references
- Use `?.` null-conditional operators where appropriate
- Log errors using `Jotunn.Logger.LogError()`
- Log info using `Jotunn.Logger.LogInfo()`
