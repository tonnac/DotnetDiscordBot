using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class RouletteModules : BaseCommandModule
{
    [Command, Aliases("룰렛정보")]
    public async Task RouletteInfo(CommandContext ctx, [RemainingText] string? name)
    {
        if (name == null)
        {
            return;
        }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();

        var rouletteList = await database.GetRoulette(ctx.Guild, name);

        int numberOfMan = 0;
        int losses = 0;
        int takingPartCount = rouletteList.Count;
        
        foreach (var roulette in rouletteList)
        {
            if (roulette.Winner == name)
            {
                losses++;
                numberOfMan += roulette.Members.Count - 1;
            }
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .AddField(new DiscordEmbedField("참가한 게임", takingPartCount.ToString(), true))
            .AddField(new DiscordEmbedField("은혜를 입은 사람", numberOfMan.ToString(), true))
            .AddField(new DiscordEmbedField("염치없게 얻어먹은 게임", (takingPartCount - losses).ToString(), true));

        await ctx.RespondAsync(embedBuilder);
    }
    
}