using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewBetterFrog;

public sealed class ModEntry : Mod
{
    private ModConfig _config = new();
    
    public override void Entry(IModHelper helper)
    {
        //Setup config
        _config = helper.ReadConfig<ModConfig>();
        helper.Events.GameLoop.GameLaunched += SetupConfigMenu;
    }

    private void SetupConfigMenu(object? _, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu == null) return;
        
        configMenu.Register(
            mod: ModManifest,
            reset: () => _config = new ModConfig(),
            save: () => Helper.WriteConfig(_config)
        );
        _config.RegisterConfig(ModManifest, configMenu);
    }
}