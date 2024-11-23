using HarmonyLib;
using StardewBetterFrog.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Companions;
using StardewValley.Objects.Trinkets;

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
        harmony.Patch(
            original: AccessTools.Method(typeof(HungryFrogCompanion), nameof(HungryFrogCompanion.tongueReachedMonster)),
            prefix: new(typeof(HungryFrogCompanionPatches), nameof(HungryFrogCompanionPatches.TongueReachedMonster_CallOnMonsterEaten_Prefix))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(HungryFrogCompanion), nameof(HungryFrogCompanion.Update)),
            transpiler: new(typeof(HungryFrogCompanionPatches), nameof(HungryFrogCompanionPatches.Update_UseCustomTargetFunction_Transpiler))
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