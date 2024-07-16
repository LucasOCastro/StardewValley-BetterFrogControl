using StardewModdingAPI;

namespace StardewBetterFrog;

public sealed class ModConfig
{
    public bool AllowSpittingMonster { get; set; } = true;

    public float FrogInteractDistance { get; set; } = 60f;
    
    public bool CountAsPlayerKill { get; set; }
    
    public void RegisterConfig(IManifest manifest, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Allow spitting monsters",
            tooltip: () => "If allowed, right clicking your frog when it has a monster in its mouth will make it spit up the monster.",
            getValue: () => AllowSpittingMonster,
            setValue: value => AllowSpittingMonster = value
        );

        if (AllowSpittingMonster)
            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Frog interaction distance",
                tooltip: () => "The maximum distance from the frog in which a right click will count as a request to spit up what it is eating.",
                getValue: () => FrogInteractDistance,
                setValue: value => FrogInteractDistance = value
            );
        
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Count as player kill",
            tooltip: () => "If checked, monsters a frog swallows will count as a player kill, which means there will be loot drops, quest and bounty progression.",
            getValue: () => CountAsPlayerKill,
            setValue: value => CountAsPlayerKill = value
        );
    }
}