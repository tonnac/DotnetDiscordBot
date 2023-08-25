using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.EventArgs;
using DiscordBot.Database;
using DiscordBot.Database.Tables;
using DiscordBot.Resource;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Music;

public class MusicPlayer
{
    private readonly Dictionary<int, List<MusicTrack>> _trackList = new ();
    public readonly LavalinkGuildConnection Connection;
    private static readonly int QueuePagePerCount = 10;
    private static readonly int MaxTrackCount = 3;
    private DiscordMessage? _trackStartMessage;

    public MusicPlayer(LavalinkGuildConnection connection)
    {
        Connection = connection;

        Connection.PlaybackStarted += OnTractStarted;
        Connection.PlaybackFinished += OnTrackFinished;
    }

    public async Task Play(Playlist playlist)
    {
        if (Connection.IsConnected == false)
        {
            return;
        }

        bool bPlayed = false;

        foreach (var keyValuePair in playlist.List)
        {
            foreach (var playingMusic in keyValuePair.Value)
            {
                LavalinkLoadResult? loadResult = await GetLoadResult(playingMusic.Url);

                if (loadResult == null)
                {
                    continue;
                }

                Connection.Guild.Members.TryGetValue(playingMusic.MemberId, out DiscordMember? member);
                Connection.Guild.Channels.TryGetValue(playingMusic.RequestChannel, out DiscordChannel? channel);

                if (member == null || channel == null)
                {
                    continue;
                }

                var musicTrack = MusicTrack.CreateMusicTrack(member, channel, loadResult.Tracks.First(), playingMusic);
                if (bPlayed == false && playingMusic.Time != null)
                {
                    await Connection.PlayPartialAsync(musicTrack.LavaLinkTrack, (TimeSpan)playingMusic.Time, musicTrack.LavaLinkTrack.Length);
                    bPlayed = true;
                }
                
                if (_trackList.TryGetValue(keyValuePair.Key, out List<MusicTrack>? musicTracks))
                {
                    musicTracks.Add(musicTrack);
                }
                else
                {
                    List<MusicTrack> newTrack = new List<MusicTrack>();
                    newTrack.Add(musicTrack);
                    _trackList.Add(keyValuePair.Key, newTrack);
                }
            }
        }
    }

    public Playlist GetPlayList()
    {
        var playList = new Playlist
        {
            Channel = Connection.Channel.Id,
            List = new Dictionary<int, List<PlayingMusic>>()
        };

        foreach (var keyValuePair in _trackList)
        {
            foreach (var musicTrack in keyValuePair.Value)
            {
                var playingMusic = new PlayingMusic
                {
                    Url = musicTrack.LavaLinkTrack.Uri.ToString(),
                    RequestChannel = musicTrack.Channel.Id,
                    MemberId = musicTrack.User.Id,
                    PlayListIndex = musicTrack.TrackIndex,
                    AddedTime = musicTrack.AddedTime,
                    StartTime = musicTrack.StartTime
                };

                if (musicTrack.LavaLinkTrack.Identifier == Connection.CurrentState.CurrentTrack.Identifier)
                {
                    playingMusic.Time = Connection.CurrentState.PlaybackPosition;
                }

                if (playList.List.TryGetValue(keyValuePair.Key, out List<PlayingMusic>? musicList))
                {
                    musicList.Add(playingMusic);
                }
                else
                {
                    List<PlayingMusic> newTrack = new List<PlayingMusic>();
                    newTrack.Add(playingMusic);
                    playList.List.Add(keyValuePair.Key, newTrack);
                }
            }
        }

        return playList;
    }

    public async Task Play(CommandContext ctx, string searchQuery, int index)
    {
        LavalinkLoadResult? loadResult = await GetLoadResult(searchQuery);

        if (loadResult == null)
        {
            return;
        }

        List<MusicTrack> addedTrack = loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded ? loadResult.Tracks.Select(track => MusicTrack.CreateMusicTrack(ctx, track, index)).ToList() : new List<MusicTrack> {MusicTrack.CreateMusicTrack(ctx, loadResult.Tracks.First(), index)};

        addedTrack.RemoveAll(track =>
        {
            for (int i = 0; i < MaxTrackCount; i++)
            {
                if (_trackList.TryGetValue(i, out List<MusicTrack>? trackList))
                {
                    if (trackList.Find(musicTrack =>
                            musicTrack.LavaLinkTrack.Identifier == track.LavaLinkTrack.Identifier) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        });

        if (_trackList.TryGetValue(index, out List<MusicTrack>? musicTracks))
        {
            musicTracks.AddRange(addedTrack);
        }
        else
        {
            List<MusicTrack> tracks = new List<MusicTrack>();
            tracks.AddRange(addedTrack);
            _trackList.Add(index, tracks);
        }

        int position = -1;
        for (int i = 0; i <= index; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? trackList))
            {
                position += trackList.Count;
            }
        }

        if (addedTrack.Count > 0)
        {
            MusicTrack newTrack = addedTrack.First();

            if (Connection.CurrentState.CurrentTrack == null)
            {
                await Connection.PlayAsync(newTrack.LavaLinkTrack);
                return;
            }

            for (int i = index + 1; i < MaxTrackCount; i++)
            {
                if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
                {
                    if (tracks.First().LavaLinkTrack.Identifier == Connection.CurrentState.CurrentTrack.Identifier)
                    {
                        tracks.First().TimeSpan = Connection.CurrentState.PlaybackPosition;
                        await Connection.PlayAsync(newTrack.LavaLinkTrack);
                        return;
                    }
                }
            }
            
            var embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(Localization.addedQueue)
                .WithDescription(newTrack.GetTrackTitle())
                .AddField(new DiscordEmbedField(Localization.RequestedBy, newTrack.User.Mention, true))
                .AddField(new DiscordEmbedField(Localization.Duration, newTrack.LavaLinkTrack.Length.ToDuration().InlineCode(), true))
                .AddField(new DiscordEmbedField(Localization.positionInQueue, Convert.ToString(position).InlineCode(), true));
            await ctx.RespondAsync(embedBuilder);
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
        var copyTrack = new List<MusicTrack>();
        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                copyTrack.AddRange(tracks);
            }
        }
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
                    .WithDescription($"{Localization.NowPlaying.Bold()}: {currentTrack.GetTrackTitle()} \n\n{Localization.UpNext.Bold()} \n {string.Join("\n", chunkedTracks[index].Select((track, i) => $"{Convert.ToString(QueuePagePerCount * index + i + 1).InlineCode()} {track.GetTrackTitle()} \n{track.LavaLinkTrack.Length.ToDuration()} | {Localization.RequestedBy.Bold()} {track.User.Mention}"))}")
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
            .WithDescription(currTrack.GetTrackTitle())
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

        var copyTracks = new List<MusicTrack>();
        foreach (var keyValuePair in _trackList)
        {
            copyTracks.AddRange(keyValuePair.Value);
        }

        if (index < 1 || index >= copyTracks.Count)
        {
            await ctx.RespondAsync(String.Format(Localization.remove_Usage, Config.Prefix));
            return;
        }

        var track = copyTracks[index];
        
        foreach (var keyValuePair in _trackList)
        {
            if (keyValuePair.Value.Remove(track))
            {
                break;
            }
        }

        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                if (tracks.Count == 0)
                {
                    _trackList.Remove(i);
                }
            }
        }

        var embedBuilder = new DiscordEmbedBuilder()
            .WithDescription($"Removed Track {Convert.ToString(index).InlineCode()} {track.GetTrackTitle()}");
        await ctx.RespondAsync(embedBuilder);
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }

    public async Task Grab(CommandContext ctx)
    {
        if (Connection.CurrentState.CurrentTrack == null)
        {
            return;
        }

        MusicTrack currTrack = FindTrack(Connection.CurrentState.CurrentTrack);
        var current = Connection.CurrentState.PlaybackPosition;
        var total = currTrack.LavaLinkTrack.Length;
        var currentTimeIncludeUri = Utility.MakeYouTubeShareUrl(currTrack.LavaLinkTrack.Uri.AbsoluteUri, current);
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.DarkGreen)
            .WithAuthor(Localization.SaveMusic)
            .WithThumbnail(Utility.MakeYouTubeThumbnailUrl(currTrack.LavaLinkTrack.Uri.AbsoluteUri))
            .WithDescription($"[{currTrack.LavaLinkTrack.Title}]({currentTimeIncludeUri})")
            .WithFooter($"{Localization.RequestedBy} : {currTrack.User.UsernameWithDiscriminator}", currTrack.User.AvatarUrl)
            .AddField(new DiscordEmbedField("URL", currentTimeIncludeUri))
            .AddField(new DiscordEmbedField(Localization.SaveTime, $"{current.ToDuration()}/{total.ToDuration()}"));

        await ctx.User.SendMessageAsync(embedBuilder);
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        DiscordEmbedBuilder respondEmbed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.PhthaloGreen)
            .WithAuthor("✅" + Localization.CheckDm);
        await ctx.RespondAsync(respondEmbed);
    }

    private async Task<LavalinkLoadResult?> GetLoadResult(string searchQuery)
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
            return null;
        }

        return loadResult;
    }

    private async Task OnTractStarted(LavalinkGuildConnection connection, TrackStartEventArgs args)
    {
        if (_trackList.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(701, "unknown track started"), "MusicPlayer queue has not track");
            return;
        }

        MusicTrack track = FindTrack(args.Track);
        track.TrackStart();

        connection.Node.Discord.Logger.LogDebug(new EventId(703, "Track Start"), $"Track Started {args.Track.Title}");
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithAuthor(Localization.NowPlaying)
            .WithDescription(track.GetTrackTitle())
            .AddField(new DiscordEmbedField(name: Localization.RequestedBy, value: $"{track.User.Mention}", inline: true))
            .AddField(new DiscordEmbedField(name: Localization.Duration, value: track.LavaLinkTrack.Length.ToDuration(), inline: true));

        if (_trackStartMessage != null)
        {
            await _trackStartMessage.DeleteAsync();
        }

        _trackStartMessage = await track.Channel.SendMessageAsync(embedBuilder.Build());
    }

    private async Task OnTrackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs args)
    {
        if (_trackList.Count == 0)
        {
            connection.Node.Discord.Logger.LogError(new EventId(702, "queue already empty"), "track finished but MusicPlayer queue is already empty");
            return;
        }

        if (args.Reason != TrackEndReason.Replaced)
        {
            var trackPair = GetNextTrack(args.Track);

            await trackPair.Key.TrackFinished();
            
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
        MusicTrack? foundTrack = null;
        foreach (var keyValuePair in _trackList)
        {
            foundTrack = keyValuePair.Value.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier);
            if (foundTrack != null)
            {
                break;
            }
        }

        if (foundTrack == null)
        {
            throw new Exception("found track is null");
        }

        return foundTrack;
    }

    private KeyValuePair<MusicTrack, MusicTrack?> GetNextTrack(LavalinkTrack track)
    {
        Connection.Node.Discord.Logger.LogDebug(new EventId(704, "Track Finished"), $"Track Started {track.Title}");

        MusicTrack? foundTrack = null;
        foreach (var keyValuePair in _trackList)
        {
            foundTrack = keyValuePair.Value.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Identifier);
            if (foundTrack != null && keyValuePair.Value.Remove(foundTrack))
            {
                break;
            }
        }
        
        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                if (tracks.Count == 0)
                {
                    _trackList.Remove(i);
                }
            }
        }

        if (foundTrack == null)
        {
            throw new Exception("found track is null");
        }

        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                return new KeyValuePair<MusicTrack, MusicTrack?>(foundTrack, tracks.First());
            }
        }

        return new KeyValuePair<MusicTrack, MusicTrack?>(foundTrack, null);
    }
}