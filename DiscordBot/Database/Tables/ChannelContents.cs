using Newtonsoft.Json;

namespace DiscordBot.Database.Tables;

public class ChannelContents
{
    [JsonProperty] public ulong guildid;
    [JsonProperty] public ulong channelid;
    [JsonProperty] public ulong contentsvalue;
}