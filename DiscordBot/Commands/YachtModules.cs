using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Yacht;

namespace DiscordBot.Commands
{
    public class YachtModules : BaseCommandModule
    {
        private readonly Dictionary<ulong, YachtGame?> _currPlayingYachtChannels = new();

        [Command]
        public async Task StartYacht(CommandContext ctx)
        {
            YachtGame newPlayer = new()
            {
                _1P = ctx.User
            };
            DiscordThreadChannel? yachtChannel = await ctx.Channel.CreateThreadAsync($"{newPlayer._1P.Username}님의 야추방", ThreadAutoArchiveDuration.OneDay);
            if (_currPlayingYachtChannels.TryAdd(yachtChannel.Id, newPlayer))
            {
                await yachtChannel.SendMessageAsync($"{newPlayer._1P.Mention}님이 야추방을 만드셨습니다.");
            }

        }

        [Command]
        public async Task JoinYacht(CommandContext ctx)
        {
            if (_currPlayingYachtChannels.TryGetValue(ctx.Channel.Id,out YachtGame? player))
            {
                if (player != null && player._2P == null)
                {
                    player._2P = ctx.User;
                    _currPlayingYachtChannels[ctx.Channel.Id] = player; 
                    await ctx.RespondAsync($"{ctx.User.Mention}:HERE COMES A NEW CHALLENGER");
                }
                else
                {
                    await ctx.RespondAsync("풀방임");
                }
            }
            else
            {
                await ctx.RespondAsync("야추방이 아닌데요?");
            }
        }

        [Command]
        public async Task StopYacht(CommandContext ctx)
        {
            if (!_currPlayingYachtChannels.ContainsKey(ctx.Channel.Id))
            {
                await ctx.RespondAsync("야추방이 아닌데요?");
                return;
            }

            var user1P = _currPlayingYachtChannels[ctx.Channel.Id]?._1P;
            var user2P = _currPlayingYachtChannels[ctx.Channel.Id]?._2P;
            if ((user1P != null && user1P.Id == ctx.User.Id) || (user2P != null && user2P.Id == ctx.User.Id))
            {
                _currPlayingYachtChannels.Remove(ctx.Channel.Id);
                await ctx.RespondAsync("야추 종료");
            }
            else
            {
                await ctx.RespondAsync("플레이중이 아니신데요?");
            }
        }
    }
}