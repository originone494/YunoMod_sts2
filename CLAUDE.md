# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

YunoMod is a **Slay the Spire 2** character mod that adds "我妻由乃" (Yuno Gasai) as a playable character. Built with **Godot 4.5.1 Mono** (.NET 9.0, C# 12, nullable enabled). The modding framework is **STS2-RitsuLib**, which provides scaffolding templates, auto-registration, and Godot interop.

## Build & Run

```bash
# Build DLL + export PCK to STS2 mods folder
dotnet build
# or open YunoMod.sln in VS / Rider

# The .csproj post-build step copies the DLL to:
# D:\steam\steamapps\common\Slay the Spire 2\mods\YunoMod\

# PCK export runs after build (ExportDebug/ExportRelease configs):
# Uses Godot executable defined in .csproj <GodotExe> property
```

Build configurations: `Debug`, `ExportDebug`, `ExportRelease`.

## Architecture

```
Scripts/
  Entry.cs                  # Mod entry point [ModInitializer]
  Base/                     # Abstract base classes for all content
    YunoBaseCard.cs         #   extends ModCardTemplate
    YunoBaseRelic.cs        #   extends ModRelicTemplate
    YunoBasePower.cs        #   extends ModPowerTemplate
  Cards/Attack/             # Attack cards
  Cards/Skill/              # Skill cards
  Cards/Power/              # Power cards
  Power/Common/             # Simple powers (Buff/Debuff definitions)
  Power/PowerCard/          # Powers applied by Power-type cards
  Relics/                   # Relic implementations
  Character/                # YunoCharacter definition + Godot scene proxies
  Custom/                   # YunoTags, YunoKeywords (custom card tags/keywords)
  Enchantment/              # Custom card enchantments
  Hook/                     # Game event hooks
  Pool/                     # Card/relic/potion pool definitions
```

## Key Patterns

### Auto-registration via RitsuLib attributes

All content types use attributes to self-register — no manual wiring needed:

- `[RegisterCard(typeof(YunoCardPool), Inherit = true)]` on `YunoBaseCard` means all subclasses auto-register
- `[RegisterRelic(typeof(YunoRelicPool), Inherit = true)]` on `YunoBaseRelic`
- `[RegisterPower(Inherit = true)]` on `YunoBasePower`
- `[RegisterEnchantment]` on enchantment classes
- `[RegisterCharacter]` on `YunoCharacter`
- Custom keywords/tags use `[RegisterOwnedCardKeyword(...)]` / `[RegisterOwnedCardTag(...)]` on the `YunoKeywords`/`YunoTags` classes

### Base classes to extend

| Content type | Extend this base class |
|---|---|
| Card | `YunoBaseCard` |
| Power | `YunoBasePower` |
| Relic | `YunoBaseRelic` |
| Enchantment | `ModEnchantmentTemplate` |

### Asset paths convention

Card portraits: `res://YunoMod/images/cards/{ClassName}.png`
Power icons: `res://YunoMod/images/powers/{ClassName}.png`
Relic icons: `res://YunoMod/images/relics/{ClassName}.png`
Godot scenes: `res://YunoMod/scenes/*.tscn`

### Card implementation

Cards override `CanonicalVars` (DynamicVar definitions), `OnPlay` (effect logic), and `OnUpgrade` (upgrade changes). They inherit `CardAssetProfile` from `YunoBaseCard` which maps portrait paths automatically from the class name.

### DynamicVar system

```csharp
new DamageVar(6, ValueProp.Move)    // damage that scales with Strength
new BlockVar(5, ValueProp.Move)     // block that scales with Dexterity
new PowerVar<StrengthPower>(2)      // power application amount
new RepeatVar(3)                    // hit count for multi-hit attacks
new DynamicVar("KeyName", 1m)       // custom named variable
```

### Custom keywords & tags

Defined in `YunoKeywords.cs` and `YunoTags.cs`. Use `ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(...))` for the ID string. Check with `.HasModKeyword(YunoKeywords.Xxx)` or filter cards with `card.Tags.Contains(CardTag.Xxx)`.

### Mod entry point

`Scripts/Entry.cs` — `[ModInitializer]` attribute, calls `RitsuLibFramework.EnsureGodotScriptsRegistered()` then `ModTypeDiscoveryHub.RegisterModAssembly()`. Mod ID is `"YunoMod"`.

## API Reference

The project includes detailed API docs in `接口文档-card.md`, `接口文档-power.md`, and `接口文档-relic.md`. These document the STS2 core API (CardModel, PowerModel, RelicModel base classes, commands like `DamageCmd`, `PowerCmd`, `CreatureCmd`, `PlayerCmd`, `CardPileCmd`, lifecycle hooks, DynamicVar types, etc.). Reference these when implementing new content.

The simpler cheat sheet `接口.md` and `hook.md` contain commonly used code snippets.