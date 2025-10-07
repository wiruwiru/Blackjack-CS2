using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Blackjack.Config;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("Permissions")]
    public List<string> Permissions { get; set; } = new();

    [JsonPropertyName("PermissionFlag")]
    public string PermissionFlag { get; set; } = "@css/root";

    [JsonPropertyName("BaseCardUrl")]
    public string BaseCardUrl { get; set; } = "https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/jpg/no-maps/blackjack/";

    [JsonPropertyName("CardBackUrl")]
    public string CardBackUrl { get; set; } = "https://cdn.jsdelivr.net/gh/wiruwiru/MapsImagesCDN-CS/jpg/no-maps/blackjack/53.jpg";

    [JsonPropertyName("GameCooldown")]
    public int GameCooldown { get; set; } = 60;

    [JsonPropertyName("InactivityTimeout")]
    public int InactivityTimeout { get; set; } = 30;

    [JsonPropertyName("DealerWinPercentage")]
    public int DealerWinPercentage { get; set; } = 60;

    [JsonPropertyName("AnnounceResults")]
    public bool AnnounceResults { get; set; } = true;

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}