using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Database;
using DiscordBot.Equip;
using DiscordBot.Resource;

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
        DatabaseUser attackUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(attackUserDatabase.equipvalue) * EquipCalculator.Boss_WeaponUpgradeMultiplier;
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(attackUserDatabase.equipvalue) * EquipCalculator.Boss_RingUpgradeMultiplier;
        
        var rand = new Random();
        
        int missPer = 10;
        int critPer = 15;
        int massacrePer = 1;
        int attackPer = 100 - missPer - critPer - massacrePer;
        int FinalDamage = rand.Next(1, 101);
        int AttackChance = rand.Next(1, 101);
        string CritAddText = "";
        string DamageTypeEmojiCode = VEmoji.Boom + " ";
        string AttackGifurl = "https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif";
        

        if (missPer >= AttackChance + ringUpgrade) // miss
        {
            weaponUpgrade = 0;
            FinalDamage = 0;
            DamageTypeEmojiCode = VEmoji.SpiralEyes + " ";
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
                DamageTypeEmojiCode = VEmoji.Fire + " ";
                AttackGifurl = "https://media.tenor.com/dhGo-zgViLoAAAAM/soul-dark.gif";
                
                if (missPer + attackPer + critPer < AttackChance) // massacre
                {
                    weaponUpgrade = 0;
                    FinalDamage = 9999;
                    CritAddText = " !!";
                    //DamageTypeEmojiCode = VEmoji.Fire + " ";
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
            .WithAuthor(VEmoji.CrossSword + " " + name + "    +" + VEmoji.Money)
            .AddField(new DiscordEmbedField(DamageTypeEmojiCode + Convert.ToString(FinalDamage) + CritAddText + weaponUpgradePlusText,
                _bossMonster.BossEmojiCode + " " + Convert.ToString(bIsOverKill ? 0 : _bossMonster.CurrentHp - (FinalDamage + weaponUpgrade)) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        await ctx.RespondAsync(embedBuilder);

        // add parser
        int validDamage = bIsOverKill ? _bossMonster.CurrentHp : (FinalDamage + weaponUpgrade);
        
        // dead check
        int hitCount = _bossMonster.HitCount;
        int killedBossGetGold = BossInfo.GetBossDropGold(_bossMonster.Type);
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<ulong, BossUserInfo> bestDealerInfo;

        BossUserInfo attackerInfo = new BossUserInfo();
        attackerInfo.Guild = ctx.Guild;
        attackerInfo.User = ctx.User;
        attackerInfo.Member = ctx.Member;
        attackerInfo.TotalDamage = FinalDamage + weaponUpgrade;
        
        if( _bossMonster.IsKilledByDamage(attackerInfo, out bestDealerInfo) )
        {
            DiscordEmbedBuilder killEmbedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("[ " + VEmoji.ConfettiBall + name + VEmoji.ConfettiBall + " ]  +" + VEmoji.Money + Convert.ToString(killedBossGetGold) )
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " " + VEmoji.CrossSword + " " + Convert.ToString(hitCount + 1), 
                    VEmoji.GoldMedal + Utility.GetMemberDisplayName(bestDealerInfo.Value.Member) + " " + VEmoji.Boom + Convert.ToString(bestDealerInfo.Value.TotalDamage) + " +" + VEmoji.Money, false));

            
            BossQuery query = new BossQuery((ulong)validDamage, 1, killedBossGetGold + validDamage, 1);
            await database.UpdateBossRaid(ctx, query);

            GoldQuery goldQuery = new GoldQuery(bestDealerInfo.Value.TotalDamage);
            await database.UpdateUserGold(bestDealerInfo.Value.Guild, bestDealerInfo.Value.User, goldQuery);
        
            await ctx.Channel.SendMessageAsync(killEmbedBuilder);
        }
        else
        {
            BossQuery query = new BossQuery((ulong)validDamage, 0, validDamage, 1);
            await database.UpdateBossRaid(ctx, query);
        }
    }
    
    [Command, Aliases("bi", "보스정보"), Cooldown(1, 3, CooldownBucketType.User)]
    public async Task BossHuntInfo(CommandContext ctx)
    {
        KeyValuePair<ulong, BossUserInfo> bestDealerInfo = _bossMonster.GetBestDealer();
        string bestDealer = 0 == bestDealerInfo.Key ? "X" : Utility.GetMemberDisplayName(bestDealerInfo.Value.Member);
        int bestDealerTotalDamage = 0 == bestDealerInfo.Key ? 0 : bestDealerInfo.Value.TotalDamage;
        
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://upload2.inven.co.kr/upload/2018/11/09/bbs/i15705696321.jpg?MW=800")
            .WithColor(DiscordColor.Orange)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField(_bossMonster.BossEmojiCode, VEmoji.Heart + " " + _bossMonster.CurrentHp + "/" + _bossMonster.CurrentMaxHp, false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + bestDealer + "   " + VEmoji.Boom + Convert.ToString(bestDealerTotalDamage),
                VEmoji.CrossSword + " " + _bossMonster.HitCount, false));
        
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
            .AddField(new DiscordEmbedField("────" + VEmoji.Trophy + "────", "[  " + VEmoji.Money + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + goldRankUser[0], Convert.ToString(goldRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + goldRankUser[1], Convert.ToString(goldRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + goldRankUser[2], Convert.ToString(goldRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Ring + ", " + VEmoji.Weapon + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + equipRankUser[0], "+" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[0])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[0])), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + equipRankUser[1], "+" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[1])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[1])), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + equipRankUser[2], "+" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[2])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[2])), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Crossbones + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + killRankUser[0], Convert.ToString(killRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + killRankUser[1], Convert.ToString(killRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + killRankUser[2], Convert.ToString(killRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Boom + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + dealRankUser[0], Convert.ToString(dealRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + dealRankUser[1], Convert.ToString(dealRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + dealRankUser[2], Convert.ToString(dealRankCount[2]), true));
        
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
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Mosquito) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Mosquito) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Mosquito), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Bat) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Bat) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Bat), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Octopus) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Octopus) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Octopus), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Shark) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Shark) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Shark), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Unicorn) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Unicorn) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Unicorn), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Skeleton) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Skeleton) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Skeleton), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Devil) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Devil) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Devil), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.SlotMachine) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.SlotMachine) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.SlotMachine), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Alien) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Alien) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Alien), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.AngryDevil) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.AngryDevil) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.AngryDevil), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Trex) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Trex) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Trex), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.MrKrabs) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.MrKrabs) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.MrKrabs), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.Dragon) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.Dragon) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.Dragon), false))
            .AddField(new DiscordEmbedField("[ "+ BossInfo.GetBossEmojiCode(BossType.TheOffice) + " ]", VEmoji.Heart + BossInfo.GetBossMaxHp(BossType.TheOffice) + ", " + VEmoji.Money + BossInfo.GetBossDropGold(BossType.TheOffice), false));
        
        await ctx.RespondAsync(embedBuilder);
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🧾"));
    }

    [Command, Aliases("uw", "무기강화")]
    public async Task UpgradeWeapon(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        if (!ContentsChannels.ForgeChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("강화가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        int weaponCurrentUpgrade = EquipCalculator.GetWeaponUpgradeInfo(userDatabase.equipvalue);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        if (9 <= weaponCurrentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " " + VEmoji.ThumbsUp);
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
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️0 ]", true));

                        var message = await ctx.RespondAsync(embedBuilder);
                        
                        if (message != null && 7 <= weaponCurrentUpgrade)
                        {
                            await message.PinAsync();
                        }
                        break;
                    }
                    case 0: // Fail
                    {
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Red)
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 1: // Success
                    {
                        await database.AddEquipValue(ctx, 1);

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Green)
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ +️" + Convert.ToString(weaponCurrentUpgrade+1) + " ]", true));

                        var message = await ctx.RespondAsync(embedBuilder);
                        
                        if (message != null && 6 <= weaponCurrentUpgrade)
                        {
                            await message.PinAsync();
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            else
            {
                await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
            }
        }
    }
    
    [Command, Aliases("ur", "반지강화")]
    public async Task UpgradeRing(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        if (!ContentsChannels.ForgeChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("강화가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        int ringCurrentUpgrade = EquipCalculator.GetRingUpgradeInfo(userDatabase.equipvalue);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        if (9 <= ringCurrentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " " + VEmoji.ThumbsUp);
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
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️0 ]", true));

                        var message = await ctx.RespondAsync(embedBuilder);
                        if (message != null && 7 <= ringCurrentUpgrade)
                        {
                            await message.PinAsync();
                        }
                        break;
                    }
                    case 0: // Fail
                    {
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Red)
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true));

                        await ctx.RespondAsync(embedBuilder);
                        break;
                    }
                    case 1: // Success
                    {
                        await database.AddEquipValue(ctx, 10);

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                            .WithColor(DiscordColor.Green)
                            .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", false))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️" + Convert.ToString(ringCurrentUpgrade) + " ]", true))
                            .AddField(new DiscordEmbedField("▶", "▶", true))
                            .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ +️" + Convert.ToString(ringCurrentUpgrade+1) + " ]", true));

                        var message = await ctx.RespondAsync(embedBuilder);
                        if (message != null && 6 <= ringCurrentUpgrade)
                        {
                            await message.PinAsync();
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            else
            {
                await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
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
                    .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                    .AddField(new DiscordEmbedField("──────────", "[ "+ Convert.ToString(nowStep) + " > " + Convert.ToString(nowStep + 1) + " ]", false))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].SuccessPer) + "%", true))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].FailPer) + "%", true))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[nowStep].BrokenPer) + "%", true));
            
                await ctx.RespondAsync(embedBuilder);
            }

        }
        
        if(string.IsNullOrEmpty(upgradeCommand) || isFullList)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.istockphoto.com/id/607898530/photo/blacksmith-manually-forging-the-molten-metal.jpg?s=612x612&w=0&k=20&c=XJK8AuqbsehPFumor0RZGO4bd5s0M9MWInGixbzhw48=")
                .WithColor(DiscordColor.White)
                .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("──────────", "[ 0 > 1 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[0].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 1 > 2 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[1].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 2 > 3 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[2].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 3 > 4 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[3].BrokenPer) + "%", true));

            await ctx.RespondAsync(embedBuilder);

            DiscordEmbedBuilder embedBuilder2 = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.istockphoto.com/id/607898530/photo/blacksmith-manually-forging-the-molten-metal.jpg?s=612x612&w=0&k=20&c=XJK8AuqbsehPFumor0RZGO4bd5s0M9MWInGixbzhw48=")
                .WithColor(DiscordColor.White)
                .AddField(new DiscordEmbedField("[ " + VEmoji.Ring + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.RingUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Weapon + " ]", "[ " + VEmoji.Money + Convert.ToString(EquipCalculator.WeaponUpgradeMoney) + " ]", true))
                .AddField(new DiscordEmbedField("──────────", "[ 4 > 5 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[4].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 5 > 6 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[5].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 6 > 7 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[6].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 7 > 8 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[7].BrokenPer) + "%", true))
                .AddField(new DiscordEmbedField("──────────", "[ 8 > 9 ]", false))
                .AddField(new DiscordEmbedField("[ " + VEmoji.GreenCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].SuccessPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.RedCircle + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].FailPer) + "%", true))
                .AddField(new DiscordEmbedField("[ " + VEmoji.Boom + " ]", Convert.ToString(EquipCalculator.UpgradePercentages[8].BrokenPer) + "%", true));

            await ctx.RespondAsync(embedBuilder2);
        }
        
        //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🧾"));
    }

    [Command]
    public async Task SetWeaponUpgradeMoney(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int setMoney = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out setMoney);
            }

            EquipCalculator.SetWeaponUpgradeMoney(setMoney);
            result = true;
            emoji = VEmoji.GreenCheckBox;
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
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int setMoney = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out setMoney);
            }

            EquipCalculator.SetRingUpgradeMoney(setMoney);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    
    [Command]
    public async Task SetBossWeaponMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int value = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out value);
            }

            EquipCalculator.SetBoss_WeaponUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task SetFishWeaponMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int value = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out value);
            }

            EquipCalculator.SetFish_WeaponUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task SetBossRingMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int value = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out value);
            }

            EquipCalculator.SetBoss_RingUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task SetDiceRingMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int value = 0;
            if( !string.IsNullOrEmpty(setCommand))
            {
                Int32.TryParse(setCommand, out value);
            }

            EquipCalculator.SetDice_RingUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
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
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (ContentsChannels.BossChannels.Contains(ctx.Channel.Id))
            {
                ContentsChannels.BossChannels.Remove(ctx.Channel.Id);
                emoji = VEmoji.RedCrossMark;
            }
            else
            {
                ContentsChannels.BossChannels.Add(ctx.Channel.Id);
                emoji = VEmoji.GreenCheckBox;
            }

            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }

    [Command]
    public async Task ResetBossMonster(CommandContext ctx, [RemainingText] string? resetCommand)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            int resetBossType = 0;
            Int32.TryParse(resetCommand, out resetBossType);

            _bossMonster.ResetBossMonster(resetBossType);
            
            emoji = VEmoji.GreenCheckBox;
            result = true;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }

    [Command] // ToggleForgeChannel
    public async Task Uuuu(CommandContext ctx)
    {
        bool result = false;
        string emoji = VEmoji.RedCrossMark;
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (ContentsChannels.ForgeChannels.Contains(ctx.Channel.Id))
            {
                ContentsChannels.ForgeChannels.Remove(ctx.Channel.Id);
                emoji = VEmoji.RedCrossMark;
            }
            else
            {
                ContentsChannels.ForgeChannels.Add(ctx.Channel.Id);
                emoji = VEmoji.GreenCheckBox;
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
                bool combatCountResult = await database.ResetCombatCount(ctx);
                bool equipValueResult = await database.ResetEquipValue(ctx);

                result = killResult && totalDamageResult && goldResult && combatCountResult && equipValueResult;
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
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.GreenCheckBox));
            }
        }
    }
}