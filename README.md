# Lightweight Inventory

A lightweight, client-side Valheim mod that reduces the weight of items stored in configurable inventory rows.

Keep your heaviest materials in designated lightweight slots to carry more without changing your character's maximum carry capacity.

## Features

- **Configurable lightweight rows** — Choose how many inventory rows receive reduced item weight.
- **Adjustable weight reduction** — Set the weight multiplier from 0.0 to 1.0 (50% by default).
- **Feather indicators** — Lightweight inventory slots display a feather icon when empty.
- **Dynamic inventory support** — Lightweight rows adjust when your inventory expands.
- **SeneaL UI compatibility** — Supports SeneaL's expanded inventory and separate equipment storage.
- **Client-side functionality** — Does not require installation on a dedicated server.
- **No additional equipment slots** — Focuses exclusively on inventory weight reduction.

## Configuration

| Setting | Default | Description |
|---|---|---|
| Enabled | `true` | Enables or disables weight reduction |
| FirstRowIndex | `-2` | Negative values select the bottom N inventory rows; positive values select a starting row |
| WeightMultiplier | `0.5` | Multiplier applied to item weight in lightweight rows |

**Example:** With `FirstRowIndex = -2` and `WeightMultiplier = 0.5`, items in the bottom two inventory rows contribute half their normal weight to your carried total.

## Installation

1. Install BepInExPack for Valheim.
2. Place `LightweightInventory.dll` and `lightenedslot.png` together in `BepInEx/plugins/LightweightInventory/`.
3. Launch Valheim.

Configuration options are available through the generated BepInEx configuration file or a compatible in-game configuration manager.

### Compatibility

- Tested with Valheim 1.0.20.
- Tested with SeneaL UI 1.2.3 and expanded inventory rows.
- Designed for client-side use; no server installation required.
- Incompatible with ExtraSlots to avoid overlapping lightweight-inventory functionality.

SeneaL compatibility currently accounts for five internal inventory rows reserved by SeneaL. Future changes to SeneaL's storage implementation may require a compatibility update.

## Known Issues

Changing lightweight-row settings during gameplay may not immediately update the displayed carry weight. Moving an item within the inventory refreshes the display.

## Building from Source

Requires the .NET SDK and Valheim/BepInEx assemblies.

1. Create `Local.Build.props` in the project directory using the example configuration.
2. Set `ValheimManaged` to Valheim's managed assemblies directory.
3. Set `BepInExCore` to your BepInEx core directory.
4. Build the project in Release configuration using Visual Studio or `dotnet build -c Release`.

The compiled DLL is written to `bin/Release/netstandard2.1/`.

## Credits

Inspired by the lightweight inventory functionality of [ExtraSlots by shudnal](https://github.com/shudnal/ExtraSlots).

Built using BepInEx and Harmony.

## Version

**1.0.2 — Fixed lightweight weight initialization when logging into a character**