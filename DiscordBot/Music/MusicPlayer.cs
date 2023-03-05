using DisCatSharp.CommandsNext;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.EventArgs;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Music;

public class MusicPlayer
{
    private readonly Queue<MusicTrack> _queue = new Queue<MusicTrack>();
    public readonly LavalinkGuildConnection Connection;

    public MusicPlayer(LavalinkGuildConnection connection)
    {
        Connection = connection;

        Connection.PlaybackStarted += OnTractStarted;
        Connection.PlaybackFinished += OnTrackFinished;
    }

    public async Task Play(CommandContext ctx, string searchQuery)
    {
        var loadResult = await Connection.Node.Rest.GetTracksAsync(searchQuery);
        
        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
            || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.RespondAsync($"Track search failed for {searchQuery}.");
            return;
        }

        MusicTrack newTrack = MusicTrack.CreateMusicTrack(ctx, loadResult.Tracks.First());
        _queue.Enqueue(newTrack);

        if (Connection.CurrentState.CurrentTrack == null)
        {
            await Connection.PlayAsync(newTrack.LavaLinkTrack);
        }
        else
        {
            await ctx.RespondAsync($"Added Queue {newTrack.LavaLinkTrack.Title}!");
        }
    }

    public async Task Leave()
    {
        await Connection.DisconnectAsync();
    }

    private async Task OnTractStarted(LavalinkGuildConnection connection, TrackStartEventArgs args)
    {
        if (_queue.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(701, "unknown track started"), "MusicPlayer queue has not track");
            return;
        }
        
        
        MusicTrack track = _queue.Peek();
        connection.Node.Discord.Logger.LogDebug(new EventId(703, "Track Start"), $"Track Started {args.Track.Title}");
        await track.Channel.SendMessageAsync($"Play Track {args.Track.Title}");
    }
    
    private async Task OnTrackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs args)
    {
        if (_queue.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(702, "queue already empty"), "track finished but MusicPlayer queue is already empty");
            return;
        }

        var track = _queue.Dequeue();
        connection.Node.Discord.Logger.LogDebug(new EventId(704, "Track Finished"), $"Track Started {track.LavaLinkTrack.Title}");
        if (_queue.Count == 0)
        {
            await track.Channel.SendMessageAsync("Queue has ended");
            await Leave();
        }
        else
        {
            await Connection.PlayAsync(_queue.Peek().LavaLinkTrack);
        }
    }
}