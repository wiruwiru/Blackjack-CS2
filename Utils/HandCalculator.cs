namespace Blackjack.Utils
{
    public static class HandCalculator
    {
        public static int CalculateHand(List<Card> hand)
        {
            int total = 0;
            int aces = 0;

            foreach (var card in hand)
            {
                if (card.Rank == "A")
                {
                    aces++;
                }
                else if (card.Rank == "J" || card.Rank == "Q" || card.Rank == "K")
                {
                    total += 10;
                }
                else
                {
                    total += int.Parse(card.Rank);
                }
            }

            for (int i = 0; i < aces; i++)
            {
                total += (total + 11 <= 21) ? 11 : 1;
            }

            return total;
        }

        public static int GetCardValue(Card card, List<Card> hand)
        {
            if (card.Rank == "A")
            {
                int totalWithoutAces = 0;
                int aces = 0;

                foreach (var c in hand)
                {
                    if (c.Rank == "A")
                    {
                        aces++;
                    }
                    else if (c.Rank == "J" || c.Rank == "Q" || c.Rank == "K")
                    {
                        totalWithoutAces += 10;
                    }
                    else
                    {
                        totalWithoutAces += int.Parse(c.Rank);
                    }
                }

                for (int i = 0; i < aces; i++)
                {
                    totalWithoutAces += (totalWithoutAces + 11 <= 21) ? 11 : 1;
                }

                return totalWithoutAces - CalculateHand(hand.Where(c => c != card).ToList());
            }
            else if (card.Rank == "J" || card.Rank == "Q" || card.Rank == "K")
            {
                return 10;
            }
            else
            {
                return int.Parse(card.Rank);
            }
        }
    }
}