﻿using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DiscordBot.Music;
using DiscordBot.Resource;
using Microsoft.Extensions.Logging;
using CommandContext = DisCatSharp.CommandsNext.CommandContext;

namespace DiscordBot.Commands
{
    public class MusicModules : BaseCommandModule
    {
        public MusicModules(Dictionary<DiscordGuild, MusicPlayer> musicPlayers)
        {
            _musicPlayers = musicPlayers;
        }

        private readonly Dictionary<DiscordGuild, MusicPlayer> _musicPlayers;

        [Command, Aliases("p", "재생")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Play(ctx, search, 0);
        }
        
        [Command, Aliases("lp", "긴재생")]
        public async Task LongPlay(CommandContext ctx, [RemainingText] string search)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Play(ctx, search, 1);
        }
        
        [Command, Aliases("bgm")]
        public async Task BackGroundMusic(CommandContext ctx, [RemainingText] string search)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Play(ctx, search, 2);
        }

        [Command, Aliases("l", "음악중지")]
        public async Task Leave(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Leave(ctx);
        }

        [Command, Aliases("q", "노래리스트")]
        public async Task Queue(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Queue(ctx);
        }

        [Command, Aliases("np", "현재곡")]
        public async Task NowPlaying(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.NowPlaying(ctx);
        }

        [Command, Aliases("일시정지")]
        public async Task Pause(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Pause(ctx);
        }

        [Command, Aliases("재개")]
        public async Task Resume(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Resume(ctx);
        }

        [Command, Aliases("시간넘기기")]
        public async Task Seek(CommandContext ctx, [RemainingText] string positionString)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
   
            await player.Seek(ctx, positionString);
        }
        
        [Command, Aliases("s", "스킵")]
        public async Task Skip(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx, false);
            if (player == null)
            {
                return;
            }

            await player.Skip(ctx);
        }


        [Command, Aliases("r", "삭제")]
        public async Task Remove(CommandContext ctx, string indexString)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx, false);
            if (player == null)
            {
                return;
            }

            await player.Remove(ctx,indexString);
        }

        [Command, Aliases("그랩")] 
        public async Task Grab(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx, false);
            if (player == null)
            {
                return;
            }

            await player.Grab(ctx);
        }
        
        [Command, Aliases("m", "이동")]
        public async Task Move(CommandContext ctx, string indexString, string moveIndexString)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx, false);
            if (player == null)
            {
                return;
            }

            await player.Move(ctx, indexString, moveIndexString);
        }
        
        [Command, Aliases("sp", "재생속도")]
        public async Task Speed(CommandContext ctx, [RemainingText] string speed)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Speed(ctx, speed);
        }

        private async Task<MusicPlayer?> GetMusicPlayer(CommandContext ctx, bool notCreating = true)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync(Localization.NotInChannel);
                return null;
            }

            MusicPlayer? player;
            if (_musicPlayers.TryGetValue(ctx.Guild, out MusicPlayer? musicPlayer))
            {
                player = musicPlayer;

                if (player.Connection.Channel != ctx.Member.VoiceState?.Channel)
                {
                    await ctx.RespondAsync(String.Format(Localization.NotInSameChannel, player.Connection.Discord.CurrentUser.Username));
                    return null;
                }

                return player;
            }

            if (notCreating)
            {
                player = await CreateMusicPlayer(ctx);
                return player;
            }

            return null;
        }

        private async Task<MusicPlayer?> CreateMusicPlayer(CommandContext ctx)
        {
            return await CreateMusicPlayer(ctx.Client, ctx.Member.VoiceState.Channel, _musicPlayers);
        }
        public static async Task<MusicPlayer?> CreateMusicPlayer(DiscordClient client, DiscordChannel channel, Dictionary<DiscordGuild, MusicPlayer> musicPlayers)
        {
            var musicPlayer = await CreateMusicPlayer(client, channel);
            if (musicPlayer != null)
            {
                musicPlayer.Connection.Session.GuildPlayerDestroyed += (sender, args) =>
                {
                    musicPlayers.Remove(args.Guild);
                    return Task.CompletedTask;
                };
                musicPlayers.TryAdd(channel.Guild, musicPlayer);
            }

            return musicPlayer;
        }
        
        private static async Task<MusicPlayer?> CreateMusicPlayer(DiscordClient client, DiscordChannel channel)
        {
            LavalinkExtension? lava = client.GetLavalink();
            if (lava == null)
            {
                client.Logger.LogError(new EventId(200, "LavaLinkExtension"), null, "LavaLinkExtension null");
                return null;
            }

            var conn = lava.GetGuildPlayer(channel.Guild);
            if (conn == null)
            {
                var session = lava.ConnectedSessions.Values.First();
                await session.ConnectAsync(channel);
                conn = lava.GetGuildPlayer(channel.Guild);
                if (conn == null)
                {
                    client.Logger.LogError(new EventId(202, "LavaLinkGuildConnection"), null,
                        "LavaLinkGuildConnection null");
                    return null;
                }
            }

            MusicPlayer musicPlayer = new MusicPlayer(conn);
            return musicPlayer;
        }
    }
}