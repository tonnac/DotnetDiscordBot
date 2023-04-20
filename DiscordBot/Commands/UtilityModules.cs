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
            { "Utility", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRcjtzAEQMDdcnf_VmHJ9RcQSzP50VulGw7lazLNV189n-PsSEvOAYJWaaObqTReXMr7s4&usqp=CAU" }
        };

        [Command, Aliases("h")]
        public async Task Help(CommandContext ctx)
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
                select groupData;
            
            foreach (var copyCommand in copyCommands)
            {
                var commandsString = string.Join("\n", copyCommand.Select(x => $"`{x.Name}`{(x.Aliases.Count == 0 ? "" : $"(**{string.Join(", ", x.Aliases.Select((alias => alias)))}**)")}: {FindLocal(x.Name + "_Description")}"));
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
            if ((ctx.Member.Permissions & Permissions.ManageMessages) != 0)
            {
                await ctx.RespondAsync(Localization.Permission);
                return;
            }
            await _messageHandler.ToggleChannel(ctx);
        }
    }
}