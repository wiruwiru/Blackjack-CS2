namespace Blackjack.Utils
{
    public static class GameLogic
    {
        public static GameResult DetermineWinner(int playerTotal, int dealerTotal)
        {
            if (playerTotal > 21)
                return GameResult.Lose;

            if (dealerTotal > 21)
                return GameResult.Win;

            if (playerTotal > dealerTotal)
                return GameResult.Win;

            if (playerTotal == dealerTotal)
                return GameResult.Draw;

            return GameResult.Lose;
        }
    }
}