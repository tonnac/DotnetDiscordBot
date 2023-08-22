using Newtonsoft.Json;

namespace DiscordBot.Music;

public class PlayingMusic
{
    [JsonProperty] public int PlayListIndex;
    [JsonProperty] public string Url;
    [JsonProperty] public TimeSpan? Time;
    [JsonProperty] public ulong RequestChannel;
    [JsonProperty] public ulong MemberId;
}

public class Playlist
{
    [JsonProperty] public ulong Channel;
    [JsonProperty] public Dictionary<int, List<PlayingMusic>> List = new ();
}