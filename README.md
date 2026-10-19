# Lightweight Inventory 0.2 — ExtraSlots weight subsystem port

Ported from `ExtraSlots/InventoryInteraction.cs` and `LightenedSlots.cs` in the supplied ExtraSlots master source. The significant difference from prototype 0.1 is applying the weight multiplier **inside** `Inventory.UpdateTotalWeight()` via the `ItemDrop.ItemData.GetWeight()` postfix, rather than changing the result of `Inventory.GetTotalWeight()`.

This plugin does **not** add inventory rows or equipment slots, alter serialization, require a server mod, or draw feather icons. The latter is a separate UI task, and SeneaL replaces the ordinary inventory UI, so working weight behavior should be confirmed first.

## Build (from PowerShell in this directory)

```powershell
dotnet build -c Release -p:ValheimManaged="C:\Steam\steamapps\common\Valheim\valheim_Data\Managed" -p:BepInExCore="C:\Users\savag\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\UI Test\BepInEx\core"
```

On success, copy `bin\Release\netstandard2.1\LightweightInventory.dll` to your UI Test profile's `BepInEx\plugins\LightweightInventory\`. Overwrite the previous DLL; do not install both versions. Leave ExtraSlots disabled in UI Test. This source is not compiled or game-tested in the current environment.

## Configuration

- General / Enabled = true
- Lightweight Rows / FirstRowIndex = -2
- Lightweight Rows / WeightMultiplier = 0.5

Changing a value refreshes the current player's inventory weight. Items moving between inventory cells should trigger vanilla inventory weight updates.

## Testing

Use a normal unmodded item stack with known weight. Move it from an unaffected row into the bottom two inventory rows, then back again. Confirm the displayed weight and encumbrance. If it still doesn't work, collect the BepInEx log and note whether the displayed weight changes after closing/reopening inventory or moving a second item.


## v0.2.1 build fix

Valheim 1.0.20 declares `Inventory.UpdateTotalWeight()` and `m_temoraryInventory` non-public. The previous project incorrectly accessed them from C#. The Harmony patch targets the method by string name, and configuration changes invoke the method through reflection. The temporary-inventory check was redundant because the patch already checks equality with the local player's inventory. This revision is not game-tested.
