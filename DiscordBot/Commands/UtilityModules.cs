using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Channels;
using DiscordBot.Core;
using DiscordBot.Resource;
using OpenAI_API;
using OpenAI_API.Chat;

namespace DiscordBot.Commands
{
    public sealed class UtilityModules : BaseCommandModule
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public UtilityModules(OpenAIAPI openAIAPI, DiscordMessageHandler messageHandler)
        {
            _openAiApi = openAIAPI;
            _messageHandler = messageHandler;
        }

        private readonly DiscordMessageHandler _messageHandler;
        private readonly OpenAIAPI _openAiApi;
        
        [Command]
        [RequireBotPermissions(Permissions.ManageMessages), RequirePermissions(Permissions.Administrator)]
        public async Task MusicChannel(CommandContext ctx)
        {
            string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.Music);
            string emoji = VEmoji.RedCrossMark;
            if ("+" == result)
            {
                emoji = VEmoji.GreenCheckBox;
            }
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }

        [Command]
        [RequireBotPermissions(Permissions.ManageMessages), RequirePermissions(Permissions.Administrator)]
        public async Task NoticeChannel(CommandContext ctx)
        {
            string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.Notice);
            string emoji = VEmoji.RedCrossMark;
            if ("+" == result)
            {
                emoji = VEmoji.GreenCheckBox;
            }
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }

        [Command, Aliases("g")]
        public async Task Gpt(CommandContext ctx, [RemainingText] string chatMessage)
        {
            var result = await _openAiApi.Chat.CreateChatCompletionAsync(new ChatRequest
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 2048,
                Messages = new []
                {
                    new ChatMessage(ChatMessageRole.User, chatMessage)
                }
            });

            var reply = result.Choices[0].Message;
            await ctx.RespondAsync($"{reply.Content.Trim()}");
        }
        
        
        [Command]
        [RequireBotPermissions(Permissions.ManageMessages), RequirePermissions(Permissions.Administrator)]
        public async Task DisableChat(CommandContext ctx)
        {
            if ((ctx.Member.Permissions & Permissions.Administrator) == 0)
            {
                DiscordMessage? message = await ctx.RespondAsync(Localization.Permission);
                if (message != null && _messageHandler.IsDisableChatChannel(ctx.Channel))
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        await message.DeleteAsync();
                    });
                }
                return;
            }

            string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.DisableChat);
            string emoji = VEmoji.RedCrossMark;
            if ("+" == result)
            {
                emoji = VEmoji.GreenCheckBox;
            }
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }

        [Command, Aliases("wd", "와우주사위")]
        public async Task WOWDice(CommandContext ctx, [RemainingText] string? diceCommand)
        {
            var rand = new Random();
            if (string.IsNullOrEmpty(diceCommand))
            {
                int value = rand.Next(1, 101);
                
                await ctx.RespondAsync(string.Format(Localization.Dice, ctx.Member.Mention, Convert.ToString(value), 100));
                return;
            }
            string[] diceNums = diceCommand.Split(' ');
            int? result = null;
            if (diceNums.Length == 1)
            {
                if (Int32.TryParse(diceNums[0], out int num))
                    result = rand.Next(1,num+1);
            }
            else if (diceNums.Length == 2)
            {
                if (Int32.TryParse(diceNums[0], out int minNum) && Int32.TryParse(diceNums[1], out int maxNum))
                    if (minNum < maxNum)
                        result = rand.Next(minNum, maxNum+1);
            }

            if (result.HasValue)
                await ctx.RespondAsync(string.Format(Localization.Dice, ctx.Member.Mention, result, diceNums.Length == 1 ? $"{diceNums[0]}" : $"{diceNums[0]}~{diceNums[1]}"));
            else
                await ctx.RespondAsync(Localization.wrongDice);
        }
        
        [Command, Aliases("d", "주사위"), Cooldown(1, 300, CooldownBucketType.User)]
        public async Task Dice(CommandContext ctx, [RemainingText] string? diceCommand)
        {
            var rand = new Random();
            if (string.IsNullOrEmpty(diceCommand))
            {
                int value = rand.Next(1, 101);

                string name = Utility.GetMemberDisplayName(ctx.Member);
                
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://media.tenor.com/zk3sVpc7OGkAAAAi/dice-roll-the-dice.gif")
                    .WithColor(DiscordColor.DarkGreen)
                    .WithAuthor("[" + $"{1}~{100}" + "]")
                    .AddField(new DiscordEmbedField("👋 " + name, "🎲 " + Convert.ToString(value), true));
                
                var message = await ctx.RespondAsync(embedBuilder);

                if (message != null && value is 1 or 100)
                {
                    await message.PinAsync();
                }
                
                return;
            }
            string[] diceNums = diceCommand.Split(' ');
            int? result = null;
            if (diceNums.Length == 1)
            {
                if (Int32.TryParse(diceNums[0], out int num))
                    result = rand.Next(1,num+1);
            }
            else if (diceNums.Length == 2)
            {
                if (Int32.TryParse(diceNums[0], out int minNum) && Int32.TryParse(diceNums[1], out int maxNum))
                    if (minNum < maxNum)
                        result = rand.Next(minNum, maxNum+1);
            }

            if (result.HasValue)
            {
                string name = Utility.GetMemberDisplayName(ctx.Member);
                
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://media.tenor.com/zk3sVpc7OGkAAAAi/dice-roll-the-dice.gif")
                    .WithColor(DiscordColor.DarkGreen)
                    .WithAuthor(diceNums.Length == 1 ? "[1" + $"~{diceNums[0]}" + "]" : "[" + $"{diceNums[0]}~{diceNums[1]}" + "]")
                    //.WithDescription(currTrack.GetTrackTitle())
                    //.AddField(new DiscordEmbedField(Localization.Roller, ctx.Member.Mention))
                    .AddField(new DiscordEmbedField("👋 " + name, "🎲 " + Convert.ToString(result)));
                
                await ctx.RespondAsync(embedBuilder);
            }
            else
                await ctx.RespondAsync(Localization.wrongDice);
        }

        [Command, Aliases("Exit")]
        [RequireBotPermissions(Permissions.KickMembers)]
        public async Task DoExit(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync(Localization.NotInChannel);
                return;
            }
            
            await ctx.Member.DisconnectFromVoiceAsync();
        }
    }
}