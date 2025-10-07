using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace Blackjack.Utils
{
    public static class PlayerUtils
    {
        public static void Reply(CCSPlayerController player, IStringLocalizer localizer, string messageKey)
        {
            player.PrintToChat($"{localizer["prefix"]} {localizer[messageKey]}");
        }
    }
}