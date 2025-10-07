using Blackjack.Config;

namespace Blackjack.Utils
{
    public static class DeckUtils
    {
        private static readonly string[] Suits = { "♣", "♦", "♥", "♠" };
        private static readonly string[] Ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        public static List<Card> CreateDeck(Random rng)
        {
            List<Card> deck = new();

            foreach (var suit in Suits)
            {
                foreach (var rank in Ranks)
                {
                    deck.Add(new Card(rank, suit));
                }
            }

            ShuffleDeck(deck, rng);
            return deck;
        }

        public static void ShuffleDeck(List<Card> deck, Random rng)
        {
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
        }

        public static Card DrawCard(GameState game, Random rng)
        {
            if (game.Deck.Count == 0)
            {
                game.Deck.AddRange(CreateDeck(rng));
            }

            Card card = game.Deck[0];
            game.Deck.RemoveAt(0);
            return card;
        }

        public static string GetCardImageUrl(Card card, BaseConfigs config)
        {
            int suitIndex = Array.IndexOf(Suits, card.Suit);
            int rankIndex = Array.IndexOf(Ranks, card.Rank);
            int cardNumber = suitIndex * 13 + rankIndex + 1;

            string baseUrl = config.BaseCardUrl;
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            return $"{baseUrl}{cardNumber}.jpg";
        }
    }
}