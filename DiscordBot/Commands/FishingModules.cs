using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Database;
using DiscordBot.Database.Tables;
using DiscordBot.Equip;
using DiscordBot.Resource;

namespace DiscordBot.Commands;

public class FishingModules : BaseCommandModule
{
    private readonly ContentsChannels _contentsChannels;
    
    
    public string fishEmoji_none = VEmoji.Shoe;
    public string fishEmoji_common = VEmoji.Fish;
    public string fishEmoji_rare = VEmoji.Blowfish;
    public string fishEmoji_epic = VEmoji.TropicalFish;
    public string fishEmoji_legendary = VEmoji.Merperson;

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

    public FishingModules(ContentsChannels contentsChannels)
    {
        _contentsChannels = contentsChannels;
    }

    //[Command, Aliases("f")]
    [Command, Aliases("f", "낚시"), Cooldown(1, 900, CooldownBucketType.UserAndChannel, true, true, 10)]
    public async Task Fishing(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        bool isFishingChannel = await _contentsChannels.IsFishingChannel(ctx);
        if (isFishingChannel == false)
        {
            var message = await ctx.RespondAsync("낚시가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }

        string fishThumbnail = "https://i.pinimg.com/originals/0f/8f/4f/0f8f4fbcd48f4d335f1ff3f8ec803c3b.gif";
        string fishEmoji_Result = fishEmoji_none;
        int fishGold_Result = fishGold_none;
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        // start,, calc final damage
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser fishUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(fishUserDatabase.equipvalue) * EquipCalculator.Fish_WeaponUpgradeMultiplier;
        int gemUpgrade = EquipCalculator.GetGemUpgradeInfo(fishUserDatabase.equipvalue) * EquipCalculator.Gold_GemUpgradeMultiplier;
        float gemPercentage = gemUpgrade / 100.0f;
        
        var rand = new Random();
        int fishingRandom = rand.Next(1, 101);

        fishingRandom += weaponUpgrade;

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
            fishThumbnail = "https://media.tenor.com/xOqPQTU9RagAAAAd/ariel-princess-ariel.gif"; 
            fishEmoji_Result = fishEmoji_legendary;
            fishGold_Result = fishGold_legendary;
        }
        
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail(fishThumbnail)
            .WithColor(DiscordColor.Blue)
            .WithAuthor(VEmoji.FishingPole + " " + name)
            .AddField(new DiscordEmbedField(VEmoji.Hook + fishEmoji_Result + " !", "+ " + VEmoji.Money + Convert.ToString(fishGold_Result), false));
        
        float addGemPercentageMoney = fishGold_Result * (1.0f + gemPercentage);
        GoldQuery query = new GoldQuery((int)addGemPercentageMoney);
        await database.UpdateUserGold(ctx, query);
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("fl", "물고기리스트"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task FishList(CommandContext ctx)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://image.yes24.com/goods/99358015/XL")
            .WithColor(DiscordColor.White)
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_none + " ]", Convert.ToString(nonePer) + "%, " + VEmoji.Money + Convert.ToString(fishGold_none), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_common + " ]", Convert.ToString(commonPer) + "%, " + VEmoji.Money + Convert.ToString(fishGold_common), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_rare + " ]", Convert.ToString(rarePer) + "%, " + VEmoji.Money + Convert.ToString(fishGold_rare), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_epic + " ]", Convert.ToString(epicPer) + "%, " + VEmoji.Money + Convert.ToString(fishGold_epic), false))
            .AddField(new DiscordEmbedField("[ "+ fishEmoji_legendary + " ]", Convert.ToString(legendaryPer) + "%, " + VEmoji.Money + Convert.ToString(fishGold_legendary), false));
        
        await ctx.RespondAsync(embedBuilder);
    }
}