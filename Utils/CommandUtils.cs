using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Localization;

using Blackjack.Config;

namespace Blackjack.Utils
{
    public static class CommandUtils
    {
        public static bool ValidateCommand(CCSPlayerController? player, CommandInfo command, BaseConfigs config, IStringLocalizer localizer)
        {
            if (player == null || !player.IsValid)
            {
                command.ReplyToCommand($"{localizer["prefix"]} {localizer["only_players"]}");
                return false;
            }

            if (!IsPlayerAuthorized(player, config))
            {
                command.ReplyToCommand($"{localizer["prefix"]} {localizer["no_permission"]}");
                return false;
            }

            return true;
        }

        public static bool IsPlayerAuthorized(CCSPlayerController player, BaseConfigs config)
        {
            string steamId64 = player.SteamID.ToString();
            if (config.Permissions.Contains(steamId64))
            {
                return true;
            }

            return AdminManager.PlayerHasPermissions(player, config.PermissionFlag);
        }
    }
}