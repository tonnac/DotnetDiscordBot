using Newtonsoft.Json;

namespace DiscordBot.Database;

public class DatabaseUser
{
    [JsonProperty] public ulong userid;
    [JsonProperty] public bool aram;
    [JsonProperty] public int bosskillcount;
    [JsonProperty] public ulong bosstotaldamage;
    [JsonProperty] public int gold;
}