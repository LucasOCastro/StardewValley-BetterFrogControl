using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Objects;

namespace StardewBetterFrog;

public sealed class ModEntry : Mod
{
    private ModConfig _config = new();
    
    public override void Entry(IModHelper helper)
    {
        BetterFrogCompanion.Monitor = Monitor;
        
        //Setup config
        _config = helper.ReadConfig<ModConfig>();
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
            reset: () => _config = new(),
            save: () => Helper.WriteConfig(_config)
        );
        _config.RegisterConfig(ModManifest, configMenu);
    }
}