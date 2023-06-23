using System.Reflection;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using Discord;

namespace DiscordBot.Music;

public class MusicTrack
{
    public DiscordChannel Channel { get; }
    public LavalinkTrack LavaLinkTrack { get; }
    public DiscordUser User { get; }
    protected MusicTrack(DiscordUser requester, DiscordChannel channel, LavalinkTrack lavaLinkTrack)
    {
        User = requester;
        Channel = channel;
        LavaLinkTrack = lavaLinkTrack;
    }

    public virtual string GetTrackTitle()
    {
        return $"[{LavaLinkTrack.Title}]({LavaLinkTrack.Uri})";
    }
    public static MusicTrack CreateMusicTrack(CommandContext ctx, LavalinkTrack track, bool isLongPlay)
    {
        return CreateMusicTrack(ctx.Member, ctx.Channel, track, isLongPlay);
    }
    public static MusicTrack CreateMusicTrack(DiscordMember member, DiscordChannel channel, LavalinkTrack track, bool isLongPlay)
    {
        MusicTrack newMusicTrack = isLongPlay == false ? new MusicTrack(member, channel, track) : new LongPlayTrack(member, channel, track);
        return newMusicTrack;
    }
    
}