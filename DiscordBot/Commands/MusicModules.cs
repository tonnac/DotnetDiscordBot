using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
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

        [Command, Aliases("p", "p1")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }

            await player.Play(ctx, search);
        }

        [Command, Aliases("l")]
        public async Task Leave(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
            await player.Leave();
        }
        
        [Command, Aliases("q")]
        public async Task Queue(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
            await player.Queue(ctx);
        }
        
        [Command, Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
            await player.NowPlaying(ctx);
        }
        
        [Command]
        public async Task Pause(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
            await player.Pause(ctx);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            MusicPlayer? player = await GetMusicPlayer(ctx);
            if (player == null)
            {
                return;
            }
            await player.Resume(ctx);
        }
    
        private async Task<MusicPlayer?> GetMusicPlayer(CommandContext ctx)
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
                    await ctx.RespondAsync(String.Format(Localization.NotInSameChannel, player.Connection.Node.Discord.CurrentUser.Username));
                    return null;
                }
            }
            else
            {
                player = await CreateMusicPlayer(ctx);
            }

            return player;
        }

        private async Task<MusicPlayer?> CreateMusicPlayer(CommandContext ctx)
        {
            LavalinkExtension? lava = ctx.Client.GetLavalink();
            if (lava == null)
            {
                ctx.Client.Logger.LogError(new EventId(200, "LavaLinkExtension"), null, "LavaLinkExtension null");
                return null;
            }
            LavalinkNodeConnection? node = lava.ConnectedNodes.Values.First();
            if (node == null)
            {
                ctx.Client.Logger.LogError(new EventId(201, "LavaLinkNodeConnection"), null, "LavaLinkNodeConnection null");
                return null;
            }
            
            LavalinkGuildConnection? conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
            if (conn == null)
            {
                ctx.Client.Logger.LogError(new EventId(202, "LavaLinkGuildConnection"), null, "LavaLinkGuildConnection null");
                return null;
            }
            
            conn.ChannelDisconnected += (connection) =>
            {
                _musicPlayers.Remove(connection.Guild);
            };
            
            MusicPlayer musicPlayer = new MusicPlayer(conn);
            _musicPlayers.TryAdd(ctx.Guild, musicPlayer);
            return musicPlayer;
        }
    }
}