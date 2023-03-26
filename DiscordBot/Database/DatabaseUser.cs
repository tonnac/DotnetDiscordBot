using Newtonsoft.Json;

namespace DiscordBot.Database;

public class DatabaseUser
{
    [JsonProperty] public ulong guildid;
    [JsonProperty] public ulong userid;
}