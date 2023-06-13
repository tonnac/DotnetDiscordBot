using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class FishingModules : BaseCommandModule
{
    //[Command, Aliases("f")]
    [Command, Aliases("f"), Cooldown(1, 600, CooldownBucketType.User, true, true, 10)]
    public async Task Fishing(CommandContext ctx)
    {
        string fishEmoji_common = "\uD83D\uDC1F";
        string fishEmoji_rare = "\uD83D\uDC21";
        string fishEmoji_epic = "\uD83D\uDC20";
        string fishEmoji_legendary = "\uD83E\uDDDC";
        string fishEmoji_Result = fishEmoji_common;

        int fishGold_common = 50; // 50
        int fishGold_rare = 100; // 30
        int fishGold_epic = 200; // 15
        int fishGold_legendary = 1000; // 5
        int fishGold_Result = fishGold_common;
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        var rand = new Random();
        int fishingRandom = rand.Next(1, 101);

        if (50 < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_rare;
            fishGold_Result = fishGold_rare;
        }
        if (80 < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_epic;
            fishGold_Result = fishGold_epic;
        }
        if (95 < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_legendary;
            fishGold_Result = fishGold_legendary;
        }
        
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://i.pinimg.com/originals/0f/8f/4f/0f8f4fbcd48f4d335f1ff3f8ec803c3b.gif")
            .WithColor(DiscordColor.Blue)
            .WithAuthor("\uD83C\uDFA3 " + name)
            .AddField(new DiscordEmbedField("\uD83E\uDE9D" + fishEmoji_Result + " !", "+\uD83D\uDCB0" + Convert.ToString(fishGold_Result), false));
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        await database.GetDatabaseUser(ctx.Guild, ctx.User);
        FishingQuery query = new FishingQuery(fishGold_Result);
        await database.UpdateFishingGold(ctx, query);
        
        await ctx.RespondAsync(embedBuilder);
    }
}