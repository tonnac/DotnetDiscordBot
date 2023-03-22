using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;

namespace DiscordBot.Music;

public class LongPlayTrack : MusicTrack
{
    public TimeSpan TimeSpan = TimeSpan.Zero;
    
    protected internal LongPlayTrack(DiscordUser requester, DiscordChannel channel, LavalinkTrack lavaLinkTrack) : base(requester, channel, lavaLinkTrack)
    {
    }

    public override string GetTrackTitle()
    {
        return $"{base.GetTrackTitle()} {"#LP".Bold()}";
    }
}