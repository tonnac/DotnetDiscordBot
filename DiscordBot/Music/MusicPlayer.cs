using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
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

    public async Task Queue(CommandContext ctx)
    {
        if (_queue.Count == 0)
        {
            return;
        }

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
        int index = 0;
        MusicTrack currtrack = _queue.Peek();
        String urlInedxer = "";
        foreach (MusicTrack musicTrack in _queue)
        {
            index++;
            urlInedxer += index + " - " + $"[{musicTrack.LavaLinkTrack.Title}]({musicTrack.LavaLinkTrack.Uri.AbsoluteUri})" + $"by {musicTrack.User.Mention}" + "\n";
        }

        embedBuilder.Color = DiscordColor.Orange;
        embedBuilder.AddField(new DiscordEmbedField("현재 재생중인 음악", $"[{currtrack.LavaLinkTrack.Title}]({currtrack.LavaLinkTrack.Uri.AbsoluteUri})" + $"by {currtrack.User.Mention}"));
        embedBuilder.AddField(new DiscordEmbedField("대기열", urlInedxer));
        messageBuilder.AddEmbed(embedBuilder);
        await ctx.RespondAsync(messageBuilder);
    }

    public async Task NowPlaying(CommandContext ctx)
    {
        if (_queue.Count == 0)
        {
            return;
        }

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
        MusicTrack currtrack = _queue.Peek();

        embedBuilder.Color = DiscordColor.Blue;

        var current = Connection.CurrentState.PlaybackPosition;
        var total = currtrack.LavaLinkTrack.Length;
        var diff = (int)(current.TotalSeconds / total.TotalSeconds * 15); // Bar's length
        var bar = "\u25B6 ";
        for (int i = 0; i < 15; i++)
        {
            if (i == diff) bar += "\uD83D\uDD18";
            else bar += "▬";
        }

        bar += $" [{current:hh\\:mm\\:ss}/{total:hh\\:mm\\:ss}]";

        embedBuilder.WithFooter(bar);
        embedBuilder.AddField(new DiscordEmbedField("현재 재생중인 음악", $"[{currtrack.LavaLinkTrack.Title}]({currtrack.LavaLinkTrack.Uri.AbsoluteUri})" + $"by {currtrack.User.Mention}"));
        messageBuilder.AddEmbed(embedBuilder);
        await ctx.RespondAsync(messageBuilder);
    }

    public async Task Pause(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            await Connection.PauseAsync();
        }
    }

    public async Task Resume(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            await Connection.ResumeAsync();
        }
    }
    public async Task Seek(CommandContext ctx, string position)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            string[] separator = { "h", "m", "s" };
            TimeSpan positionTime = new TimeSpan();
            if (position.Contains('h') && position.Contains('m') && position.Contains('s'))
            {
                string[] splitResult = position.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int hour = int.Parse(splitResult[0]);
                int minute = int.Parse(splitResult[1]);
                int sec = int.Parse(splitResult[2]);
                positionTime = new TimeSpan(hour, minute, sec);
            }
            else if (position.Contains('m') && position.Contains('s'))
            {
                string[] splitResult = position.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int minute = int.Parse(splitResult[0]);
                int sec = int.Parse(splitResult[1]);
                positionTime = new TimeSpan(0, minute, sec);
            }
            else if (position.Contains('h'))
            {
                string[] splitResult = position.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int hour = int.Parse(splitResult[0]);
                positionTime = new TimeSpan(hour, 0, 0);               
            }
            else if (position.Contains('m'))
            {
                string[] splitResult = position.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int minutes = int.Parse(splitResult[0]);
                positionTime = new TimeSpan(0, minutes, 0);
            }
            else if (position.Contains('s'))
            {
                string[] splitResult = position.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                int sec = int.Parse(splitResult[0]);
                positionTime = new TimeSpan(0, 0, sec);
            }
            
            await Connection.SeekAsync(positionTime);
        } 
    }

    public async Task Skip(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            await Connection.SeekAsync(_queue.Peek().LavaLinkTrack.Length);
        }
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
        await track.Channel.SendMessageAsync($"Play Track {args.Track.Title} \n URL : {args.Track.Uri.AbsoluteUri} \n By : {track.User.Mention}");
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