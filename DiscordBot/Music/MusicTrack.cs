using System.Reflection;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;

namespace DiscordBot.Music;

public class MusicTrack
{
    private readonly DiscordUser _requester;
    public DiscordChannel Channel { get; }
    public LavalinkTrack LavaLinkTrack { get; }
    private MusicTrack(DiscordUser requester, DiscordChannel channel, LavalinkTrack lavaLinkTrack)
    {
        _requester = requester;
        Channel = channel;
        LavaLinkTrack = lavaLinkTrack;
    }

    public static MusicTrack CreateMusicTrack(CommandContext ctx, LavalinkTrack track)
    {
        MusicTrack newMusicTrack = new MusicTrack(ctx.Member, ctx.Channel, track);
        return newMusicTrack;
    }
    
}