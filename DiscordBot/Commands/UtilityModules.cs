﻿using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Core;
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
        public UtilityModules(DiscordClient client, OpenAIAPI openAIAPI, DiscordMessageHandler messageHandler)
        {
            _client = client;
            _openAiApi = openAIAPI;
            _messageHandler = messageHandler;
        }

        private readonly DiscordClient _client;
        private readonly DiscordMessageHandler _messageHandler;
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
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            var user = await database.GetDatabaseUser(ctx.Guild, ctx.User);
            var embedBuilder = new DiscordEmbedBuilder();

            if (user.userid != 0)
            {
                embedBuilder.WithDescription("Member already exist");
            }
            else
            {
                bool bSuccess = await database.UserRegister(ctx);

                if (bSuccess)
                {
                    embedBuilder.WithDescription("Success!");
                }
                else
                {
                    return;
                }
            }
            await ctx.RespondAsync(embedBuilder);
        }
        
        [Command]
        public async Task Delete(CommandContext ctx)
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();

            var user = await database.GetDatabaseUser(ctx.Guild, ctx.User);
            var embedBuilder = new DiscordEmbedBuilder();
            if (user.userid == 0)
            {
                embedBuilder.WithDescription("Member doesn't exist");
            }
            else
            {
                await database.UserDelete(ctx);
                var bSuccess = await database.UserDelete(ctx);
                if (bSuccess)
                {
                    embedBuilder.WithDescription("Success!");
                }
                else
                {
                    return;
                }
            }
            await ctx.RespondAsync(embedBuilder);
        }
        
        [Command, Cooldown(1, 20, CooldownBucketType.Guild)]
        public async Task Aram(CommandContext ctx)
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            var users = await database.GetDatabaseUsers(ctx);

            users.RemoveAll((user => user.userid == ctx.User.Id));
            
            foreach (var databaseUser in users)
            {
                if (ctx.Guild.Members.TryGetValue(databaseUser.userid, out DiscordMember? member))
                {
                    var embedBuilder = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithDescription($"{ctx.User.Mention}님의 칼바람나락 호출이 왔습니다!")
                        .WithImageUrl("https://static.wikia.nocookie.net/leagueoflegends/images/5/5f/Howling_Abyss_Map_Preview.jpg/revision/latest?cb=20140612032106");
                    await member.SendMessageAsync(embedBuilder);
                }
            }

            if (users.Count > 0)
            {
                await ctx.RespondAsync("메세지를 전송했습니다.");
            }
        }
        
        [Command]
        public async Task ImageOnly(CommandContext ctx)
        {
            await _messageHandler.ToggleChannel(ctx);
        }
    }
}