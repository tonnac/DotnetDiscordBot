using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
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
        public UtilityModules(DiscordClient client, OpenAIAPI openAIAPI, DiscordMessageHandler messageHandler)
        {
            _client = client;
            _openAiApi = openAIAPI;
            _messageHandler = messageHandler;
        }

        private readonly DiscordClient _client;
        private readonly DiscordMessageHandler _messageHandler;
        private readonly OpenAIAPI _openAiApi;
        private readonly Dictionary<string, string?> _moduleThumbnail = new ()
        {
            { "Music", "https://daily.jstor.org/wp-content/uploads/2023/01/good_times_with_bad_music_1050x700.jpg" }, 
            { "Lol", "https://yt3.googleusercontent.com/_nlyMx8RWF3h2aG8PslnqMobecnco8XjOBki7dL_nayZYfNxxFdPSp2PpxUytjN4VmHqb4XPtA=s900-c-k-c0x00ffffff-no-rj" }, 
            { "Boss", "https://oldschoolroleplaying.com/wp-content/uploads/2020/01/Skull-Cave-Entrance.jpg" },
            { "Fishing", "https://i.pinimg.com/550x/ec/4f/da/ec4fda8ea3d3a52ba561e9e54d3c81fc.jpg" },
            { "Gamble", "https://img.freepik.com/free-photo/casino-games-backdrop-banner-3d-illustration-with-casino-elements-craps-roulette-and-poker-cards-generative-ai_91128-2286.jpg?w=2000" },
            { "UserGameInfo", "https://cdn-icons-png.flaticon.com/512/943/943579.png" },
            { "Utility", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRcjtzAEQMDdcnf_VmHJ9RcQSzP50VulGw7lazLNV189n-PsSEvOAYJWaaObqTReXMr7s4&usqp=CAU" }
            
        };

        [Command, Aliases("h", "도움말"), Cooldown(1, 60, CooldownBucketType.User)]
        public async Task Help(CommandContext ctx)
        {
            await Help_Private(ctx, new List<string>{ "Music", "Lol", "Utility" });
        }
        
        [Command, Aliases("bh", "보스도움말"), Cooldown(1, 60, CooldownBucketType.User)]
        public async Task BossHelp(CommandContext ctx)
        {
            await Help_Private(ctx, new List<string>{ "Boss" });
        }
        
        [Command, Aliases("fh", "낚시도움말"), Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FishingHelp(CommandContext ctx)
        {
            await Help_Private(ctx, new List<string>{ "Fishing" });
        }
        
        [Command, Aliases("gh", "도박도움말"), Cooldown(1, 60, CooldownBucketType.User)]
        public async Task GambleHelp(CommandContext ctx)
        {
            await Help_Private(ctx, new List<string>{ "Gamble" });
        }
        
        [Command, Aliases("uih", "유저정보도움말"), Cooldown(1, 60, CooldownBucketType.User)]
        public async Task UserInfoHelp(CommandContext ctx)
        {
            await Help_Private(ctx, new List<string>{ "UserGameInfo" });
        }

        private async Task Help_Private(CommandContext ctx, List<string> category)
        {
            CommandsNextExtension? commandNext = _client.GetCommandsNext();
            if (commandNext == null)
            {
                return;
            }

            string FindLocal(string name)
            {
                TypeInfo typeinfo = typeof(Localization).GetTypeInfo();
                foreach (PropertyInfo propertyInfo in typeinfo.DeclaredProperties)
                {
                    if (propertyInfo.Name == name && propertyInfo.GetValue(typeinfo) is string)
                    {
                        return (string)propertyInfo.GetValue(typeinfo)!;
                    }
                }

                return "";
            }

            var copyCommands =
                from pair in commandNext.RegisteredCommands
                where pair.Key == pair.Value.Name
                orderby pair.Value.Name
                group pair.Value by pair.Value.Module.ModuleType.Name.Split("Modules")[0]
                into groupData
                where category.Contains(groupData.Key)
                select groupData;
            
            foreach (var copyCommand in copyCommands)
            {
                var commandsString = string.Join("", copyCommand.Select(x => x.Aliases.Count == 0 ? "" : $"`{x.Name}`{(x.Aliases.Count == 0 ? "" : $"(**{string.Join(", ", x.Aliases.Select((alias => alias)))}**)")}:\n- {FindLocal(x.Name + "_Description")}\n"));
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithAuthor(copyCommand.Key)
                    .WithColor(DiscordColor.Azure)
                    .WithTimestamp(DateTime.Now)
                    .WithDescription(commandsString);

                if (_moduleThumbnail.TryGetValue(copyCommand.Key, out string? thumbnailUrl))
                {
                    embedBuilder.WithThumbnail(thumbnailUrl);
                }
        
                await ctx.Channel.SendMessageAsync(embedBuilder.Build());
            }
            
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
        [RequireBotPermissions(Permissions.ManageMessages)]
        public async Task ImageOnly(CommandContext ctx)
        {
            if ((ctx.Member.Permissions & Permissions.Administrator) == 0)
            {
                DiscordMessage? message = await ctx.RespondAsync(Localization.Permission);
                if (message != null && _messageHandler.IsImageOnlyChannel(ctx.Channel))
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        await message.DeleteAsync();
                    });
                }
                return;
            }
            await _messageHandler.ToggleChannel(ctx);
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