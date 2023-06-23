using Newtonsoft.Json;

namespace DiscordBot.Music;

public class PlayingMusic
{
    [JsonProperty] public string Url;
    [JsonProperty] public TimeSpan? Time;
    [JsonProperty] public ulong RequestChannel;
    [JsonProperty] public ulong MemberId;
}

public class Playlist
{
    [JsonProperty] public ulong Channel;
    [JsonProperty] public List<PlayingMusic> List = new ();
    [JsonProperty] public List<PlayingMusic> LongPlaylist = new ();
}