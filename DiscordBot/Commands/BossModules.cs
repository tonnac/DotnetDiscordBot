using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Database;
using DiscordBot.Equip;

namespace DiscordBot.Commands;

public class BossModules : BaseCommandModule
{   
    private readonly BossMonster _bossMonster;

    public BossModules()
    {
        var rand = new Random();
        int bossType = rand.Next((int)BossType.Start + 1, (int) BossType.End);
        _bossMonster = new BossMonster((BossType)bossType);
    }
    
    //[Command, Aliases("ba")]
    [Command, Aliases("ba", "공격", "보스공격"), Cooldown(1, 300, CooldownBucketType.UserAndChannel, true, true, 10)]
    public async Task BossAttack(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        if (!ContentsChannels.BossChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("보스공격이 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        

        // start,, calc final damage
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(userDatabase.equipvalue);
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(userDatabase.equipvalue);
        
        var rand = new Random();
        
        int missPer = 10;
        int critPer = 15;
        int massacrePer = 1;
        int attackPer = 100 - missPer - critPer - massacrePer;
        int FinalDamage = rand.Next(1, 101);
        int AttackChance = rand.Next(1, 101);
        string CritAddText = "";
        string DamageTypeEmojiCode = "\uD83D\uDCA5 ";
        string AttackGifurl = "https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif";
        

        if (missPer >= AttackChance + ringUpgrade) // miss
        {
            FinalDamage = 0;
            DamageTypeEmojiCode = "\ud83d\ude35\u200d\ud83d\udcab ";
            AttackGifurl = "https://media.tenor.com/ov3Jx6Fu-6kAAAAM/dark-souls-dance.gif";
        }
        else
        {
            // attack
            
            if (missPer + attackPer < AttackChance + ringUpgrade) // critical
            {
                weaponUpgrade *= 2;
                FinalDamage = FinalDamage * 2 + 100;
                CritAddText = " !";
                DamageTypeEmojiCode = "\uD83D\uDD25 ";
                AttackGifurl = "https://media.tenor.com/dhGo-zgViLoAAAAM/soul-dark.gif";
                
                if (missPer + attackPer + critPer < AttackChance) // massacre
                {
                    FinalDamage = 9999;
                    CritAddText = " !!";
                    DamageTypeEmojiCode = "\uD83D\uDD25 ";
                    AttackGifurl = "https://media.tenor.com/8ZdT_rjqHzcAAAAd/dark-souls-gwyn.gif";
                }
            }
        }
        // end,, calc final damage
        
        string weaponUpgradePlusText = 0 < weaponUpgrade ? " +" + Convert.ToString(weaponUpgrade) + "🗡️": "";
        
        bool bIsOverKill = (FinalDamage + weaponUpgrade) >= _bossMonster.CurrentHp;

        string name = Utility.GetMemberDisplayName(ctx.Member);

        // hit embed
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail(AttackGifurl)
            .WithColor(DiscordColor.HotPink)
            .WithAuthor("\u2694\uFE0F " + name + "    +\uD83D\uDCB0")
            .AddField(new DiscordEmbedField(DamageTypeEmojiCode + Convert.ToString(FinalDamage) + CritAddText + weaponUpgradePlusText,
                _bossMonster.BossEmojiCode + " " + Convert.ToString(bIsOverKill ? 0 : _bossMonster.CurrentHp - (FinalDamage + weaponUpgrade)) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        await ctx.RespondAsync(embedBuilder);

        // add parser
        int validDamage = bIsOverKill ? _bossMonster.CurrentHp : (FinalDamage + weaponUpgrade);
        
        // dead check
        int hitCount = _bossMonster.HitCount;
        int killedBossGetGold = 777 == _bossMonster.CurrentMaxHp ? 7777 : _bossMonster.CurrentMaxHp;
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<string, int> bestDealerInfo;
        
        if( _bossMonster.IsKilledByDamage(name, (FinalDamage + weaponUpgrade), out bestDealerInfo) )
        {
            DiscordEmbedBuilder killEmbedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("[ \uD83C\uDF8A" + name + "\uD83C\uDF8A ]  +\uD83D\uDCB0" + Convert.ToString(killedBossGetGold) )
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " \u2694\uFE0F " + Convert.ToString(hitCount + 1), 
                    "\uD83E\uDD47" + bestDealerInfo.Key + "   " + "\uD83D\uDCA5" + Convert.ToString(bestDealerInfo.Value),
                    false));

            
            BossQuery query = new BossQuery((ulong)validDamage, 1, killedBossGetGold + validDamage, 1);
            await database.UpdateBossRaid(ctx, query);
        
            await ctx.Channel.SendMessageAsync(killEmbedBuilder);
        }
        else
        {
            BossQuery query = new BossQuery((ulong)validDamage, 0, validDamage, 1);
            await database.UpdateBossRaid(ctx, query);
        }
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("💥"));
    }
    
    [Command, Aliases("bi", "보스정보"), Cooldown(1, 3, CooldownBucketType.User)]
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
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("📊"));
    }
    
    [Command, Aliases("gr", "랭킹"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task GameRanking(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        List<DatabaseUser> users= await database.GetDatabaseUsers(ctx);

        Dictionary<string, int> killCountRankDictionary = users.Where(user => user.bosskillcount > 0).OrderByDescending(user => user.bosskillcount).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.bosskillcount);
        
        Dictionary<string, int> goldRankDictionary = users.Where(user => user.gold > 0).OrderByDescending(user => user.gold).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.gold);

        Dictionary<string, ulong> dealRankDictionary = users.Where(user => user.bosstotaldamage > 0).OrderByDescending(user => user.bosstotaldamage).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.bosstotaldamage);
        
        Dictionary<string, int> equipRankDictionary = users.Where(user => user.equipvalue > 0).OrderByDescending(user => user.equipvalue).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.equipvalue);
        
        List<string> killRankUser = new List<string>();
        List<int> killRankCount = new List<int>();
        List<string> goldRankUser = new List<string>();
        List<int> goldRankCount = new List<int>();
        List<string> dealRankUser = new List<string>();
        List<ulong> dealRankCount = new List<ulong>();
        List<string> equipRankUser = new List<string>();
        List<int> equipRankCount = new List<int>();

        for (int index = 0; index < 3; ++index)
        {
            killRankUser.Add(index+1 <= killCountRankDictionary.Keys.ToList().Count ? killCountRankDictionary.Keys.ToList()[index] : "X");
            killRankCount.Add(index+1 <= killCountRankDictionary.Values.ToList().Count ? killCountRankDictionary.Values.ToList()[index] : 0);
            goldRankUser.Add(index+1 <= goldRankDictionary.Keys.ToList().Count ? goldRankDictionary.Keys.ToList()[index] : "X");
            goldRankCount.Add(index+1 <= goldRankDictionary.Values.ToList().Count ? goldRankDictionary.Values.ToList()[index] : 0);
            dealRankUser.Add(index+1 <= dealRankDictionary.Keys.ToList().Count ? dealRankDictionary.Keys.ToList()[index] : "X");
            dealRankCount.Add(index+1 <= dealRankDictionary.Values.ToList().Count ? dealRankDictionary.Values.ToList()[index] : 0);
            equipRankUser.Add(index+1 <= equipRankDictionary.Keys.ToList().Count ? equipRankDictionary.Keys.ToList()[index] : "X");
            equipRankCount.Add(index+1 <= equipRankDictionary.Values.ToList().Count ? equipRankDictionary.Values.ToList()[index] : 0);
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1021/1021100.png")
            .WithColor(DiscordColor.Brown)
            .AddField(new DiscordEmbedField("────\uD83C\uDFC6────", "[  \uD83D\uDCB0  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + goldRankUser[0], Convert.ToString(goldRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + goldRankUser[1], Convert.ToString(goldRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + goldRankUser[2], Convert.ToString(goldRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  🗡️ + 💍  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + equipRankUser[0], "+" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[0])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[0])), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + equipRankUser[1], "+" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[1])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[1])), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + equipRankUser[2], "+" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[2])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[2])), true))
            .AddField(new DiscordEmbedField("──────────", "[  \u2620\uFE0F  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + killRankUser[0], Convert.ToString(killRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + killRankUser[1], Convert.ToString(killRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + killRankUser[2], Convert.ToString(killRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  \uD83D\uDCA5  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + dealRankUser[0], Convert.ToString(dealRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + dealRankUser[1], Convert.ToString(dealRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + dealRankUser[2], Convert.ToString(dealRankCount[2]), true));
        
        await ctx.RespondAsync(embedBuilder);
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🏆"));
    }
    
    [Command, Aliases("bl", "보스리스트"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task BossList(CommandContext ctx)
    {

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1440/1440998.png")
            .WithColor(DiscordColor.White)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Mosquito) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Mosquito) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Mosquito), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Bat) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Bat) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Bat), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Octopus) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Octopus) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Octopus), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Shark) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Shark) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Shark), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Unicorn) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Unicorn) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Unicorn), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Mammoth) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Mammoth) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Mammoth), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Devil) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Devil) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Devil), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.SlotMachine) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.SlotMachine) + ", \uD83D\uDCB0" + Convert.ToString(7777), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Alien) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Alien) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Alien), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Trex) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Trex) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Trex), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.Dragon) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.Dragon) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.Dragon), false))
            .AddField(new DiscordEmbedField("[ "+ _bossMonster.GetBossEmojiCode(BossType.TheOffice) + " ]", "\u2665\uFE0F" + _bossMonster.GetBossMaxHp(BossType.TheOffice) + ", \uD83D\uDCB0" + _bossMonster.GetBossMaxHp(BossType.TheOffice), false));
        
        await ctx.RespondAsync(embedBuilder);
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🧾"));
    }

    [Command, Aliases("uw", "무기강화")]
    public async Task UpgradeWeapon(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        int weaponCurrentUpgrade = EquipCalculator.GetWeaponUpgradeInfo(userDatabase.equipvalue);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        if (9 <= weaponCurrentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " 👍");
        }
        else
        {
            if (userDatabase.gold >= EquipCalculator.WeaponUpgradeMoney)
            {
                int upgradeResult = EquipCalculator.Upgrade(weaponCurrentUpgrade);

                GoldQuery query = new GoldQuery(-EquipCalculator.WeaponUpgradeMoney);
                await database.UpdateUserGold(ctx, query);

                switch (upgradeResult)
                {
                    case -1: // Broken
                    {
                        await database.AddEquipValue(ctx, -weaponCurrentUpgrade);

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://social-phinf.pstatic.net/20210407_47/161775296734159xKI_GIF/1787c8c2dd04baebd123123312312.gif")
                            .WithColor(DiscordColor.DarkRed)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️0 ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 0: // Fail
                    {
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Red)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 1: // Success
                    {
                        await database.AddEquipValue(ctx, 1);

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Green)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade+1) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    default:
                        break;
                }
            }
            else
            {
                await ctx.RespondAsync("\uD83D\uDCB0.. \u2753");
            }
        }
    }
    
    [Command, Aliases("ur", "반지강화")]
    public async Task UpgradeRing(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        int ringCurrentUpgrade = EquipCalculator.GetRingUpgradeInfo(userDatabase.equipvalue);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        if (9 <= ringCurrentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " 👍");
        }
        else
        {
            if (userDatabase.gold >= EquipCalculator.RingUpgradeMoney)
            {
                int upgradeResult = EquipCalculator.Upgrade(ringCurrentUpgrade);

                GoldQuery query = new GoldQuery(-EquipCalculator.RingUpgradeMoney);
                await database.UpdateUserGold(ctx, query);

                switch (upgradeResult)
                {
                    case -1: // Broken
                    {
                        await database.AddEquipValue(ctx, -(ringCurrentUpgrade*10));

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://social-phinf.pstatic.net/20210407_47/161775296734159xKI_GIF/1787c8c2dd04baebd123123312312.gif")
                            .WithColor(DiscordColor.DarkRed)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️0 ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 0: // Fail
                    {
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Red)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 1: // Success
                    {
                        await database.AddEquipValue(ctx, 10);

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Green)
                            .AddField(new DiscordEmbedField("⚒️ " + name, "[ - \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ 💍 ]", "[ +️" + Convert.ToString(ringCurrentUpgrade+1) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    default:
                        break;
                }
            }
            else
            {
                await ctx.RespondAsync("\uD83D\uDCB0.. \u2753");
            }
        }
    }
    
    [Command, Aliases("ul", "강화확률"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task UpgradeSuccessPercentageList(CommandContext ctx, [RemainingText] string? upgradeCommand)
    {
        bool isFullList = false;
        if (!string.IsNullOrEmpty(upgradeCommand))
        {
            int upgradeStep = 0;
            Int32.TryParse(upgradeCommand, out upgradeStep);

            if (0 > upgradeStep || 8 < upgradeStep)
            {
                isFullList = true;
            }
            else
            {
                int nowStep = upgradeStep;
                
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://media.istockphoto.com/id/607898530/photo/blacksmith-manually-forging-the-molten-metal.jpg?s=612x612&w=0&k=20&c=XJK8AuqbsehPFumor0RZGO4bd5s0M9MWInGixbzhw48=")
                    .WithColor(DiscordColor.White)
                    .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                    .AddField(new DiscordEmbedField("[ 💍 ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                    .AddField(new DiscordEmbedField("──────────", "[ "+ Convert.ToString(nowStep) + " > " + Convert.ToString(nowStep + 1) + " ]", false))
                    .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].SuccessPer) + "%", true))
                    .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].FailPer) + "%", true))
                    .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].BrokenPer) + "%", true));
            
                await ctx.RespondAsync(embedBuilder);
            }

        }
        
        if(string.IsNullOrEmpty(upgradeCommand) || isFullList)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.istockphoto.com/id/607898530/photo/blacksmith-manually-forging-the-molten-metal.jpg?s=612x612&w=0&k=20&c=XJK8AuqbsehPFumor0RZGO4bd5s0M9MWInGixbzhw48=")
                .WithColor(DiscordColor.White)
                .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("[ 💍 ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("──────────", "[ 0 > 1 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 1 > 2 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 2 > 3 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 3 > 4 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].BrokenPer) + "%", true));

            await ctx.RespondAsync(embedBuilder);

            DiscordEmbedBuilder embedBuilder2 = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.istockphoto.com/id/607898530/photo/blacksmith-manually-forging-the-molten-metal.jpg?s=612x612&w=0&k=20&c=XJK8AuqbsehPFumor0RZGO4bd5s0M9MWInGixbzhw48=")
                .WithColor(DiscordColor.White)
                .AddField(new DiscordEmbedField("[ 🗡️ ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("[ 💍 ]", "[ \uD83D\uDCB0" + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("──────────", "[ 4 > 5 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 5 > 6 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 6 > 7 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 7 > 8 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 8 > 9 ]", false))
                .AddField(new DiscordEmbedField("[ 🟢 ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 🔴 ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ 💥 ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].BrokenPer) + "%", true));

            await ctx.RespondAsync(embedBuilder2);
        }
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🧾"));
    }

    [Command]
    public async Task SetWeaponUpgradeMoney(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = "❌";
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int setMoney = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out setMoney);
            }

            EquipCalculator.SetWeaponUpgradeMoney(setMoney);
            result = true;
            emoji = "✅";
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task SetRingUpgradeMoney(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = "❌";
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int setMoney = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out setMoney);
            }

            EquipCalculator.SetRingUpgradeMoney(setMoney);
            result = true;
            emoji = "✅";
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }

    [Command] // ToggleBossChannel
    public async Task Bbbb(CommandContext ctx)
    {
        bool result = false;
        string emoji = "❌";
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (ContentsChannels.BossChannels.Contains(ctx.Channel.Id))
            {
                ContentsChannels.BossChannels.Remove(ctx.Channel.Id);
                emoji = "❌";
            }
            else
            {
                ContentsChannels.BossChannels.Add(ctx.Channel.Id);
                emoji = "✅";
            }

            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task DataReset(CommandContext ctx, [RemainingText] string? resetCommand)
    {
        bool result = false;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            
            if (string.IsNullOrEmpty(resetCommand) || "all" == resetCommand)
            {
                _bossMonster.ResetBossMonster();
                
                bool killResult = await database.ResetBossKillCount(ctx);
                bool totalDamageResult = await database.ResetBossTotalDamage(ctx);
                bool goldResult = await database.ResetGold(ctx);

                result = killResult && totalDamageResult && goldResult;
            }
            else if ("gold" == resetCommand)
            {
                result = await database.ResetGold(ctx);
            }
            else if ("kill" == resetCommand)
            {
                result = await database.ResetBossKillCount(ctx);
            }
            else if ("totaldamage" == resetCommand)
            {
                result = await database.ResetBossTotalDamage(ctx);
            }
            else if ("combatcount" == resetCommand)
            {
                result = await database.ResetCombatCount(ctx);
            }
            else if ("equipvalue" == resetCommand)
            {
                result = await database.ResetEquipValue(ctx);
            }
            else if ("boss" == resetCommand)
            {
                _bossMonster.ResetBossMonster();
                result = true;
            }
            
            if (result)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
            }
        }
    }
}