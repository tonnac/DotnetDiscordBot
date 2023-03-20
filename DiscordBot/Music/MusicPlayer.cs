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
    private readonly List<MusicTrack> _bgmList = new List<MusicTrack>();
    public readonly LavalinkGuildConnection Connection;
    private static readonly int QueuePagePerCount = 10;

    public MusicPlayer(LavalinkGuildConnection connection)
    {
        Connection = connection;

        Connection.PlaybackStarted += OnTractStarted;
        Connection.PlaybackFinished += OnTrackFinished;
    }

    public async Task Play(CommandContext ctx, string searchQuery, bool isBgm = false)
    {
        LavalinkLoadResult? loadResult;
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

        List<MusicTrack> addedTrack = loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded ? loadResult.Tracks.Select(track => MusicTrack.CreateMusicTrack(ctx, track)).ToList() : new List<MusicTrack> { MusicTrack.CreateMusicTrack(ctx, loadResult.Tracks.First()) };

        int position;
        if (isBgm)
        {
            addedTrack.RemoveAll(track =>
            {
                return _list.Find(musicTrack => musicTrack.LavaLinkTrack.TrackString == track.LavaLinkTrack.TrackString) != null;
            });

            position = _list.Count + _bgmList.Count;
            _bgmList.AddRange(addedTrack);
        }
        else
        {
            position = _list.Count;
            _list.AddRange(addedTrack);
        }

        if (addedTrack.Count > 0)
        {
            MusicTrack newTrack = addedTrack.First();
            
            if (Connection.CurrentState.CurrentTrack == null)
            {
                await Connection.PlayAsync(newTrack.LavaLinkTrack);
            }
            else if (_bgmList.Count > 0  && _bgmList.First().LavaLinkTrack.Identifier == Connection.CurrentState.CurrentTrack.Identifier)
            {
                _bgmList.First().TimeSpan = Connection.CurrentState.PlaybackPosition;
                await Connection.PlayAsync(newTrack.LavaLinkTrack);
            }
            else
            {
                var embedBuilder = new DiscordEmbedBuilder()
                    .WithAuthor(Localization.addedQueue)
                    .WithDescription($"[{newTrack.LavaLinkTrack.Title}]({newTrack.LavaLinkTrack.Uri})")
                    .AddField(new DiscordEmbedField(Localization.RequestedBy, newTrack.User.Mention, true))
                    .AddField(new DiscordEmbedField(Localization.Duration, newTrack.LavaLinkTrack.Length.ToDuration().InlineCode(), true))
                    .AddField(new DiscordEmbedField(Localization.positionInQueue, Convert.ToString(position).InlineCode(), true));
                await ctx.RespondAsync(embedBuilder);
            }
        }
    }

    public async Task Leave(CommandContext ctx)
    {
        await Connection.DisconnectAsync();
        var embedBuilder = new DiscordEmbedBuilder()
            .WithDescription($"{DiscordEmoji.FromName(ctx.Client, ":notes:")} | {Localization.disconnected.Bold()}");
        await ctx.RespondAsync(embedBuilder);
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }

    public async Task Queue(CommandContext ctx)
    {
        var copyTrack = _list.ToList();
        copyTrack.AddRange(_bgmList.ToList());
        if (copyTrack.Count == 0)
        {
            return;
        }

        MusicTrack currentTrack = copyTrack.First();
        if (copyTrack.Count == 1)
        {
            await NowPlaying(ctx);
        }
        else
        {

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
                    .AddField(new DiscordEmbedField($"{Localization.TotalSongs}: \n", $"{copyTrack.Count}".InlineCode(), true))
                    .AddField(new DiscordEmbedField($"{Localization.TotalLength}: \n", $"{totalLength.ToDuration()}".InlineCode(), true));

            var pages = chunkedTracks.Select((_, index) => new Page(embed: EmbedBuilder(index))).ToList();

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
        if (Connection.CurrentState.CurrentTrack == null)
        {
            return;
        }

        MusicTrack currTrack = FindTrack(Connection.CurrentState.CurrentTrack);
        
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
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }

    public async Task Resume(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            await Connection.ResumeAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }
    public async Task Seek(CommandContext ctx, string position)
    {
        if (Connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync(Localization.ErrorNotQueue);
            return;
        }

        try
        {
            await Connection.SeekAsync(Utility.GetTime(position));
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
        catch (Exception e)
        {
            Connection.Node.Discord.Logger.LogError(new EventId(705, "invalid seek command"), e.Message);
            await ctx.RespondAsync(String.Format(Localization.seek_Usage, Config.Prefix));
        }
    }

    public async Task Skip(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack != null)
        {
            await Connection.StopAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }

    public async Task Remove(CommandContext ctx, string indexString)
    {
        if (Connection.CurrentState.CurrentTrack == null)
        {
            await ctx.RespondAsync(Localization.ErrorNotQueue);
            return;
        }
        
        int index = int.Parse(indexString);
        
        var copyTracks = _list.ToList();
        int beginBgmIndex = copyTracks.Count;
        copyTracks.AddRange(_bgmList.ToList());

        if (index < 1 || index >= copyTracks.Count)
        {
            await ctx.RespondAsync(String.Format(Localization.remove_Usage, Config.Prefix));
            return;
        }

        var track =  copyTracks[index];
        if (index >= beginBgmIndex)
        {
            _bgmList.RemoveAt(index - beginBgmIndex);
        }
        else
        {
            _list.RemoveAt(index);
        }

        var embedBuilder = new DiscordEmbedBuilder()
            .WithDescription($"Removed Track {Convert.ToString(index).InlineCode()} [{track.LavaLinkTrack.Title}]({track.LavaLinkTrack.Uri})");
        await ctx.RespondAsync(embedBuilder);
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }

    private async Task OnTractStarted(LavalinkGuildConnection connection, TrackStartEventArgs args)
    {
        if (_list.Count == 0 && _bgmList.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(701, "unknown track started"), "MusicPlayer queue has not track");
            return;
        }

        MusicTrack track = FindTrack(args.Track);
        
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
        if (_list.Count == 0 && _bgmList.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(702, "queue already empty"), "track finished but MusicPlayer queue is already empty");
            return;
        }

        if (args.Reason != TrackEndReason.Replaced)
        {
            var trackPair = GetNextTrack(args.Track);

            if (trackPair.Value != null)
            {
                await Connection.PlayPartialAsync(trackPair.Value.LavaLinkTrack, trackPair.Value.TimeSpan, trackPair.Value.LavaLinkTrack.Length);
            }
            else
            {
                var embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Gold)
                    .WithAuthor(Localization.ErrorNotQueue)
                    .WithTimestamp(DateTime.Now);
                await trackPair.Key.Channel.SendMessageAsync(embedBuilder);
                await Connection.DisconnectAsync();
            }
        }
    }

    MusicTrack FindTrack(LavalinkTrack track)
    {
        var foundTrack = _list.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier) ?? _bgmList.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier);

        if (foundTrack == null)
        {
            throw new Exception("found track is null");
        }

        return foundTrack;
    }

    private KeyValuePair<MusicTrack, MusicTrack?> GetNextTrack(LavalinkTrack track)
    {
        Connection.Node.Discord.Logger.LogDebug(new EventId(704, "Track Finished"), $"Track Started {track.Title}");
        
        var foundTrack = _list.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier);

        if (foundTrack != null)
        {
            _list.RemoveAt(0);
        }
        else
        {
            foundTrack = _bgmList.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier);

            if (foundTrack != null)
            {
                _bgmList.RemoveAt(0);
            }
        }

        if (foundTrack == null)
        {
            throw new Exception("found track is null");
        }

        if (_list.Count > 0)
        {
            return new KeyValuePair<MusicTrack, MusicTrack?>(foundTrack, _list.First());
        }
        
        if (_bgmList.Count > 0)
        {
            return new KeyValuePair<MusicTrack, MusicTrack?>(foundTrack, _bgmList.First());
        }

        return new KeyValuePair<MusicTrack, MusicTrack?>(foundTrack, null);
    }
}