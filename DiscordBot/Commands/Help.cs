using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Resource;

namespace DiscordBot.Commands;

public static class Help
{
    private static readonly Dictionary<string, string?> ModuleThumbnail = new ()
    {
        { "Music", "https://daily.jstor.org/wp-content/uploads/2023/01/good_times_with_bad_music_1050x700.jpg" }, 
        { "Lol", "https://yt3.googleusercontent.com/_nlyMx8RWF3h2aG8PslnqMobecnco8XjOBki7dL_nayZYfNxxFdPSp2PpxUytjN4VmHqb4XPtA=s900-c-k-c0x00ffffff-no-rj" }, 
        { "Boss", "https://oldschoolroleplaying.com/wp-content/uploads/2020/01/Skull-Cave-Entrance.jpg" },
        { "Fishing", "https://i.pinimg.com/550x/ec/4f/da/ec4fda8ea3d3a52ba561e9e54d3c81fc.jpg" },
        { "Gamble", "https://img.freepik.com/free-photo/casino-games-backdrop-banner-3d-illustration-with-casino-elements-craps-roulette-and-poker-cards-generative-ai_91128-2286.jpg?w=2000" },
        { "UserGameInfo", "https://cdn-icons-png.flaticon.com/512/943/943579.png" },
        { "Battle", "https://i.ytimg.com/vi/Kcd7qnnGOUs/maxresdefault.jpg" },
        { "Utility", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRcjtzAEQMDdcnf_VmHJ9RcQSzP50VulGw7lazLNV189n-PsSEvOAYJWaaObqTReXMr7s4&usqp=CAU" }
        { "Utility", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRcjtzAEQMDdcnf_VmHJ9RcQSzP50VulGw7lazLNV189n-PsSEvOAYJWaaObqTReXMr7s4&usqp=CAU" },
        { "Yacht", "https://postfiles.pstatic.net/MjAyMTA0MDZfNDAg/MDAxNjE3NzA2Njc3NTk1.g6OF7DpcboUWq_7rVNSXgiJY9LZFPGA_x9XcAZNsUJgg.cm7hVC1kZNVmY7SEt5jPJ0nKxbRPMzQ0-IVb44bU1zUg.PNG.kboardgame/image.png?type=w966" }
    };

    public static DiscordMessageBuilder GetHelp(DiscordClient client)
    {
        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
        CommandsNextExtension? commandNext = client.GetCommandsNext();
        if (commandNext == null)
        {
            return messageBuilder;
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
            where pair.Key == pair.Value.Name && pair.Value.IsHidden == false
            orderby pair.Value.Name
            group pair.Value by pair.Value.Module.ModuleType.Name.Split("Modules")[0]
            into groupData
            select groupData;


        foreach (var copyCommand in copyCommands)
        {
            var commandsString = string.Join("", copyCommand.Select(x => $"`{x.Name}`{(x.Aliases.Count == 0 ? "" : $"(**{string.Join(", ", x.Aliases.Select((alias => alias)))}**)")}:\n- {FindLocal(x.Name + "_Description")}\n"));
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(copyCommand.Key)
                .WithColor(DiscordColor.Azure)
                .WithTimestamp(DateTime.Now)
                .WithDescription(commandsString);
        
            if (ModuleThumbnail.TryGetValue(copyCommand.Key, out string? thumbnailUrl))
            {
                embedBuilder.WithThumbnail(thumbnailUrl);
            }

            messageBuilder.AddEmbed(embedBuilder);
        }

        return messageBuilder;
    }
}