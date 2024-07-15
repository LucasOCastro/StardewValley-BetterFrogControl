using StardewModdingAPI;

namespace StardewBetterFrog;

public sealed class ModConfig
{
    public bool CountForBounty { get; set; }
    
    public void RegisterConfig(IManifest manifest, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Count for guild bounty",
            tooltip: () => "If checked, monsters a frog swallows will count towards the adventure guild bounty board.",
            getValue: () => CountForBounty,
            setValue: value => CountForBounty = value
        );
    }
}