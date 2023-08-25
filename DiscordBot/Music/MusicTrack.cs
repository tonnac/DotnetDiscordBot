using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using Discord;
using DiscordBot.Database;

namespace DiscordBot.Music;

public class MusicTrack
{
    public DiscordChannel Channel { get; }
    public LavalinkTrack LavaLinkTrack { get; }
    public DiscordUser User { get; }
    public int TrackIndex { get; }
    public TimeSpan TimeSpan = TimeSpan.Zero;
    public DateTime AddedTime = DateTime.MinValue;
    public DateTime StartTime = DateTime.MinValue;
    public DateTime FinishTime = DateTime.MinValue;

    private MusicTrack(DiscordUser requester, DiscordChannel channel, LavalinkTrack lavaLinkTrack, int trackIndex)
    {
        User = requester;
        Channel = channel;
        LavaLinkTrack = lavaLinkTrack;
        TrackIndex = trackIndex;
        AddedTime = Utility.GetCurrentTime();
    }

    public void TrackStart()
    {
        if (StartTime == DateTime.MinValue)
        {
            StartTime = Utility.GetCurrentTime();
        }
    }
    public async Task<bool> TrackFinished()
    {
        FinishTime = Utility.GetCurrentTime();
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        await database.RegisterMusic(this);

        var lists = await database.GetDatabaseMusics(Channel.Guild);

        int m = 53;
        return true;
    }

    public virtual string GetTrackTitle()
    {
        string trackName = $"[{LavaLinkTrack.Title}]({LavaLinkTrack.Uri})";
        switch (TrackIndex)
        {
            case 2:
                return $"{trackName} {"#BG".Bold()}";
            case 1:
                return $"{trackName} {"#LP".Bold()}";
            default:
                return trackName;
        }
    }
    public static MusicTrack CreateMusicTrack(CommandContext ctx, LavalinkTrack track, int trackIndex)
    {
        return CreateMusicTrack(ctx.Member, ctx.Channel, track, trackIndex);
    }
    public static MusicTrack CreateMusicTrack(DiscordMember member, DiscordChannel channel, LavalinkTrack track, int trackIndex)
    {
        MusicTrack newMusicTrack = new MusicTrack(member, channel, track, trackIndex);
        return newMusicTrack;
    }
    
}