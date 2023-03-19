using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.EventArgs;
using DiscordBot.Resource;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Music;

public class MusicPlayer
{
    private readonly List<MusicTrack> _list = new List<MusicTrack>();
    public readonly LavalinkGuildConnection Connection;
    private static readonly int QueuePagePerCount = 10;

    public MusicPlayer(LavalinkGuildConnection connection)
    {
        Connection = connection;

        Connection.PlaybackStarted += OnTractStarted;
        Connection.PlaybackFinished += OnTrackFinished;
    }

    public async Task Play(CommandContext ctx, string searchQuery)
    {
        LavalinkLoadResult? loadResult = null;
        if (searchQuery.Contains("https://"))
        {
            loadResult = await Connection.Node.Rest.GetTracksAsync(new Uri(searchQuery));
        }
        else
        {
            loadResult = await Connection.Node.Rest.GetTracksAsync(searchQuery);
        }

        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
            || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.RespondAsync($"Track search failed for {searchQuery}.");
            return;
        }

        List<MusicTrack> addedTrack;
        
        if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
        {
            addedTrack = loadResult.Tracks.Select(track => MusicTrack.CreateMusicTrack(ctx, track)).ToList();
        }
        else
        {
            addedTrack = new List<MusicTrack> { MusicTrack.CreateMusicTrack(ctx, loadResult.Tracks.First()) };
        }
        
        _list.AddRange(addedTrack);

        MusicTrack newTrack = addedTrack.First();

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
        if (_list.Count == 0)
        {
            return;
        }

        MusicTrack currentTrack = _list.First();
        if (_list.Count == 1)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithAuthor(Localization.NowPlaying)
                .WithDescription($"[{currentTrack.LavaLinkTrack.Title}]({currentTrack.LavaLinkTrack.Uri.AbsoluteUri})")
                .AddField(new DiscordEmbedField(Localization.RequestedBy, $"{currentTrack.User.Mention}", true))
                .AddField(new DiscordEmbedField(Localization.Duration, Utility.ProgressBar(Connection.CurrentState.PlaybackPosition, currentTrack.LavaLinkTrack.Length)));
            await ctx.RespondAsync(embedBuilder);
        }
        else
        {
            var copyTrack = _list.ToList();

            TimeSpan totalLength = default;
            foreach (MusicTrack musicTrack in copyTrack)
            {
                totalLength += musicTrack.LavaLinkTrack.Length;
            }

            copyTrack.RemoveAt(0);
            
            List<List<MusicTrack>> chunkedTracks = copyTrack.Partition(QueuePagePerCount);
            
            DiscordEmbedBuilder EmbedBuilder(int index) =>
                new DiscordEmbedBuilder().WithColor(DiscordColor.Orange)
                    .WithAuthor(Localization.Queue)
                    .WithDescription($"{Localization.NowPlaying.Bold()}: [{currentTrack.LavaLinkTrack.Title}]({currentTrack.LavaLinkTrack.Uri}) \n\n{Localization.UpNext.Bold()} \n {string.Join("\n", chunkedTracks[index].Select((track, i) => $"{Convert.ToString(QueuePagePerCount * index + i + 1).InlineCode()} [{track.LavaLinkTrack.Title}]({track.LavaLinkTrack.Uri}) \n{track.LavaLinkTrack.Length.ToDuration()}"))}")
                    .AddField(new DiscordEmbedField($"{Localization.TotalSongs}: \n", $"{_list.Count}".InlineCode(), true))
                    .AddField(new DiscordEmbedField($"{Localization.TotalLength}: \n", $"{totalLength.ToDuration()}".InlineCode(), true));

            var pages = chunkedTracks.Select((tracks, index) => new Page(embed: EmbedBuilder(index))).ToList();

            if (pages.Count > 1)
            {
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, new PaginationEmojis(), timeoutOverride: TimeSpan.FromSeconds(30));
            }
            else
            {
                await ctx.RespondAsync(EmbedBuilder(0));
            }
        }
    }

    public async Task NowPlaying(CommandContext ctx)
    {
        if (_list.Count == 0)
        {
            return;
        }

        MusicTrack currTrack = _list.First();
        
        var current = Connection.CurrentState.PlaybackPosition;
        var total = currTrack.LavaLinkTrack.Length;

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Blue)
            .WithAuthor(Localization.NowPlaying)
            .WithDescription($"[{currTrack.LavaLinkTrack.Title}]({currTrack.LavaLinkTrack.Uri.AbsoluteUri})")
            .AddField(new DiscordEmbedField(Localization.RequestedBy, currTrack.User.Mention))
            .AddField(new DiscordEmbedField(Localization.Duration, Utility.ProgressBar(current, total)));
        
        await ctx.RespondAsync(embedBuilder);
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
            await Connection.StopAsync();
        }
    }

    public async Task Remove(CommandContext ctx, string indexString)
    {
        if (Connection.CurrentState.CurrentTrack == null)
            return;

        int index = int.Parse(indexString) - 1;
        if (index == 0)
        {
            await Connection.SeekAsync(_list.First().LavaLinkTrack.Length);
            return;
        }

        if (index < _list.Count)
        {
            var track =  _list[index];
            _list.RemoveAt(index);
            await ctx.RespondAsync($"Remove {index} {track.LavaLinkTrack.Title} {track.User.Mention}!");
        }
    }

    private async Task OnTractStarted(LavalinkGuildConnection connection, TrackStartEventArgs args)
    {
        if (_list.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(701, "unknown track started"), "MusicPlayer queue has not track");
            return;
        }
        
        MusicTrack track = _list.First();
        connection.Node.Discord.Logger.LogDebug(new EventId(703, "Track Start"), $"Track Started {args.Track.Title}");
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithAuthor(Localization.NowPlaying)
            .WithDescription($"[{args.Track.Title}]({args.Track.Uri.AbsoluteUri})")
            .AddField(new DiscordEmbedField(name: Localization.RequestedBy, value:$"{track.User.Mention}", inline:true))
            .AddField(new DiscordEmbedField(name: Localization.Duration, value:track.LavaLinkTrack.Length.ToDuration(), inline:true));
        
        await track.Channel.SendMessageAsync(embedBuilder.Build());
    }
    
    private async Task OnTrackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs args)
    {
        if (_list.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(702, "queue already empty"), "track finished but MusicPlayer queue is already empty");
            return;
        }

        var track = _list.First();
        _list.RemoveAt(0);
        connection.Node.Discord.Logger.LogDebug(new EventId(704, "Track Finished"), $"Track Started {track.LavaLinkTrack.Title}");
        if (_list.Count == 0)
        {
            await track.Channel.SendMessageAsync("Queue has ended");
            await Leave();
        }
        else
        {
            await Connection.PlayAsync(_list.First().LavaLinkTrack);
        }
    }
}