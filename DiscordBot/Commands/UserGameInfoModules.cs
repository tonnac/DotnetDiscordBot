using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class UserGameInfoModules : BaseCommandModule
{
    [Command, Aliases("mi"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task MyInfo(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser MyUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        string name = string.IsNullOrEmpty(ctx.Member.Nickname) ? ctx.Member.Username : ctx.Member.Nickname;

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/943/943579.png")
            .WithColor(DiscordColor.Black)
            .AddField(new DiscordEmbedField("\uD83D\uDD0D " + name, "──────────", false))
            .AddField(new DiscordEmbedField("[  \u2620\uFE0F  ]", Convert.ToString(MyUserDatabase.bosskillcount), true))
            .AddField(new DiscordEmbedField("[  \uD83D\uDCB0  ]", Convert.ToString(MyUserDatabase.gold), true))
            .AddField(new DiscordEmbedField("[  \uD83D\uDCA5  ]", Convert.ToString(MyUserDatabase.bosstotaldamage), true));
        
        await ctx.RespondAsync(embedBuilder);
    }
}