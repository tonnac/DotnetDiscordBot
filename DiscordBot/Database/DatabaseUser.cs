using Newtonsoft.Json;

namespace DiscordBot.Database;

public class DatabaseUser
{
    [JsonProperty] public ulong userid;
}