using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Microsoft.Extensions.Localization;

using Blackjack.Config;
using Blackjack.Utils;
using Blackjack.Managers;

namespace Blackjack.Commands;

public class BlackjackCommands
{
    private BaseConfigs config;
    private readonly IStringLocalizer localizer;
    private readonly GameManager gameManager;
    private readonly Dictionary<ulong, Timer> endGameTimers = new();

    public BlackjackCommands(BaseConfigs config, IStringLocalizer localizer)
    {
        this.config = config;
        this.localizer = localizer;
        gameManager = new GameManager(config);

        gameManager.SetInactivityTimeoutCallback(HandleInactivityTimeout);
    }

    public void UpdateConfig(BaseConfigs newConfig)
    {
        config = newConfig;
        gameManager.UpdateConfig(newConfig);
    }

    public void UpdateDisplays()
    {
        gameManager.UpdateActiveGames(localizer);
        gameManager.UpdateEndGameStates();
    }

    public void StartGameCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!CommandUtils.ValidateCommand(player, command, config, localizer))
        {
            return;
        }

        ulong steamId = player!.SteamID;

        if (gameManager.HasActiveGame(steamId))
        {
            PlayerUtils.Reply(player, localizer, "Already Playing A Game");
            return;
        }

        if (gameManager.IsOnCooldown(steamId))
        {
            int remainingCooldown = gameManager.GetRemainingCooldown(steamId);
            player.PrintToChat($"{localizer["prefix"]} {localizer["Cooldown Active"].Value.Replace("{seconds}", remainingCooldown.ToString())}");
            return;
        }

        gameManager.StartNewGame(steamId);
        PlayerUtils.Reply(player, localizer, "Starting A Game");
    }

    public void HitCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!CommandUtils.ValidateCommand(player, command, config, localizer))
        {
            return;
        }

        ulong steamId = player!.SteamID;

        if (!gameManager.HasActiveGame(steamId))
        {
            PlayerUtils.Reply(player, localizer, "No Active Game");
            return;
        }

        var hitResult = gameManager.PlayerHit(steamId);
        PlayerUtils.Reply(player, localizer, "Hit");

        if (hitResult.IsBust)
        {
            ShowEndGame(player, steamId, localizer["Bust"], "#FF0000", localizer["More Than 21"]);
            if (config.AnnounceResults)
            {
                AnnounceBustResult(player, hitResult.Total);
            }
        }
    }

    public void StandCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!CommandUtils.ValidateCommand(player, command, config, localizer))
        {
            return;
        }

        ulong steamId = player!.SteamID;

        if (!gameManager.HasActiveGame(steamId))
        {
            PlayerUtils.Reply(player, localizer, "No Active Game");
            return;
        }

        var standResult = gameManager.PlayerStand(steamId);
        PlayerUtils.Reply(player, localizer, "Stand");

        string title, color, message;

        switch (standResult.Result)
        {
            case GameResult.Win:
                PlayerUtils.Reply(player, localizer, "Win");
                title = localizer["WinTitle"];
                color = "#00FF00";
                message = localizer["Prize"];
                break;

            case GameResult.Draw:
                PlayerUtils.Reply(player, localizer, "Draw");
                title = localizer["DrawTitle"];
                color = "#FFA500";
                message = localizer["TieMessage"];
                break;

            default:
                PlayerUtils.Reply(player, localizer, "Lose");
                title = localizer["LoseTitle"];
                color = "#FF0000";
                message = localizer["DealerWon"];
                break;
        }

        ShowEndGame(player, steamId, title, color, message, standResult.DealerTotal, standResult.PlayerTotal);

        if (config.AnnounceResults)
        {
            AnnounceGameResult(player, standResult.Result, standResult.PlayerTotal, standResult.DealerTotal);
        }
    }

    private void HandleInactivityTimeout(ulong steamId)
    {
        var player = Utilities.GetPlayerFromSteamId(steamId);
        if (player?.IsValid == true)
        {
            PlayerUtils.Reply(player, localizer, "Inactivity Timeout");

            var game = gameManager.GetGameState(steamId);
            if (game != null)
            {
                int playerTotal = HandCalculator.CalculateHand(game.PlayerHand);
                int dealerTotal = HandCalculator.CalculateHand(game.DealerHand);

                ShowEndGame(player, steamId, localizer["LoseTitle"], "#FF0000", localizer["Inactivity Loss"], dealerTotal, playerTotal);

                if (config.AnnounceResults)
                {
                    AnnounceGameResult(player, GameResult.Lose, playerTotal, dealerTotal, true);
                }
            }
        }
    }

    private void ShowEndGame(CCSPlayerController player, ulong steamId, string title, string color, string message, int dealerTotal = -1, int playerTotal = -1)
    {
        var game = gameManager.GetGameState(steamId);
        if (game == null) return;

        int finalPlayerTotal = playerTotal == -1 ? HandCalculator.CalculateHand(game.PlayerHand) : playerTotal;
        int finalDealerTotal = dealerTotal == -1 ? HandCalculator.CalculateHand(game.DealerHand) : dealerTotal;

        string dealerCards = string.Join(" ", game.DealerHand.Select(c => $"<img src='{DeckUtils.GetCardImageUrl(c, config)}' width='50'>"));
        string playerCards = string.Join(" ", game.PlayerHand.Select(c => $"<img src='{DeckUtils.GetCardImageUrl(c, config)}' width='50'>"));

        string html = $@"
            <center>
                <font color='#dc5555'><b>{localizer["Dealer"]}:</b> <font color='white'>{finalDealerTotal}</font><br>{dealerCards}<br></font>
                <font color='#d8dc55'><b>{localizer["You"]}:</b> <font color='white'>{finalPlayerTotal}</font><br>{playerCards}</font><br>
                <font color='{color}' class='fontSize-m'>{title} - {message}</font>
            </center>";

        gameManager.SetEndGameState(steamId, html);
        if (endGameTimers.ContainsKey(steamId))
        {
            endGameTimers[steamId]?.Kill();
        }

        endGameTimers[steamId] = new Timer(3.0f, () =>
        {
            gameManager.RemoveEndGameState(steamId);
            endGameTimers.Remove(steamId);
        });

        gameManager.EndGame(steamId);
    }

    private void AnnounceGameResult(CCSPlayerController player, GameResult result, int playerTotal, int dealerTotal, bool isInactivity = false)
    {
        string playerName = player.PlayerName;
        string resultMessage = result switch
        {
            GameResult.Win => localizer["Player Won", playerName],
            GameResult.Draw => localizer["Player Tied", playerName],
            _ => localizer["Player Lost", playerName]
        };

        string inactivitySuffix = isInactivity ? $" ({localizer["Inactivity Loss"]})" : "";
        string announcement = $"{localizer["prefix"]} {resultMessage}{inactivitySuffix} {localizer["Game Result Details", playerTotal, dealerTotal]}";
        Server.PrintToChatAll(announcement);
    }

    private void AnnounceBustResult(CCSPlayerController player, int playerTotal)
    {
        string playerName = player.PlayerName;
        string announcement = $"{localizer["prefix"]} {localizer["Player Lost", playerName]} {localizer["Bust Result Details", playerTotal]}";
        Server.PrintToChatAll(announcement);
    }

    public void Cleanup()
    {
        foreach (var timer in endGameTimers.Values)
        {
            timer?.Kill();
        }
        endGameTimers.Clear();

        gameManager.CleanupAll();
    }
}