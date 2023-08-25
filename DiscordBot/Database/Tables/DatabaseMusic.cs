using Newtonsoft.Json;

namespace DiscordBot.Database.Tables;

public class DatabaseMusic
{
    [JsonProperty] public string identifier;
    [JsonProperty] public string title;
    [JsonProperty] public string uri;
    [JsonProperty] public ulong guildid;
    [JsonProperty] public DateTime starttime;
    [JsonProperty] public DateTime finishtime;
    [JsonProperty] public ulong userid;
    [JsonProperty] public string nickname;
    [JsonProperty] public int priority;
}