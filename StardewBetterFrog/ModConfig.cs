using StardewModdingAPI;

namespace StardewBetterFrog;

public enum BlacklistType
{
    SameMonster,
    SameType,
    Everything,
    None
}

public enum BlacklistDuration
{
    Time,
    UntilLeave,
    Permanent,
}

public sealed class ModConfig
{
    public bool AllowSpittingMonster { get; set; } = true;

    public float FrogInteractDistance { get; set; } = 60f;
    
    public bool CountAsPlayerKill { get; set; }
    
    private static readonly string[] BlacklistTypeLabels = { "Same Monster", "Same Type", "Everything", "None" };
    public BlacklistType BlacklistType { get; set; } = BlacklistType.SameType;

    private static readonly string[] BlacklistDurationLabels = { "Time", "UntilLeave", "Permanent" };
    public BlacklistDuration BlacklistDuration { get; set; } = BlacklistDuration.UntilLeave;
    public float BlacklistDurationSeconds { get; set; } = 5f;
    
    
    public void RegisterConfig(IManifest manifest, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Count as player kill",
            tooltip: () => "If checked, monsters a frog swallows will count as a player kill, which means there will be loot drops, quest and bounty progression.",
            getValue: () => CountAsPlayerKill,
            setValue: value => CountAsPlayerKill = value
        );
        
        configMenu.AddSectionTitle(manifest, () => "Spitting Monsters");
        
        configMenu.AddBoolOption(
            mod: manifest,
            name: () => "Allow spitting monsters",
            tooltip: () => "If allowed, right clicking your frog when it has a monster in its mouth will make it spit up the monster.",
            getValue: () => AllowSpittingMonster,
            setValue: value => AllowSpittingMonster = value
        );
        
        configMenu.AddNumberOption(
            mod: manifest,
            name: () => "Frog interaction distance",
            tooltip: () => "The maximum distance from the frog in which a right click will count as a request to spit up what it is eating.",
            getValue: () => FrogInteractDistance,
            setValue: value => FrogInteractDistance = value
        );
        
        configMenu.AddParagraph(manifest, () => 
            "When a frog swallows a monster and the players makes it spit the monster out, a blacklist is generated. " +
            "By default, the frog would just try to swallow the same monster you just told it to spit out, which can be annoying! " +
            "Here, you can configure how the frog will behave when selecting its next target.\n" +
            "Hover over the label to see what each option means."
        );
        
        configMenu.AddTextOption(
            mod: manifest,
            name: () => "Blacklist Mode",
            tooltip: () => "How the frog will filter monsters after being told to spit one out.\n"
                           + $"{BlacklistTypeLabels[(int)BlacklistType.SameMonster]} - Won't swallow the same monster it just spat out.\n"
                           + $"{BlacklistTypeLabels[(int)BlacklistType.SameType]} - Won't swallow a monster of the same type it just spat out.\n"
                           + $"{BlacklistTypeLabels[(int)BlacklistType.Everything]} - Won't swallow any monster as long as the filter is active.\n"
                           + $"{BlacklistTypeLabels[(int)BlacklistType.None]} - Will still swallow any monsters including the one it just spat out.",
            getValue: () => BlacklistTypeLabels[(int)BlacklistType],
            setValue: value => BlacklistType = (BlacklistType)BlacklistTypeLabels.IndexOf(value),
            allowedValues: BlacklistTypeLabels
        );

        configMenu.AddTextOption(
            mod: manifest,
            name: () => "Blacklist Duration Mode",
            tooltip: () => "How long the selected filter will last for.\n"
                           + $"{BlacklistDurationLabels[(int)BlacklistDuration.Time]} - Filter expires after some seconds.\n"
                           + $"{BlacklistDurationLabels[(int)BlacklistDuration.UntilLeave]} - Filter expires after player leaves the level.\n"
                           + $"{BlacklistDurationLabels[(int)BlacklistDuration.Permanent]} - Filter lasts as long as the frog exists.\n",
            getValue: () => BlacklistDurationLabels[(int)BlacklistDuration],
            setValue: value => BlacklistDuration = (BlacklistDuration)BlacklistDurationLabels.IndexOf(value),
            allowedValues: BlacklistDurationLabels
        );

        configMenu.AddNumberOption(
            mod: manifest,
            name: () => "Blacklist Duration Seconds",
            tooltip: () => $"How many seconds the filter will last for if the duration mode above was set to {BlacklistDurationLabels[(int)BlacklistDuration.Time]}.",
            getValue: () => BlacklistDurationSeconds,
            setValue: value => BlacklistDurationSeconds = value
        );
    }
}