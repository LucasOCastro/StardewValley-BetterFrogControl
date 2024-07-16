using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Objects;

namespace StardewBetterFrog;

public sealed class ModEntry : Mod
{
    public static ModConfig ConfigSingleton { get; private set; } = new();
    public static IMonitor? MonitorSingleton { get; private set; }

    public override void Entry(IModHelper helper)
    {
        //Setup config
        MonitorSingleton = Monitor;
        ConfigSingleton = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += SetupConfigMenu;
        
        //Setup Harmony
        Harmony harmony = new(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(CompanionTrinketEffect), nameof(CompanionTrinketEffect.Apply)),
            transpiler: new(typeof(CompanionTrinketEffectPatches), nameof(CompanionTrinketEffectPatches.Apply_UseBetterFrogConstructor_Transpiler))
        );
    }

    private void SetupConfigMenu(object? _, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu == null) return;
        
        configMenu.Register(
            mod: ModManifest,
            reset: () => ConfigSingleton = new(),
            save: () => Helper.WriteConfig(ConfigSingleton)
        );
        ConfigSingleton.RegisterConfig(ModManifest, configMenu);
    }
}