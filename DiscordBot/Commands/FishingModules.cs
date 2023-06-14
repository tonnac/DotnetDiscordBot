﻿using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class FishingModules : BaseCommandModule
{
    //[Command, Aliases("f")]
    [Command, Aliases("f"), Cooldown(1, 900, CooldownBucketType.User, true, true, 10)]
    public async Task Fishing(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        string fishEmoji_none = "\uD83D\uDC5F";
        string fishEmoji_common = "\uD83D\uDC1F";
        string fishEmoji_rare = "\uD83D\uDC21";
        string fishEmoji_epic = "\uD83D\uDC20";
        string fishEmoji_legendary = "\uD83E\uDDDC";
        string fishEmoji_Result = fishEmoji_none;

        int fishGold_none = 0;
        int fishGold_common = 50;
        int fishGold_rare = 100;
        int fishGold_epic = 200;
        int fishGold_legendary = 1000;
        int fishGold_Result = fishGold_none;

        int nonePer = 5;
        int commonPer = 50;
        int rarePer = 30;
        int epicPer = 10;
        int legendaryPer = 5;
        
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
    
    [Command, Aliases("fl"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task FishList(CommandContext ctx)
    {
        string fishEmoji_none = "\uD83D\uDC5F";
        string fishEmoji_common = "\uD83D\uDC1F";
        string fishEmoji_rare = "\uD83D\uDC21";
        string fishEmoji_epic = "\uD83D\uDC20";
        string fishEmoji_legendary = "\uD83E\uDDDC";

        int fishGold_none = 0;
        int fishGold_common = 50;
        int fishGold_rare = 100;
        int fishGold_epic = 200;
        int fishGold_legendary = 1000;

        int nonePer = 5;
        int commonPer = 50;
        int rarePer = 30;
        int epicPer = 10;
        int legendaryPer = 5;
        
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
}