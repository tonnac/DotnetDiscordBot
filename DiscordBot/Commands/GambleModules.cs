﻿using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Database;
using DiscordBot.Equip;
using DiscordBot.Resource;

/*
    [1000]
    -%	0
    5%	1500	x1.5
    3%	3000	x3
    1%	7777	x7
    
    [5000]
    -%	0
    5%	10000	x2
    3%	20000	x4
    1%	40000	x8
    
    [10000]
    -%	0
    5%	1       ?
    3%	10	    ?
    1%	100000	x10
 */
namespace DiscordBot.Commands;

public class GambleModules : BaseCommandModule
{   
    private readonly FundsGamble _fundsGamble;
    private readonly DiceGamble _diceGamble;

    private int _donationMoney;
    
    public GambleModules()
    {
        _donationMoney = 0;
        
        _fundsGamble = new FundsGamble(1, 500, 200, 24);

        _diceGamble = new DiceGamble();
    }

    [Command, Aliases("ggl", "도박리스트"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task GambleGameList(CommandContext ctx, [RemainingText] string? gambleCommand)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://img.freepik.com/premium-photo/classic-casino-roulette_103577-4040.jpg")
            .WithColor(DiscordColor.White)
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.CardFileBox + "  ] " + VEmoji.Money + Convert.ToString(_fundsGamble.Ante) + " ─── dfg", false))
            .AddField(new DiscordEmbedField(VEmoji.Trophy + " " + Convert.ToString(_fundsGamble.WinPer) + "%", VEmoji.Money + Convert.ToString(_fundsGamble.WinMoney), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Dice + "  ] " + VEmoji.Money + "1~10000 ─── ddg ???", false))
            .AddField(new DiscordEmbedField(VEmoji.Trophy + " ??%", " " + VEmoji.Money + "1~10000", true));
            
        
        await ctx.RespondAsync(embedBuilder);
    }

    [Command, Aliases("ddg", "주사위도박"), Cooldown(1, 3, CooldownBucketType.UserAndChannel)]
    public async Task DoDiceGamble(CommandContext ctx, [RemainingText] string? gambleCommand)
    {
        if (!ContentsChannels.GambleChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("도박이 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser gambleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(gambleUserDatabase.equipvalue) * EquipCalculator.Dice_RingUpgradeMultiplier;

        int ante = 0;
        int result = 0;
        int userDice = 0;
        int comDice = 0;
        if( !string.IsNullOrEmpty(gambleCommand))
        {
            Int32.TryParse(gambleCommand, out ante);
        }
        ante = 0 >= ante ? 1 : ante;
        
        if (ante > gambleUserDatabase.gold)
        {
            await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
            return;
        }

        result = _diceGamble.DoDiceGamble(ante, out userDice, out comDice, ringUpgrade);
        
        GoldQuery query = new GoldQuery(result - ante);
        await database.UpdateUserGold(ctx, query);

        string thumbnail = "https://mblogthumb-phinf.pstatic.net/20150921_183/ahn3607_14427964013593pE8h_PNG/bandicam_2015-09-21_09-33-43-638.png?type=w2";
        string plusminus = "-";
        
        if (0 < result)
        {
            thumbnail = "https://mblogthumb-phinf.pstatic.net/MjAxNzExMTJfMjUy/MDAxNTEwNDk0NDcxNzE3.TviKbphDkRt73FbgkUtXn-gpFXuCEfWsfCLYh7hgFNIg.tlUNqn3XMoIm_Mm69k-mo07vCH9YBYY9jfcESIaN9jMg.JPEG.jongwon6544/15ed6b7663649c14e.jpg?type=w2";
            plusminus = "+";
        }
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        string ringUpgradePlusText = 0 < ringUpgrade ? " +" + Convert.ToString(ringUpgrade) + "💍": "";

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail(thumbnail)
            .WithColor(DiscordColor.Gold)
            .AddField(new DiscordEmbedField("[ " + name + " ]", VEmoji.Dice + " " + Convert.ToString(userDice) + ringUpgradePlusText, true))
            .AddField(new DiscordEmbedField("VS","!", true))
            .AddField(new DiscordEmbedField("[ " + VEmoji.Robot + " ]", VEmoji.Dice + " " + Convert.ToString(comDice), true))
            .AddField(new DiscordEmbedField("──────────────", "[ " + plusminus + " " + VEmoji.Money + Convert.ToString(ante) + " ]", false));
                
        await ctx.RespondAsync(embedBuilder);
    }

    [Command, Aliases("dfg", "수금도박"), Cooldown(1, 5, CooldownBucketType.UserAndChannel, true, true, 5)]
    public async Task DoFundsGamble(CommandContext ctx, [RemainingText] string? gambleCommand)
    {
        if (!ContentsChannels.GambleChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("도박이 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser gambleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        if (gambleUserDatabase.gold >= _fundsGamble.Ante)
        {
            string name = Utility.GetMemberDisplayName(ctx.Member);
            
            int winMoney = _fundsGamble.DoFundsGamble(ctx);
        
            GoldQuery query = new GoldQuery(winMoney - _fundsGamble.Ante);
            await database.UpdateUserGold(ctx, query);

            if (0 < winMoney)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://mblogthumb-phinf.pstatic.net/MjAxNzExMTJfMjUy/MDAxNTEwNDk0NDcxNzE3.TviKbphDkRt73FbgkUtXn-gpFXuCEfWsfCLYh7hgFNIg.tlUNqn3XMoIm_Mm69k-mo07vCH9YBYY9jfcESIaN9jMg.JPEG.jongwon6544/15ed6b7663649c14e.jpg?type=w2")
                    .WithColor(DiscordColor.Gold)
                    .AddField(new DiscordEmbedField(name + " " + VEmoji.CryingFace, "[ - " + VEmoji.Money + Convert.ToString(_fundsGamble.Ante) + " ]", false))
                    .AddField(new DiscordEmbedField("..", "...." + VEmoji.Trophy + " !", false))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.ConfettiBall + " " + name + " " + VEmoji.ConfettiBall + " ]", "[ + " + VEmoji.Money + Convert.ToString(winMoney) + " ]", false));
        
                await ctx.RespondAsync(embedBuilder);
            }
            else
            {
                Dictionary<string, int> shareMoneySortDictionary = new Dictionary<string, int>();
                List<string> shareMoneyUsers = new List<string>();
                List<int> shareMoneys = new List<int>();

                shareMoneySortDictionary = _fundsGamble.GetWinMoneyShareSortDictionary();
            
                for (int index = 0; index < 3; ++index)
                {
                    shareMoneyUsers.Add(index+1 <= shareMoneySortDictionary.Keys.ToList().Count ? shareMoneySortDictionary.Keys.ToList()[index] : "X");
                    shareMoneys.Add(index+1 <= shareMoneySortDictionary.Values.ToList().Count ? shareMoneySortDictionary.Values.ToList()[index] : 0);
                }
            
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://i.gifer.com/E3xX.gif")
                    .WithColor(DiscordColor.Gold)
                    .AddField(new DiscordEmbedField(VEmoji.Trophy + " -> " + VEmoji.Money + Convert.ToString(_fundsGamble.WinMoney), "──────────", false))
                    .AddField(new DiscordEmbedField(name + " " + VEmoji.CryingFace, "[ - " + VEmoji.Money+ Convert.ToString(_fundsGamble.Ante) + " ]", false))
                    .AddField(new DiscordEmbedField(VEmoji.GoldMedal + shareMoneyUsers[0], Convert.ToString(shareMoneys[0]), true))
                    .AddField(new DiscordEmbedField(VEmoji.SilverMedal + shareMoneyUsers[1], Convert.ToString(shareMoneys[1]), true))
                    .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + shareMoneyUsers[2], Convert.ToString(shareMoneys[2]), true));
        
                await ctx.RespondAsync(embedBuilder);
            }
        }
        else
        {
            await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
        }
    }

    [Command, Aliases("dn", "기부", "사료")]
    public async Task Donation(CommandContext ctx, [RemainingText] string? donationCommand)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser gambleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        int donationValue = 0;
        if( !string.IsNullOrEmpty(donationCommand))
        {
            Int32.TryParse(donationCommand, out donationValue);
        }
        donationValue = 0 >= donationValue ? 100 : donationValue;
        
        if (donationValue > gambleUserDatabase.gold)
        {
            await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
            return;
        }
        
        GoldQuery query = new GoldQuery(-donationValue);
        await database.UpdateUserGold(ctx, query);

        _donationMoney += donationValue;

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/3815/3815861.png")
            .WithColor(DiscordColor.Gold)
            .AddField(new DiscordEmbedField(" - " + VEmoji.Money + Convert.ToString(donationValue), VEmoji.WingMoney + " " + ctx.Member.Mention, false))
            .AddField(new DiscordEmbedField("──────────", "[ " + VEmoji.GiftBox + " " + Convert.ToString(_donationMoney) + " ]", false));
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("thx", "감사", "왕왕"), Cooldown(1, 10, CooldownBucketType.User, true, true, 5)]
    public async Task Thanks(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        if (0 >= _donationMoney)
        {
            await ctx.RespondAsync(VEmoji.GiftBox + ".. " + VEmoji.QuestionMark);
            return;
        }

        int tempDonationMoney = _donationMoney;
        
        GoldQuery query = new GoldQuery(_donationMoney);
        await database.UpdateUserGold(ctx, query);

        _donationMoney = 0;

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/2913/2913091.png")
            .WithColor(DiscordColor.Gold)
            .AddField(new DiscordEmbedField(" + " + VEmoji.Money + Convert.ToString(tempDonationMoney), ctx.Member.Mention, false))
            .AddField(new DiscordEmbedField("──────────", "[ " + VEmoji.GiftBox + " " + Convert.ToString(_donationMoney) + " ]", false));
        
        await ctx.RespondAsync(embedBuilder);
    }

    [Command]
    public async Task SetFundsGambleWinMoney(CommandContext ctx, [RemainingText] string? fundsCommand)
    {
        bool result = false;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int fundsValue = 0;
            if( !string.IsNullOrEmpty(fundsCommand))
            {
                Int32.TryParse(fundsCommand, out fundsValue);
            }

            if (0 != fundsValue)
            {
                _fundsGamble.WinMoney = fundsValue;
                result = true;
            }
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.GreenCheckBox));
        }
    }
    
    [Command]
    public async Task AddMoneyAdmin(CommandContext ctx, [RemainingText] string? testMoneyCommand)
    {
        bool result = false;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int testMoney = 0;
            if( !string.IsNullOrEmpty(testMoneyCommand))
            {
                Int32.TryParse(testMoneyCommand, out testMoney);
            }

            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.GetDatabaseUser(ctx.Guild, ctx.User);
            GoldQuery query = new GoldQuery(testMoney);
            await database.UpdateUserGold(ctx, query);
            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.GreenCheckBox));
        }
    }
    
    [Command] // ToggleGambleChannel
    public async Task Gggg(CommandContext ctx)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (ContentsChannels.GambleChannels.Contains(ctx.Channel.Id))
            {
                ContentsChannels.GambleChannels.Remove(ctx.Channel.Id);
                emoji = VEmoji.RedCrossMark;
            }
            else
            {
                ContentsChannels.GambleChannels.Add(ctx.Channel.Id);
                emoji = VEmoji.GreenCheckBox;
            }

            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
}