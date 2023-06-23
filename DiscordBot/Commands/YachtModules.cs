using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DiscordBot.Yacht;

namespace DiscordBot.Commands
{
    public class YachtModules : BaseCommandModule
    {
        private readonly Dictionary<ulong, YachtGame?> _currPlayingYachtChannels = new();
        private readonly DiscordClient _client;

        public YachtModules(DiscordClient client)
        {
            _client = client;
        }

        private bool IsYachtChannel(ulong channelId)
        {
            return _currPlayingYachtChannels.ContainsKey(channelId);
        }

        private bool IsYachtPlayer(ulong channelId, ulong userId)
        {
            var user1P = _currPlayingYachtChannels[channelId]?._1P;
            var user2P = _currPlayingYachtChannels[channelId]?._2P;
            return (user1P != null && user1P.Id == userId) || (user2P != null && user2P.Id == userId);
        }

        [Command, Aliases("yo")]
        public async Task YachtOpen(CommandContext ctx)
        {
            YachtGame newGame = new()
            {
                _1P = ctx.User,
                _yachtChannel = await ctx.Channel.CreateThreadAsync($"{ctx.User.Username}님의 야추방", ThreadAutoArchiveDuration.OneDay),
            };
            _client.MessageReactionAdded += newGame.DiceTrayMessageReactionAdded;
            _client.MessageReactionRemoved += newGame.DiceTrayMessageReactionRemoved;
            _client.MessageReactionAdded += newGame.ScoreBoardMessageReactionAdded;
            _client.MessageReactionRemoved += newGame.ScoreBoardMessageReactionRemoved;
            if (_currPlayingYachtChannels.TryAdd(newGame._yachtChannel.Id, newGame))
            {
                await newGame._yachtChannel.SendMessageAsync($"{newGame._1P.Mention}님이 야추방을 만드셨습니다.");
            }

        }

        [Command, Aliases("yj")]
        public async Task YachtJoin(CommandContext ctx)
        {
            if (!IsYachtChannel(ctx.Channel.Id))
            {
                await ctx.RespondAsync("야추방이 아닌데요?");
                return;
            } 
            
            if ( _currPlayingYachtChannels[ctx.Channel.Id]  != null &&  _currPlayingYachtChannels[ctx.Channel.Id]?._2P == null)
            {
                _currPlayingYachtChannels[ctx.Channel.Id]!._2P = ctx.User;
                await ctx.RespondAsync($"{ctx.User.Mention}:HERE COMES A NEW CHALLENGER");
            }
            else
            {
                await ctx.RespondAsync("풀방임");
            }
        }

        [Command, Aliases("ys")]
        public async Task YachtStop(CommandContext ctx)
        {
            if (!IsYachtChannel(ctx.Channel.Id))
            {
                await ctx.RespondAsync("야추방이 아닌데요?");
                return;
            } 

            if (!IsYachtPlayer(ctx.Channel.Id, ctx.User.Id))
            {
                await ctx.RespondAsync("플레이중이 아니신데요?");
                return;
            }
            _client.MessageReactionAdded -= _currPlayingYachtChannels[ctx.Channel.Id]!.DiceTrayMessageReactionAdded;
            _client.MessageReactionRemoved -= _currPlayingYachtChannels[ctx.Channel.Id]!.DiceTrayMessageReactionRemoved;
            _client.MessageReactionAdded -= _currPlayingYachtChannels[ctx.Channel.Id]!.ScoreBoardMessageReactionAdded;
            _client.MessageReactionRemoved -= _currPlayingYachtChannels[ctx.Channel.Id]!.ScoreBoardMessageReactionRemoved;
            _currPlayingYachtChannels[ctx.Channel.Id]?.GameSettle();
            _currPlayingYachtChannels.Remove(ctx.Channel.Id);
            await ctx.RespondAsync("야추 종료");
        }
        [Command, Aliases("ync")]
        public async Task YachtNextCommand(CommandContext ctx)
        {
            if (!IsYachtChannel(ctx.Channel.Id))
            {
                await ctx.RespondAsync("야추방이 아닌데요?");
                return;
            } 

            if (!IsYachtPlayer(ctx.Channel.Id, ctx.User.Id))
            {
                await ctx.RespondAsync("플레이중이 아니신데요?");
                return;
            }

            if (_currPlayingYachtChannels.TryGetValue(ctx.Channel.Id, out YachtGame? yachtGame))
            {
                if (yachtGame?.CurrPlayer?.Id != ctx.User.Id)
                {
                    await ctx.RespondAsync("니차례 아님");
                    return;
                }

                await yachtGame.RefreshGameBoard(ctx.Client);
            }

        }
    }
}