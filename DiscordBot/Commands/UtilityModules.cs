using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using Discord;
using DiscordBot.Resource;

namespace DiscordBot.Commands
{
    public sealed class UtilityModules : BaseCommandModule
    {
        public UtilityModules(DiscordClient client, Config config)
        {
            _client = client;
            _config = config;
        }
        
        private readonly DiscordClient _client;
        private readonly Config _config;
        
        [Command, Aliases("h")]
        public async Task Help(CommandContext ctx)
        {
            // CommandsNextExtension? commandNext = _client.GetCommandsNext();
            // if (commandNext == null)
            // {
            //     return;
            // }
            //
            // var copyCommands =
            //     (from pair in commandNext.RegisteredCommands
            //         where pair.Key == pair.Value.Name
            //         select pair.Value)
            //     .OrderBy((command => command.Name)).ToList();
            //
            // DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            // embedBuilder.WithColor(DiscordColor.Azure);
            // embedBuilder.WithFooter("footer");
            //
            // string FindLocal(string name)
            // {
            //     TypeInfo typeinfo = typeof(Localization).GetTypeInfo();
            //     foreach (PropertyInfo propertyInfo in typeinfo.DeclaredProperties)
            //     {
            //         if (propertyInfo.Name == name && propertyInfo.GetValue(typeinfo) is string)
            //         {
            //             return (string)propertyInfo.GetValue(typeinfo)!;
            //         }
            //     }
            //
            //     return "";
            // }
            //
            // var commandsString = string.Join("\n", copyCommands.Select(x => $"`{x.Name}`{(x.Aliases.Count == 0 ? "" : $"(**{string.Join(", ", x.Aliases.Select((alias => alias)))}**)")}: {FindLocal(x.Name + "_Description")}"));
            // embedBuilder.WithDescription(commandsString);
            // await ctx.Channel.SendMessageAsync(embedBuilder.Build());
        }
    }
}