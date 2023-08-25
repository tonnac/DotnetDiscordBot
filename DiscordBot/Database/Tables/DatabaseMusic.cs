using Newtonsoft.Json;

namespace DiscordBot.Database.Tables;

public class DatabaseMusic
{
    [JsonProperty] public string identifier;
    [JsonProperty] public Uri url;
    [JsonProperty] public ulong guildid;
    [JsonProperty] public DateTime starttime;
    [JsonProperty] public DateTime finishtime;
    [JsonProperty] public ulong userid;
    [JsonProperty] public string nickname;
    [JsonProperty] public int priority;
}