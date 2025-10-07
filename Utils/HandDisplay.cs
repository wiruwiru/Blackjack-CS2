using Blackjack.Config;

namespace Blackjack.Utils
{
    public static class HandDisplay
    {
        public static string HandToString(List<Card> hand, List<Card> fullHand, BaseConfigs config)
        {
            string cardsHtml = string.Join(" ", hand.Select(c => $"<img src='{DeckUtils.GetCardImageUrl(c, config)}' width='50'>"));
            string valuesHtml = string.Join(" + ", hand.Select(c => $"{HandCalculator.GetCardValue(c, fullHand)}"));
            int total = HandCalculator.CalculateHand(fullHand);

            if (hand.Count == 1)
            {
                valuesHtml = $"({HandCalculator.GetCardValue(hand[0], fullHand)})";
            }
            else
            {
                valuesHtml = $"({valuesHtml} = {total})";
            }

            return $"<div style='display:inline-block; vertical-align:middle; margin:0 5px'>{cardsHtml}</div><div style='display:inline-block; vertical-align:middle; color:#00FFFF; margin-left:10px'>{valuesHtml}</div>";
        }
    }
}