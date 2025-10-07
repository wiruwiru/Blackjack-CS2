using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Microsoft.Extensions.Localization;

using Blackjack.Config;
using Blackjack.Utils;

namespace Blackjack.Managers;

public class GameManager
{
    private readonly Dictionary<ulong, GameState> activeGames = new();
    private readonly Dictionary<ulong, string> endGameStates = new();
    private readonly Dictionary<ulong, DateTime> lastGameTime = new();
    private readonly Dictionary<ulong, Timer> inactivityTimers = new();
    private readonly Dictionary<ulong, DateTime> lastActivityTime = new();
    private readonly Random rng = new();
    private BaseConfigs config;
    private Action<ulong>? onInactivityTimeout;

    public GameManager(BaseConfigs config)
    {
        this.config = config;
    }

    public void UpdateConfig(BaseConfigs newConfig)
    {
        config = newConfig;
    }

    public void SetInactivityTimeoutCallback(Action<ulong> callback)
    {
        onInactivityTimeout = callback;
    }

    public bool HasActiveGame(ulong steamId) => activeGames.ContainsKey(steamId);

    public bool IsOnCooldown(ulong steamId)
    {
        if (!lastGameTime.TryGetValue(steamId, out var lastTime))
            return false;

        var timeSinceLastGame = (DateTime.UtcNow - lastTime).TotalSeconds;
        return timeSinceLastGame < config.GameCooldown;
    }

    public int GetRemainingCooldown(ulong steamId)
    {
        if (!lastGameTime.TryGetValue(steamId, out var lastTime))
            return 0;

        var timeSinceLastGame = (DateTime.UtcNow - lastTime).TotalSeconds;
        var remaining = config.GameCooldown - timeSinceLastGame;
        return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
    }

    public GameState? GetGameState(ulong steamId)
    {
        return activeGames.TryGetValue(steamId, out var game) ? game : null;
    }

    public void StartNewGame(ulong steamId)
    {
        var game = new GameState
        {
            Deck = DeckUtils.CreateDeck(rng),
            PlayerHand = new List<Card>(),
            DealerHand = new List<Card>()
        };

        game.PlayerHand.Add(DeckUtils.DrawCard(game, rng));
        game.PlayerHand.Add(DeckUtils.DrawCard(game, rng));
        game.DealerHand.Add(DeckUtils.DrawCard(game, rng));

        if (config.DealerWinPercentage > 50)
        {
            game.DealerHand.Add(DrawCardWithProbability(game, config.DealerWinPercentage));
        }
        else
        {
            game.DealerHand.Add(DeckUtils.DrawCard(game, rng));
        }

        activeGames[steamId] = game;
        lastActivityTime[steamId] = DateTime.UtcNow;

        StartInactivityTimer(steamId);
    }

    private Card DrawCardWithProbability(GameState game, int dealerWinPercentage)
    {
        int dealerCurrentTotal = HandCalculator.CalculateHand(game.DealerHand);

        var goodCards = GetGoodCardsForDealer(dealerCurrentTotal, dealerWinPercentage);
        foreach (var card in game.Deck)
        {
            if (goodCards.Contains(card.Rank))
            {
                game.Deck.Remove(card);
                return card;
            }
        }

        return DeckUtils.DrawCard(game, rng);
    }

    private HashSet<string> GetGoodCardsForDealer(int dealerCurrentTotal, int dealerWinPercentage)
    {
        var goodCards = new HashSet<string>();
        if (dealerCurrentTotal <= 11)
        {
            if (dealerWinPercentage >= 80)
            {
                goodCards.UnionWith(["10", "J", "Q", "K", "A"]);
            }
            else if (dealerWinPercentage >= 60)
            {
                goodCards.UnionWith(["9", "10", "J", "Q", "K", "A"]);
            }
            else
            {
                goodCards.UnionWith(["8", "9", "10", "J", "Q", "K", "A"]);
            }
        }
        else if (dealerCurrentTotal <= 16)
        {
            if (dealerWinPercentage >= 80)
            {
                goodCards.UnionWith(["5", "6", "7", "8", "9", "10", "J", "Q", "K"]);
            }
            else if (dealerWinPercentage >= 60)
            {
                goodCards.UnionWith(["4", "5", "6", "7", "8", "9", "10"]);
            }
            else
            {
                goodCards.UnionWith(["3", "4", "5", "6", "7", "8"]);
            }
        }
        else
        {
            if (dealerWinPercentage >= 80)
            {
                goodCards.UnionWith(["2", "3", "4"]);
            }
            else if (dealerWinPercentage >= 60)
            {
                goodCards.UnionWith(["2", "3", "4", "5"]);
            }
            else
            {
                goodCards.UnionWith(["A", "2", "3", "4", "5"]);
            }
        }

        return goodCards;
    }

    private void StartInactivityTimer(ulong steamId)
    {
        if (inactivityTimers.TryGetValue(steamId, out var existingTimer))
        {
            existingTimer?.Kill();
        }

        var timer = new Timer(config.InactivityTimeout, () =>
        {
            if (activeGames.ContainsKey(steamId))
            {
                onInactivityTimeout?.Invoke(steamId);
                EndGameDueToInactivity(steamId);
            }
            inactivityTimers.Remove(steamId);
        });

        inactivityTimers[steamId] = timer;
    }

    private void ResetInactivityTimer(ulong steamId)
    {
        lastActivityTime[steamId] = DateTime.UtcNow;
        StartInactivityTimer(steamId);
    }

    public HitResult PlayerHit(ulong steamId)
    {
        if (!activeGames.TryGetValue(steamId, out var game))
        {
            return new HitResult { IsBust = false, Total = 0 };
        }

        ResetInactivityTimer(steamId);

        game.PlayerHand.Add(DeckUtils.DrawCard(game, rng));
        int playerTotal = HandCalculator.CalculateHand(game.PlayerHand);

        return new HitResult
        {
            IsBust = playerTotal > 21,
            Total = playerTotal
        };
    }

    public StandResult PlayerStand(ulong steamId)
    {
        if (!activeGames.TryGetValue(steamId, out var game))
        {
            return new StandResult { Result = GameResult.Lose, DealerTotal = 0, PlayerTotal = 0 };
        }

        if (inactivityTimers.TryGetValue(steamId, out var timer))
        {
            timer?.Kill();
            inactivityTimers.Remove(steamId);
        }

        while (HandCalculator.CalculateHand(game.DealerHand) < 17)
        {
            if (config.DealerWinPercentage > 50)
            {
                game.DealerHand.Add(DrawCardWithProbability(game, config.DealerWinPercentage));
            }
            else
            {
                game.DealerHand.Add(DeckUtils.DrawCard(game, rng));
            }
        }

        int playerTotal = HandCalculator.CalculateHand(game.PlayerHand);
        int dealerTotal = HandCalculator.CalculateHand(game.DealerHand);

        var result = GameLogic.DetermineWinner(playerTotal, dealerTotal);

        return new StandResult
        {
            Result = result,
            DealerTotal = dealerTotal,
            PlayerTotal = playerTotal
        };
    }

    public void EndGame(ulong steamId)
    {
        activeGames.Remove(steamId);
        lastGameTime[steamId] = DateTime.UtcNow;
        lastActivityTime.Remove(steamId);

        if (inactivityTimers.TryGetValue(steamId, out var timer))
        {
            timer?.Kill();
            inactivityTimers.Remove(steamId);
        }
    }

    private void EndGameDueToInactivity(ulong steamId)
    {
        activeGames.Remove(steamId);
        lastGameTime[steamId] = DateTime.UtcNow;
        lastActivityTime.Remove(steamId);
    }

    public void SetEndGameState(ulong steamId, string html)
    {
        endGameStates[steamId] = html;
    }

    public void RemoveEndGameState(ulong steamId)
    {
        endGameStates.Remove(steamId);
    }

    public void UpdateActiveGames(IStringLocalizer localizer)
    {
        foreach (var (steamId, game) in activeGames)
        {
            var player = Utilities.GetPlayerFromSteamId(steamId);
            if (player?.IsValid == true)
            {
                ShowGameState(player, game, localizer, steamId);
            }
        }
    }

    public void UpdateEndGameStates()
    {
        foreach (var (steamId, html) in endGameStates)
        {
            var player = Utilities.GetPlayerFromSteamId(steamId);
            if (player?.IsValid == true)
            {
                player.PrintToCenterHtml(html);
            }
        }
    }

    private void ShowGameState(CCSPlayerController player, GameState game, IStringLocalizer localizer, ulong steamId)
    {
        int playerTotal = HandCalculator.CalculateHand(game.PlayerHand);
        int dealerVisible = HandCalculator.GetCardValue(game.DealerHand[0], new List<Card> { game.DealerHand[0] });

        int remainingTime = config.InactivityTimeout;
        if (lastActivityTime.TryGetValue(steamId, out var lastActivity))
        {
            var elapsed = (DateTime.UtcNow - lastActivity).TotalSeconds;
            remainingTime = Math.Max(0, config.InactivityTimeout - (int)elapsed);
        }

        string playerCards = string.Join(" ", game.PlayerHand.Select(c => $"<img src='{DeckUtils.GetCardImageUrl(c, config)}' width='50'>"));
        string dealerCards = $"<img src='{DeckUtils.GetCardImageUrl(game.DealerHand[0], config)}' width='50'> <img src='{config.CardBackUrl}' width='50'>";

        string html = $@"
            <center>
                <font color='#dc5555'><b>{localizer["Dealer"]}:</b> <font color='white'>{dealerVisible}</font><br>{dealerCards}<br></font>
                <font color='#d8dc55'><b>{localizer["You"]}:</b> <font color='white'>{playerTotal}</font><br>{playerCards}</font><br>
                <font color='white' class='fontSize-m'>{localizer["ChooseTitle"]}:</font> <font color='#61dc55' class='fontSize-m'>!hit</font> <font color='white' class='fontSize-m'>o</font> <font color='#dc5555' class='fontSize-m'>!stand</font><br>
                <font color='#FFA500' class='fontSize-s'>⏱️ {remainingTime}s</font>
            </center>";

        player.PrintToCenterHtml(html);
    }

    public void CleanupAll()
    {
        foreach (var timer in inactivityTimers.Values)
        {
            timer?.Kill();
        }
        inactivityTimers.Clear();

        activeGames.Clear();
        endGameStates.Clear();
        lastGameTime.Clear();
        lastActivityTime.Clear();
    }
}

public class HitResult
{
    public bool IsBust { get; set; }
    public int Total { get; set; }
}

public class StandResult
{
    public GameResult Result { get; set; }
    public int DealerTotal { get; set; }
    public int PlayerTotal { get; set; }
}