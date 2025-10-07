# Blackjack CS2
Blackjack is a fully-featured minigame plugin that brings the classic casino card game to Counter-Strike 2 servers with an intuitive HUD interface and comprehensive game mechanics.

> [!NOTE]
> This plugin is a modified and enhanced version of the original [cs2-blackjack](https://github.com/vulikit/cs2-blackjack) project

## 🚀 Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [Blackjack.zip](https://github.com/wiruwiru/Blackjack-CS2/releases/latest) from releases
3. Extract and upload to your game server: `csgo/addons/counterstrikesharp/plugins/Blackjack/`
4. Start server and configure the generated config file at `csgo/addons/counterstrikesharp/configs/plugins/Blackjack/`

---

## 📋 Configuration
| Parameter         | Description                                                                                        | Default | Required |
|-------------------|----------------------------------------------------------------------------------------------------|---------|----------|
| `Permissions`  | List of SteamID64s with access to use the commands. | `[]` | **YES**  |
| `PermissionFlag`  | Permission flag required to use commands if not in the permissions list. | `"@css/root"` | **YES**  |
| `BaseCardUrl` | Base URL for card images (1-52.jpg). | `"https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/jpg/no-maps/blackjack/"` | **YES**  |
| `CardBackUrl` | URL for the card back image (hidden dealer card). | `"https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/jpg/no-maps/blackjack/53.jpg"` | **YES**  |
| `GameCooldown` | Cooldown time in seconds between games per player. | `60` | **YES**  |
| `InactivityTimeout` | Time in seconds before a player loses due to inactivity. | `30` | **YES**  |
| `DealerWinPercentage` | Percentage chance (0-100) that influences dealer card drawing to favor dealer wins. Values above 50 give the dealer an advantage. | `60` | **YES**  |
| `AnnounceResults` | Whether to announce game results to all players in chat. | `true` | **YES**  |
| `EnableDebug`     | Enable detailed logging for troubleshooting. | `false` | **YES**  |

---

## 🎮 Commands
| Command | Alias | Description | Permission Required |
|---------|-------|-------------|---------------------|
| `css_blackjack` | `css_bj` | Start a new Blackjack game | Yes (configured in `PermissionFlag` or `Permissions`) |
| `css_hit` | - | Draw an additional card during your turn | Yes (configured in `PermissionFlag` or `Permissions`) |
| `css_stand` | - | End your turn and let the dealer play | Yes (configured in `PermissionFlag` or `Permissions`) |

---

## 🎯 Features
- **Inactivity Timer**: Automatic game timeout system with countdown display
- **Game Cooldown**: Prevents spam by enforcing wait times between games
- **Configurable Difficulty**: Adjust dealer win percentage to balance gameplay
- **Multi-language Support**: Includes English, Spanish, Portuguese (BR/PT), and Russian translations
- **Result Announcements**: Optional server-wide game result notifications
- **Permission System**: Flexible access control via flags or SteamID whitelist

---

## 📝 Support
For issues, questions, or feature requests, please create an issue on the GitHub repository.