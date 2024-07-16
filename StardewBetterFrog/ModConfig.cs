using StardewModdingAPI;

namespace StardewBetterFrog;

public sealed class ModConfig
{
    public bool CountAsPlayerKill { get; set; }
    
    public void RegisterConfig(IManifest manifest, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Count as player kill",
            tooltip: () => "If checked, monsters a frog swallows will count as a player kill, which means there will be loot drops, quest and bounty progression.",
            getValue: () => CountAsPlayerKill,
            setValue: value => CountAsPlayerKill = value
        );
    }
}