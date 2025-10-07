namespace Blackjack
{
    public class Card
    {
        public string Rank { get; }
        public string Suit { get; }

        public Card(string rank, string suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public override string ToString() => $"{Rank}{Suit}";
    }

    public class GameState
    {
        public List<Card> Deck { get; set; } = new();
        public List<Card> PlayerHand { get; set; } = new();
        public List<Card> DealerHand { get; set; } = new();
    }

    public enum GameResult
    {
        Win,
        Lose,
        Draw
    }
}