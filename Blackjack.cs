using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

using Blackjack.Config;
using Blackjack.Commands;
using Blackjack.Utils;

namespace Blackjack;

public class Blackjack : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "Blackjack";
    public override string ModuleVersion => "1.0.2";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Blackjack minigame for CS2";

    public required BaseConfigs Config { get; set; }

    private BlackjackCommands? blackjackCommands;

    public override void Load(bool hotReload)
    {
        InitializeCommands();
        RegisterCommands();
        RegisterListener<Listeners.OnTick>(OnTick);

        Debug.Config = Config;

        if (Config.EnableDebug)
        {
            Debug.DebugMessage($"Blackjack loaded - Version: {ModuleVersion}");
            Debug.DebugMessage($"Inactivity Timeout: {Config.InactivityTimeout}s");
            Debug.DebugMessage($"Game Cooldown: {Config.GameCooldown}s");
        }
    }

    private void InitializeCommands()
    {
        blackjackCommands = new BlackjackCommands(Config, Localizer);
    }

    private void RegisterCommands()
    {
        AddCommand("css_bj", "Start a Blackjack game", blackjackCommands!.StartGameCommand);
        AddCommand("css_blackjack", "Start a Blackjack game", blackjackCommands!.StartGameCommand);
        AddCommand("css_hit", "Draw another card", blackjackCommands!.HitCommand);
        AddCommand("css_stand", "Stand with current hand", blackjackCommands!.StandCommand);
    }

    public void OnConfigParsed(BaseConfigs config)
    {
        Config = config;
        Debug.Config = config;

        if (blackjackCommands != null)
        {
            blackjackCommands.UpdateConfig(config);
        }

        if (config.EnableDebug)
        {
            Debug.DebugMessage("Configuration reloaded");
        }
    }

    public void OnTick()
    {
        blackjackCommands?.UpdateDisplays();
    }

    public override void Unload(bool hotReload)
    {
        if (Config.EnableDebug)
        {
            Debug.DebugMessage("Blackjack plugin unloading...");
        }

        blackjackCommands?.Cleanup();
        blackjackCommands = null;
    }
}