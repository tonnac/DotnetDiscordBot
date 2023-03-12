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
    private MusicTrack(DiscordUser requester, DiscordChannel channel, LavalinkTrack lavaLinkTrack)
    {
        User = requester;
        Channel = channel;
        LavaLinkTrack = lavaLinkTrack;
    }

    public static MusicTrack CreateMusicTrack(CommandContext ctx, LavalinkTrack track)
    {
        MusicTrack newMusicTrack = new MusicTrack(ctx.Member, ctx.Channel, track);
        return newMusicTrack;
    }
    
}