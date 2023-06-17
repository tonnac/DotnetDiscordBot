using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class FishingModules : BaseCommandModule
{
    private readonly SortedSet<ulong> _fishingChannels = new();
    
    public string fishEmoji_none = "\uD83D\uDC5F";
    public string fishEmoji_common = "\uD83D\uDC1F";
    public string fishEmoji_rare = "\uD83D\uDC21";
    public string fishEmoji_epic = "\uD83D\uDC20";
    public string fishEmoji_legendary = "\uD83E\uDDDC";

    public int fishGold_none = 0;
    public int fishGold_common = 100;
    public int fishGold_rare = 300;
    public int fishGold_epic = 500;
    public int fishGold_legendary = 2000;

    public int nonePer = 5;
    public int commonPer = 50;
    public int rarePer = 30;
    public int epicPer = 10;
    public int legendaryPer = 5;
    
    //[Command, Aliases("f")]
    [Command, Aliases("f", "낚시"), Cooldown(1, 900, CooldownBucketType.UserAndChannel, true, true, 10)]
    public async Task Fishing(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        if (!_fishingChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("낚시가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        string fishEmoji_Result = fishEmoji_none;
        int fishGold_Result = fishGold_none;
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        var rand = new Random();
        int fishingRandom = rand.Next(1, 101);

        if (nonePer < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_common;
            fishGold_Result = fishGold_common;
        }
        if (nonePer+commonPer < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_rare;
            fishGold_Result = fishGold_rare;
        }
        if (nonePer+commonPer+rarePer < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_epic;
            fishGold_Result = fishGold_epic;
        }
        if (nonePer+commonPer+rarePer+epicPer < fishingRandom)
        {
            fishEmoji_Result = fishEmoji_legendary;
            fishGold_Result = fishGold_legendary;
        }
        
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://i.pinimg.com/originals/0f/8f/4f/0f8f4fbcd48f4d335f1ff3f8ec803c3b.gif")
            .WithColor(DiscordColor.Blue)
            .WithAuthor("\uD83C\uDFA3 " + name)
            .AddField(new DiscordEmbedField("\uD83E\uDE9D" + fishEmoji_Result + " !", "+ \uD83D\uDCB0" + Convert.ToString(fishGold_Result), false));
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        await database.GetDatabaseUser(ctx.Guild, ctx.User);
        GoldQuery query = new GoldQuery(fishGold_Result);
        await database.UpdateUserGold(ctx, query);
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("fl", "물고기리스트"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task FishList(CommandContext ctx)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://image.yes24.com/goods/99358015/XL")
            .WithColor(DiscordColor.White)
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_none + " ]", Convert.ToString(nonePer) + "%, \uD83D\uDCB0" + Convert.ToString(fishGold_none), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_common + " ]", Convert.ToString(commonPer) + "%, \uD83D\uDCB0" + Convert.ToString(fishGold_common), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_rare + " ]", Convert.ToString(rarePer) + "%, \uD83D\uDCB0" + Convert.ToString(fishGold_rare), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_epic + " ]", Convert.ToString(epicPer) + "%, \uD83D\uDCB0" + Convert.ToString(fishGold_epic), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_legendary + " ]", Convert.ToString(legendaryPer) + "%, \uD83D\uDCB0" + Convert.ToString(fishGold_legendary), false));
        
        await ctx.RespondAsync(embedBuilder);
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🧾"));
    }
    
    [Command] // ToggleFishingChannel
    public async Task Ffff(CommandContext ctx)
    {
        bool result = false;
        string emoji = "❌";
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (_fishingChannels.Contains(ctx.Channel.Id))
            {
                _fishingChannels.Remove(ctx.Channel.Id);
                emoji = "❌";
            }
            else
            {
                _fishingChannels.Add(ctx.Channel.Id);
                emoji = "✅";
            }

            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
}