using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class LolModules : BaseCommandModule
{
    [Command, Aliases("AramRegister")]
    public async Task DoAramRegister(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var user = await database.GetDatabaseUser(ctx.Guild, ctx.User);
        var embedBuilder = new DiscordEmbedBuilder();

        if (user.userid != 0 && user.aram == true)
        {
            embedBuilder.WithDescription("Already Active");
        }
        else
        {
            bool bSuccess = await database.ActiveAram(ctx, true);

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
        
    [Command, Aliases("AramDelete")]
    public async Task DoAramDelete(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();

        var user = await database.GetDatabaseUser(ctx.Guild, ctx.User);
        var embedBuilder = new DiscordEmbedBuilder();
        if (user.aram == false)
        {
            embedBuilder.WithDescription("Already InActive");
        }
        else
        {
            var bSuccess = await database.ActiveAram(ctx, false);
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
        
    [Command, Aliases("DoAram"), Cooldown(1, 20, CooldownBucketType.Guild)]
    public async Task Aram(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var users = await database.GetAramUser(ctx);

        users.RemoveAll(user => user.userid == ctx.User.Id);
            
        foreach (var databaseUser in users)
        {
            if (ctx.Guild.Members.TryGetValue(databaseUser.userid, out DiscordMember? member))
            {
                string name = Utility.GetMemberDisplayName(member);
                var embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithDescription($"{name}님의 칼바람나락 호출이 왔습니다!")
                    .WithImageUrl("https://static.wikia.nocookie.net/leagueoflegends/images/5/5f/Howling_Abyss_Map_Preview.jpg/revision/latest?cb=20140612032106");
                await member.SendMessageAsync(embedBuilder);
            }
        }

        if (users.Count > 0)
        {
            await ctx.RespondAsync("메세지를 전송했습니다.");
        }
    }
}