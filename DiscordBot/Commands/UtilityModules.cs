using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Resource;
using DiscordBot.Database;
using OpenAI_API;
using OpenAI_API.Chat;

namespace DiscordBot.Commands
{
    public sealed class UtilityModules : BaseCommandModule
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public UtilityModules(DiscordClient client, OpenAIAPI openAIAPI)
        {
            _client = client;
            _openAiApi = openAIAPI;
        }

        private readonly DiscordClient _client;
        private readonly OpenAIAPI _openAiApi;
        
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
                (from pair in commandNext.RegisteredCommands
                    where pair.Key == pair.Value.Name
                    select pair.Value)
                .OrderBy((command => command.Name)).ToList();

            var commandsString = string.Join("\n", copyCommands.Select(x => $"`{x.Name}`{(x.Aliases.Count == 0 ? "" : $"(**{string.Join(", ", x.Aliases.Select((alias => alias)))}**)")}: {FindLocal(x.Name + "_Description")}"));
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Azure)
                .WithTimestamp(DateTime.Now)
                .WithDescription(commandsString);
            
            await ctx.Channel.SendMessageAsync(embedBuilder.Build());
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
        public async Task Register(CommandContext ctx)
        {
            var db = new DiscordBotDatabase();
            await db.Connect();

            bool bSuccess = await db.UserRegister(ctx);
                
            if (bSuccess)
            {
                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.WithDescription("Success");
                await ctx.RespondAsync(embedBuilder);
            }
        }
        
        [Command]
        public async Task Delete(CommandContext ctx)
        {
            var db = new DiscordBotDatabase();
            await db.Connect();
            
            var bSuccess = await db.UserDelete(ctx);
            if (bSuccess)
            {
                var embedBuilder = new DiscordEmbedBuilder();
                embedBuilder.WithDescription("Success");
                await ctx.RespondAsync(embedBuilder);
            }
        }
        
        [Command]
        public async Task Aram(CommandContext ctx)
        {
            var db = new DiscordBotDatabase();
            await db.Connect();
            
            var users = await db.GetDatabaseUsers(ctx);
            List<DiscordUser> discordUsers = new List<DiscordUser>(users.Count);
            
            foreach (var databaseUser in users)
            {
                if (ctx.Guild.Members.TryGetValue(databaseUser.userid, out DiscordMember? member))
                {
                    await member.SendMessageAsync("111");
                }
            }
        }
    }
}