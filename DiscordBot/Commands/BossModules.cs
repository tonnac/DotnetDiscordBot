﻿using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;

namespace DiscordBot.Commands;

public class BossModules : BaseCommandModule
{
    private readonly BossMonster _bossMonster;
    private readonly BossParser _bossParser;

    public BossModules()
    {
        var rand = new Random();
        int bossType = rand.Next((int)BossType.Start + 1, (int) BossType.End);
        _bossMonster = new BossMonster((BossType)bossType);

        _bossParser = new BossParser();
    }
    
    //[Command, Aliases("ba")]
    [Command, Aliases("ba"), Cooldown(1, 300, CooldownBucketType.User)]
    public async Task BossAttack(CommandContext ctx)
    {
        var rand = new Random();

        int AttackChance = rand.Next(1, 101);
        string DamageTypeEmojiCode = "\uD83D\uDCA5 ";
        string CritAddText = "";
        string AttackGifurl = "https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif";
        
        int FinalDamage = rand.Next(1, 101);
        if (10 >= AttackChance)
        {
            FinalDamage = 0;
            DamageTypeEmojiCode = "\ud83d\ude35\u200d\ud83d\udcab ";
            AttackGifurl = "https://media.tenor.com/ov3Jx6Fu-6kAAAAM/dark-souls-dance.gif";
        }
        else if (85 <= AttackChance)
        {
            FinalDamage = FinalDamage * 2 + 100;
            CritAddText = " !";
            DamageTypeEmojiCode = "\uD83D\uDD25 ";
            AttackGifurl = "https://media.tenor.com/dhGo-zgViLoAAAAM/soul-dark.gif";
        }
        
        int hitCount = _bossMonster.HitCount;
        int killedBossGetGold = 777 == _bossMonster.CurrentMaxHp ? 7777 : _bossMonster.CurrentMaxHp;
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<string, int> BestDealerInfo;
        bool bIsKilled = _bossMonster.IsKilledByDamage(ctx.Member.Username, FinalDamage, out BestDealerInfo);

        if (!bIsKilled)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail(AttackGifurl)
                .WithColor(DiscordColor.HotPink)
                .WithAuthor("\u2694\uFE0F " + ctx.Member.Username)
                .AddField(new DiscordEmbedField(DamageTypeEmojiCode + Convert.ToString(FinalDamage) + CritAddText,
                    _bossMonster.BossEmojiCode + " " + Convert.ToString(_bossMonster.CurrentHp) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));

            _bossParser.AddTotalDeal(ctx.Member.Username, FinalDamage);
        
            await ctx.RespondAsync(embedBuilder);
        }
        else
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("\uD83C\uDF8A " + ctx.Member.Username + "  " + "\uD83D\uDCA5" + Convert.ToString(FinalDamage))
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " \u2694\uFE0F " + Convert.ToString(hitCount + 1), 
                    "\uD83E\uDD47" + BestDealerInfo.Key + "   " + "\uD83D\uDCA5" + Convert.ToString(BestDealerInfo.Value),
                    false));

            _bossParser.AddKillCount(ctx.Member.Username, 1);
            _bossParser.AddTotalGold(ctx.Member.Username, killedBossGetGold);
        
            var message = await ctx.RespondAsync(embedBuilder);

            if( message != null )
            {
                await message.PinAsync();
            }
        }
    }
    
    [Command, Aliases("bi")]
    public async Task BossInfo(CommandContext ctx)
    {
        KeyValuePair<string, int> BestDealerInfo = _bossMonster.GetBestDealer();
        string BestDealer = BestDealerInfo.Key;
        int BestDealerTotalDamage = BestDealerInfo.Value;
        
        if (string.IsNullOrEmpty(BestDealerInfo.Key))
        {
            BestDealer = "X";
            BestDealerTotalDamage = 0;
        }
        
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://upload2.inven.co.kr/upload/2018/11/09/bbs/i15705696321.jpg?MW=800")
            .WithColor(DiscordColor.Orange)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField(_bossMonster.BossEmojiCode, "\u2665\uFE0F " + _bossMonster.CurrentHp + "/" + _bossMonster.CurrentMaxHp, false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + BestDealer + "   " + "\uD83D\uDCA5" + Convert.ToString(BestDealerTotalDamage),
                "\u2694\uFE0F " + _bossMonster.HitCount, false));
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("br"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task BossRank(CommandContext ctx)
    {
        Dictionary<string, int> killCountRankDictionary = _bossParser.GetKillCountRankDictionary();
        Dictionary<string, int> goldRankDictionary = _bossParser.GetTotalGoldRankDictionary();
        Dictionary<string, int> dealRankDictionary = _bossParser.GetTotalDealRankDictionary();

        List<string> killRankUser = new List<string>();
        List<int> killRankCount = new List<int>();
        List<string> goldRankUser = new List<string>();
        List<int> goldRankCount = new List<int>();
        List<string> dealRankUser = new List<string>();
        List<int> dealRankCount = new List<int>();

        for (int index = 0; index < 3; ++index)
        {
            killRankUser.Add(index+1 <= killCountRankDictionary.Keys.ToList().Count ? killCountRankDictionary.Keys.ToList()[index] : "X");
            killRankCount.Add(index+1 <= killCountRankDictionary.Values.ToList().Count ? killCountRankDictionary.Values.ToList()[index] : 0);
            goldRankUser.Add(index+1 <= goldRankDictionary.Keys.ToList().Count ? goldRankDictionary.Keys.ToList()[index] : "X");
            goldRankCount.Add(index+1 <= goldRankDictionary.Values.ToList().Count ? goldRankDictionary.Values.ToList()[index] : 0);
            dealRankUser.Add(index+1 <= dealRankDictionary.Keys.ToList().Count ? dealRankDictionary.Keys.ToList()[index] : "X");
            dealRankCount.Add(index+1 <= dealRankDictionary.Values.ToList().Count ? dealRankDictionary.Values.ToList()[index] : 0);
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1021/1021100.png")
            .WithColor(DiscordColor.Brown)
            .AddField(new DiscordEmbedField("────\uD83C\uDFC6────", "[  \u2620\uFE0F  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + killRankUser[0], Convert.ToString(killRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + killRankUser[1], Convert.ToString(killRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + killRankUser[2], Convert.ToString(killRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  \uD83D\uDCB0  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + goldRankUser[0], Convert.ToString(goldRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + goldRankUser[1], Convert.ToString(goldRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + goldRankUser[2], Convert.ToString(goldRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  \uD83D\uDCA5  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + dealRankUser[0], Convert.ToString(dealRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + dealRankUser[1], Convert.ToString(dealRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + dealRankUser[2], Convert.ToString(dealRankCount[2]), true));
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("bl"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task BossList(CommandContext ctx)
    {

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1440/1440998.png")
            .WithColor(DiscordColor.White)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Ant), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Ant) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Ant), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Bat), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Bat) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Bat), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Octopus), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Octopus) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Octopus), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Shark), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Shark) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Shark), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Unicorn), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Unicorn) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Unicorn), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Mammoth), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Mammoth) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Mammoth), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Devil), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Devil) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Devil), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.SlotMachine), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.SlotMachine) + ", \uD83D\uDCB0" + Convert.ToString(7777), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Alien), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Alien) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Alien), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Trex), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Trex) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Trex), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.Dragon), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Dragon) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Dragon), false))
            .AddField(new DiscordEmbedField(_bossMonster.GetBossEmojiCode(BossType.TheOffice), "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.TheOffice) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.TheOffice), false));
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command]
    public async Task BossRankReset(CommandContext ctx)
    {
        if (0 == (ctx.Member.Permissions & Permissions.Administrator))
        {
            _bossParser.ResetBossParser();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }
    }
}