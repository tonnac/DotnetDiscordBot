using Newtonsoft.Json;

namespace DiscordBot.Database.Tables;

public class DatabaseRoulette
{
    [JsonProperty] public string id;
    [JsonProperty] public ulong guildid;
    [JsonProperty] public DateTime time;
    [JsonProperty] public string winner;
    [JsonProperty] public string messagelink;
}

public class DatabaseRouletteMember
{
    [JsonProperty] public string rouletteid;
    [JsonProperty] public string name;
}
