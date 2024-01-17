using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.EventArgs;
using DiscordBot.Resource;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Music;

public class MusicPlayer
{
    private readonly Dictionary<int, List<MusicTrack>> _trackList = new ();
    public readonly LavalinkGuildPlayer Connection;
    private static readonly int QueuePagePerCount = 10;
    private static readonly int MaxTrackCount = 3;
    private DiscordMessage? _trackStartMessage;

    public MusicPlayer(LavalinkGuildPlayer connection)
    {
        Connection = connection;

        Connection.TrackStarted += OnTractStarted;
        Connection.TrackEnded += OnTrackFinished;
    }

    public async Task Play(Playlist playlist)
    {
        if (Connection.IsConnected == false)
        {
            return;
        }

        foreach (var keyValuePair in playlist.List)
        {
            foreach (var playingMusic in keyValuePair.Value)
            {
                var loadResult = await GetLoadResult(playingMusic.Url);

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

                var musicTrack = MusicTrack.CreateMusicTrack(member, channel, loadResult.GetResultAs<LavalinkTrack>(), playingMusic);
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

        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? musicTracks))
            {
                if (musicTracks.Count > 0)
                {
                    await Connection.PlayPartialAsync(musicTracks[0].LavaLinkTrack, musicTracks[0].TimeSpan, musicTracks[0].LavaLinkTrack.Info.Length);
                    break;
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
                    Url = musicTrack.LavaLinkTrack.Info.Uri.ToString(),
                    RequestChannel = musicTrack.Channel.Id,
                    MemberId = musicTrack.User.Id,
                    Time = musicTrack.TimeSpan,
                    PlayListIndex = musicTrack.TrackIndex,
                    AddedTime = musicTrack.AddedTime,
                    StartTime = musicTrack.StartTime
                };

                if (musicTrack.LavaLinkTrack.Info.Identifier == Connection.Player.Track?.Info.Identifier)
                {
                    playingMusic.Time = Connection.Player.PlayerState.Position;
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
        LavalinkTrackLoadingResult? loadResult = await GetLoadResult(searchQuery);

        if (loadResult == null)
        {
            return;
        }
        
        List<MusicTrack> addedTrack = loadResult.LoadType == LavalinkLoadResultType.Playlist ? loadResult.GetResultAs<LavalinkPlaylist>().Tracks.Select(track => MusicTrack.CreateMusicTrack(ctx, track, index)).ToList() : new List<MusicTrack> {MusicTrack.CreateMusicTrack(ctx, loadResult.GetResultAs<List<LavalinkTrack>>().First(), index)};

        addedTrack.RemoveAll(track =>
        {
            for (int i = 0; i < MaxTrackCount; i++)
            {
                if (_trackList.TryGetValue(i, out List<MusicTrack>? trackList))
                {
                    if (trackList.Find(musicTrack =>
                            musicTrack.LavaLinkTrack.Info.Identifier == track.LavaLinkTrack.Info.Identifier) != null)
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

            if (Connection.Player.Track == null)
            {
                await Connection.PlayAsync(newTrack.LavaLinkTrack);
                return;
            }

            for (int i = index + 1; i < MaxTrackCount; i++)
            {
                if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
                {
                    if (tracks.First().LavaLinkTrack.Info.Identifier == Connection.Player.Track.Info.Identifier)
                    {
                        tracks.First().TimeSpan = Connection.Player.PlayerState.Position;
                        await Connection.PlayAsync(newTrack.LavaLinkTrack);
                        return;
                    }
                }
            }
            
            var embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(Localization.addedQueue)
                .WithDescription(newTrack.GetTrackTitle())
                .AddField(new DiscordEmbedField(Localization.RequestedBy, newTrack.User.Mention, true))
                .AddField(new DiscordEmbedField(Localization.Duration, newTrack.LavaLinkTrack.Info.Length.ToDuration().InlineCode(), true))
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
                totalLength += musicTrack.LavaLinkTrack.Info.Length;
            }

            copyTrack.RemoveAt(0);

            List<List<MusicTrack>> chunkedTracks = copyTrack.Partition(QueuePagePerCount);

            DiscordEmbedBuilder EmbedBuilder(int index) =>
                new DiscordEmbedBuilder().WithColor(DiscordColor.Orange)
                    .WithAuthor(Localization.Queue)
                    .WithDescription($"{Localization.NowPlaying.Bold()}: {currentTrack.GetTrackTitle()} \n\n{Localization.UpNext.Bold()} \n {string.Join("\n", chunkedTracks[index].Select((track, i) => $"{Convert.ToString(QueuePagePerCount * index + i + 1).InlineCode()} {track.GetTrackTitle()} \n{track.LavaLinkTrack.Info.Length.ToDuration()} | {Localization.RequestedBy.Bold()} {track.User.Mention}"))}")
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
        if (Connection.Player.Track == null)
        {
            return;
        }

        MusicTrack currTrack = FindTrack(Connection.Player.Track?.Info);

        var current = Connection.Player.PlayerState.Position;
        var total = currTrack.LavaLinkTrack.Info.Length;

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
        if (Connection.Player.Track != null)
        {
            await Connection.PauseAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }

    public async Task Resume(CommandContext ctx)
    {
        if (Connection.Player.Track != null)
        {
            await Connection.ResumeAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }

    public async Task Seek(CommandContext ctx, string position)
    {
        if (Connection.Player.Track == null)
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
            Connection.Discord.Logger.LogError(new EventId(705, "invalid seek command"), e.Message);
            await ctx.RespondAsync(String.Format(Localization.seek_Usage, Config.Prefix));
        }
    }

    public async Task Skip(CommandContext ctx)
    {
        if (Connection.Player.Track != null)
        {
            await Connection.StopAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }

    public async Task Remove(CommandContext ctx, string indexString)
    {
        if (Connection.Player.Track == null)
        {
            await ctx.RespondAsync(Localization.ErrorNotQueue);
            return;
        }

        int index = int.Parse(indexString);

        var copyTracks = new List<MusicTrack>();
        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                copyTracks.AddRange(tracks);
            }
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
    
    public async Task Move(CommandContext ctx, string indexString, string moveIndexString)
    {
        if (Connection.Player.Track == null)
        {
            await ctx.RespondAsync(Localization.ErrorNotQueue);
            return;
        }

        int inputFindMusicIndex = int.Parse(indexString);
        int inputMoveMusicIndex = int.Parse(moveIndexString);

        if (inputFindMusicIndex < 1)
        {
            await ctx.RespondAsync(String.Format(Localization.move_Usage, Config.Prefix));
            return;
        }

        if (inputMoveMusicIndex == 0)
        {
            await ctx.RespondAsync(Localization.move_MoveIndexWarning.Bold());
            return;
        }

        MusicTrack? foundMusic = null;

        int playListIndex = 0;
        for (int i = 0; i < MaxTrackCount; i++)
        {
            if (_trackList.TryGetValue(i, out List<MusicTrack>? tracks))
            {
                if (inputFindMusicIndex < playListIndex + tracks.Count)
                {
                    int findMusicIndex = inputFindMusicIndex - playListIndex;
                    int moveMusicIndex = inputMoveMusicIndex - playListIndex;
                    foundMusic = tracks[findMusicIndex];

                    if (tracks.ElementAtOrDefault(moveMusicIndex) == null)
                    {
                        moveMusicIndex = Math.Clamp(0, tracks.Count - 1, moveMusicIndex);
                    }

                    tracks.Remove(foundMusic);
                    tracks.Insert(moveMusicIndex, foundMusic);
                    
                    break;
                }
                playListIndex += tracks.Count;
            }
        }

        if (foundMusic == null)
        {
            await ctx.RespondAsync(String.Format(Localization.move_InvalidIndex.Bold(), inputFindMusicIndex));
            return;
        }

        var embedBuilder = new DiscordEmbedBuilder()
            .WithDescription(String.Format(Localization.move_Complete, inputFindMusicIndex.ToString().InlineCode(), inputMoveMusicIndex.ToString().InlineCode(), foundMusic.GetTrackTitle()));
        await ctx.RespondAsync(embedBuilder);
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }

    public async Task Grab(CommandContext ctx)
    {
        if (Connection.CurrentTrack == null)
        {
            return;
        }

        MusicTrack currTrack = FindTrack(Connection.CurrentTrack.Info);
        var current = Connection.Player.PlayerState.Position;
        var total = currTrack.LavaLinkTrack.Info.Length;
        var currentTimeIncludeUri = Utility.MakeYouTubeShareUrl(currTrack.LavaLinkTrack.Info.Uri.AbsoluteUri, current);
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.DarkGreen)
            .WithAuthor(Localization.SaveMusic)
            .WithThumbnail(Utility.MakeYouTubeThumbnailUrl(currTrack.LavaLinkTrack.Info.Uri.AbsoluteUri))
            .WithDescription($"[{currTrack.LavaLinkTrack.Info.Title}]({currentTimeIncludeUri})")
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

    private async Task<LavalinkTrackLoadingResult?> GetLoadResult(string searchQuery)
    {
        LavalinkTrackLoadingResult? loadResult;
        
        if (searchQuery.Contains("https://"))
        {
            loadResult = await Connection.LoadTracksAsync(searchQuery);
        }
        else
        {
            loadResult = await Connection.LoadTracksAsync(LavalinkSearchType.Youtube, searchQuery);
        }

        if (loadResult.LoadType == LavalinkLoadResultType.Error || loadResult.LoadType == LavalinkLoadResultType.Empty)
        {
            return null;
        }

        return loadResult;
    }

    private async Task OnTractStarted(LavalinkGuildPlayer connection, LavalinkTrackStartedEventArgs args)
    {
        if (_trackList.Count == 0)
        {
            connection.Discord.Logger.LogError(new EventId(701, "unknown track started"), "MusicPlayer queue has not track");
            return;
        }

        MusicTrack track = FindTrack(args.Track.Info);
        track.TrackStart();

        connection.Discord.Logger.LogDebug(new EventId(703, "Track Start"), $"Track Started {args.Track.Info}");
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Azure)
            .WithAuthor(Localization.NowPlaying)
            .WithDescription(track.GetTrackTitle())
            .AddField(new DiscordEmbedField(name: Localization.RequestedBy, value: $"{track.User.Mention}", inline: true))
            .AddField(new DiscordEmbedField(name: Localization.Duration, value: track.LavaLinkTrack.Info.Length.ToDuration(), inline: true));

        if (_trackStartMessage != null)
        {
            await _trackStartMessage.DeleteAsync();
        }

        _trackStartMessage = await track.Channel.SendMessageAsync(embedBuilder.Build());
    }

    private async Task OnTrackFinished(LavalinkGuildPlayer connection, LavalinkTrackEndedEventArgs args)
    {
        if (_trackList.Count == 0)
        {
            connection.Discord.Logger.LogError(new EventId(702, "queue already empty"), "track finished but MusicPlayer queue is already empty");
            return;
        }
        
        if (args.Reason != LavalinkTrackEndReason.Replaced)
        {
            var trackPair = GetNextTrack(args.Track.Info);

            await trackPair.Key.TrackFinished();
            
            if (trackPair.Value != null)
            {
                await Connection.PlayPartialAsync(trackPair.Value.LavaLinkTrack, trackPair.Value.TimeSpan, trackPair.Value.LavaLinkTrack.Info.Length);
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

    MusicTrack FindTrack(LavalinkTrackInfo track)
    {
        MusicTrack? foundTrack = null;
        foreach (var keyValuePair in _trackList)
        {
            foundTrack = keyValuePair.Value.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Info.Identifier);
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

    private KeyValuePair<MusicTrack, MusicTrack?> GetNextTrack(LavalinkTrackInfo track)
    {
        Connection.Discord.Logger.LogDebug(new EventId(704, "Track Finished"), $"Track Started {track.Title}");

        MusicTrack? foundTrack = null;
        foreach (var keyValuePair in _trackList)
        {
            foundTrack = keyValuePair.Value.Find(musicTrack => track.Identifier == musicTrack.LavaLinkTrack.Info.Identifier);
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