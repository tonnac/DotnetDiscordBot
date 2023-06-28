using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Enums;
using DiscordBot.Yacht;

namespace DiscordBot.Commands
{
    public class YachtModules : BaseCommandModule
    {
        private static readonly Dictionary<ulong, YachtGame?> CurrPlayingYachtChannels = new();

        public static void AddChannel(ulong channelId, YachtGame? yachtGame)
        {
            CurrPlayingYachtChannels.TryAdd(channelId, yachtGame);
        }

        public static void RemoveChannel(ulong channelId)
        {
            CurrPlayingYachtChannels.Remove(channelId);
        }

        private static bool IsYachtChannel(ulong channelId)
        {
            return CurrPlayingYachtChannels.ContainsKey(channelId);
        }

        private static bool IsYachtPlayer(ulong channelId, ulong userId)
        {
            var user1P = CurrPlayingYachtChannels[channelId]?._1P;
            var user2P = CurrPlayingYachtChannels[channelId]?._2P;
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
            ctx.Client.MessageReactionAdded += newGame.DiceTrayMessageReactionAdded;
            ctx.Client.MessageReactionRemoved += newGame.DiceTrayMessageReactionRemoved;
            ctx.Client.MessageReactionAdded += newGame.ScoreBoardMessageReactionAdded;
            ctx.Client.MessageReactionRemoved += newGame.ScoreBoardMessageReactionRemoved;
            ctx.Client.ThreadDeleted += newGame.ThreadDeleted;
            if (CurrPlayingYachtChannels.TryAdd(newGame._yachtChannel.Id, newGame))
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
            
            if ( CurrPlayingYachtChannels[ctx.Channel.Id]  != null &&  CurrPlayingYachtChannels[ctx.Channel.Id]?._2P == null)
            {
                CurrPlayingYachtChannels[ctx.Channel.Id]!._2P = ctx.User;
                await ctx.RespondAsync($"{ctx.User.Mention}:HERE COMES A NEW CHALLENGER");
                await CurrPlayingYachtChannels[ctx.Channel.Id]!.RefreshGameBoard(ctx.Client);
            }
            else
            {
                await ctx.RespondAsync("풀방임");
            }
        }

        [Command, Aliases("ys")]
        public async Task YachtSurrender(CommandContext ctx)
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

            await CurrPlayingYachtChannels[ctx.Channel.Id]?.Surrender(ctx.Client, ctx)!;
            await ctx.RespondAsync("야추 종료");
        }

        [Command, Aliases("yc")]
        public async Task YachtChoice(CommandContext ctx, [RemainingText] string? tempCommand)
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

            if (CurrPlayingYachtChannels.TryGetValue(ctx.Channel.Id, out YachtGame? yachtGame))
            {
                if (yachtGame?.CurrPlayer?.Id != ctx.User.Id)
                {
                    await ctx.RespondAsync("니차례 아님");
                    return;
                }

                if (Enum.TryParse(tempCommand, out EYachtPointType eYachtPointType))
                {
                    await yachtGame.ChoicePoint(ctx.Client, eYachtPointType);
                }
            }
        }
    }
}