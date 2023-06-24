using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Database;
using DiscordBot.Database.Tables;
using DiscordBot.Equip;
using DiscordBot.Resource;

namespace DiscordBot.Commands;

public class UserGameInfoModules : BaseCommandModule
{   
    private readonly ContentsChannels _contentsChannels;

    public UserGameInfoModules(ContentsChannels contentsChannels)
    {
        _contentsChannels = contentsChannels;
    }

    [Command, Aliases("mi", "내정보"), Cooldown(1, 5, CooldownBucketType.User)]
    public async Task MyInfo(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser myUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(myUserDatabase.equipvalue);
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(myUserDatabase.equipvalue);
        int gemUpgrade = EquipCalculator.GetGemUpgradeInfo(myUserDatabase.equipvalue);
        int level = EquipCalculator.GetLevel(myUserDatabase.equipvalue);
        int xp = EquipCalculator.GetXp(myUserDatabase.equipvalue);
        int xpPercentage = 0;
        if (0 != xp)
        {
            float xpPercentageFloat = (float) xp / level;
            xpPercentage = (int)(xpPercentageFloat * 100.0f);   
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/943/943579.png")
            .WithColor(DiscordColor.Black)
            .AddField(new DiscordEmbedField(VEmoji.Magnifier + " " + name, "───────────────", false))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Level + "  ]", "Lv " + Convert.ToString(level), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Books + "  ]", Convert.ToString(xpPercentage) + "%", true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Money + "  ]", Convert.ToString(myUserDatabase.gold), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Gem + "  ]", "+" + Convert.ToString(gemUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Ring + "  ]", "+" + Convert.ToString(ringUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Weapon + "  ]", "+" + Convert.ToString(weaponUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Crossbones + "  ]", Convert.ToString(myUserDatabase.bosskillcount), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.CrossSword + "  ]", Convert.ToString(myUserDatabase.combatcount), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Boom + "  ]", Convert.ToString(myUserDatabase.bosstotaldamage), true));
        
        await ctx.RespondAsync(embedBuilder);
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
        
        Dictionary<string, int> equipRankDictionary = users.Where(user => user.equipvalue > 0).OrderByDescending(user => user.equipvalue % EquipCalculator.LevelCutNum).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.equipvalue);
        
        Dictionary<string, int> levelRankDictionary = users.Where(user => user.equipvalue > 0).OrderByDescending(user => user.equipvalue / EquipCalculator.LevelCutNum).ToDictionary(user =>
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
        List<string> levelRankUser = new List<string>();
        List<int> levelRankCount = new List<int>();

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
            levelRankUser.Add(index+1 <= levelRankDictionary.Keys.ToList().Count ? levelRankDictionary.Keys.ToList()[index] : "X");
            levelRankCount.Add(index+1 <= levelRankDictionary.Values.ToList().Count ? levelRankDictionary.Values.ToList()[index] : 0);
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1021/1021100.png")
            .WithColor(DiscordColor.Brown)
            .AddField(new DiscordEmbedField("────" + VEmoji.Trophy + "────", "[  " + VEmoji.Level + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + levelRankUser[0], "Lv " + Convert.ToString(EquipCalculator.GetLevel(levelRankCount[0])), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + levelRankUser[1], "Lv " + Convert.ToString(EquipCalculator.GetLevel(levelRankCount[1])), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + levelRankUser[2], "Lv " + Convert.ToString(EquipCalculator.GetLevel(levelRankCount[2])), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Money + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + goldRankUser[0], Convert.ToString(goldRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + goldRankUser[1], Convert.ToString(goldRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + goldRankUser[2], Convert.ToString(goldRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Trident + ", " + VEmoji.Gem + ", " + VEmoji.Ring + ", " + VEmoji.Weapon + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + equipRankUser[0], "+" + Convert.ToString(EquipCalculator.GetTridentUpgradeInfo(equipRankCount[0])) + ", +" + Convert.ToString(EquipCalculator.GetGemUpgradeInfo(equipRankCount[0])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[0])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[0])), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + equipRankUser[1], "+" + Convert.ToString(EquipCalculator.GetTridentUpgradeInfo(equipRankCount[1])) + ", +" + Convert.ToString(EquipCalculator.GetGemUpgradeInfo(equipRankCount[1])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[1])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[1])), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + equipRankUser[2], "+" + Convert.ToString(EquipCalculator.GetTridentUpgradeInfo(equipRankCount[2])) + ", +" + Convert.ToString(EquipCalculator.GetGemUpgradeInfo(equipRankCount[2])) + ", +" + Convert.ToString(EquipCalculator.GetRingUpgradeInfo(equipRankCount[2])) + ", +" + Convert.ToString(EquipCalculator.GetWeaponUpgradeInfo(equipRankCount[2])), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Crossbones + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + killRankUser[0], Convert.ToString(killRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + killRankUser[1], Convert.ToString(killRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + killRankUser[2], Convert.ToString(killRankCount[2]), true))
            .AddField(new DiscordEmbedField("──────────", "[  " + VEmoji.Boom + "  ]", false))
            .AddField(new DiscordEmbedField(VEmoji.GoldMedal + dealRankUser[0], Convert.ToString(dealRankCount[0]), true))
            .AddField(new DiscordEmbedField(VEmoji.SilverMedal + dealRankUser[1], Convert.ToString(dealRankCount[1]), true))
            .AddField(new DiscordEmbedField(VEmoji.BronzeMedal + dealRankUser[2], Convert.ToString(dealRankCount[2]), true));
        
        await ctx.RespondAsync(embedBuilder);
    }

    [Command, Aliases("ut", "삼지창강화")]
    public async Task UpgradeTrident(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        bool isForgeChannel = await _contentsChannels.IsForgeChannel(ctx);
        if (isForgeChannel == false)
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
        DatabaseUser userDatabase = await database.GetDatabaseUser(ctx.Guild, ctx.User);

        int tridentUpgrade = EquipCalculator.GetTridentUpgradeInfo(userDatabase.equipvalue);
        int gemUpgrade = EquipCalculator.GetGemUpgradeInfo(userDatabase.equipvalue);
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(userDatabase.equipvalue);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(userDatabase.equipvalue);

        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        if (9 <= tridentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " " + VEmoji.ThumbsUp);
        }
        else
        {
            if (27 <= gemUpgrade + ringUpgrade + weaponUpgrade)
            {
                await database.AddEquipValue(ctx, 1);

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithThumbnail("https://media.tenor.com/GGsOGJnPnvgAAAAM/aquaman-jason.gif")
                    .WithColor(DiscordColor.Green)
                    .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "────────", false))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.Trident + " ]", "[ +️" + Convert.ToString(tridentUpgrade) + " ]", true))
                    .AddField(new DiscordEmbedField("▶", "▶", true))
                    .AddField(new DiscordEmbedField("[ " + VEmoji.Trident + " ]", "[ +️" + Convert.ToString(tridentUpgrade+1) + " ]", true));

                var message = await ctx.RespondAsync(embedBuilder);
                await message.PinAsync();
            }
            else
            {
                await ctx.RespondAsync(".." + VEmoji.QuestionMark + "(+9" + VEmoji.Gem + ", +9" + VEmoji.Ring + ", +9" + VEmoji.Weapon + ")");
            }
        }
    }

    [Command, Aliases("uw", "무기강화")]
    public async Task UpgradeWeapon(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        bool isForgeChannel = await _contentsChannels.IsForgeChannel(ctx);
        if (isForgeChannel == false)
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
        bool isForgeChannel = await _contentsChannels.IsForgeChannel(ctx);
        if (isForgeChannel == false)
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
                        await database.AddEquipValue(ctx, -(ringCurrentUpgrade * EquipCalculator.EquipCutNum));

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
                        await database.AddEquipValue(ctx, EquipCalculator.EquipCutNum);

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
    
    [Command, Aliases("ug", "보석강화"), Cooldown(1, 1800, CooldownBucketType.UserAndChannel, true)]
    public async Task UpgradeGem(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        bool isForgeChannel = await _contentsChannels.IsForgeChannel(ctx);
        if (isForgeChannel == false)
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
        
        int gemCurrentUpgrade = EquipCalculator.GetGemUpgradeInfo(userDatabase.equipvalue);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        if (9 <= gemCurrentUpgrade)
        {
            await ctx.RespondAsync(ctx.Member.Mention + " " + VEmoji.ThumbsUp);
        }
        else
        {
            int upgradeResult = EquipCalculator.Upgrade(gemCurrentUpgrade);

            switch (upgradeResult)
            {
                case -1: // Broken
                {
                    await database.AddEquipValue(ctx, -(gemCurrentUpgrade * EquipCalculator.EquipCutNum * EquipCalculator.EquipCutNum));

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                        .WithThumbnail("https://social-phinf.pstatic.net/20210407_47/161775296734159xKI_GIF/1787c8c2dd04baebd123123312312.gif")
                        .WithColor(DiscordColor.DarkRed)
                        .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "────────", false))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️" + Convert.ToString(gemCurrentUpgrade) + " ]", true))
                        .AddField(new DiscordEmbedField("▶", "▶", true))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️0 ]", true));

                    var message = await ctx.RespondAsync(embedBuilder);
                    if (message != null && 7 <= gemCurrentUpgrade)
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
                        .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "────────", false))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️" + Convert.ToString(gemCurrentUpgrade) + " ]", true))
                        .AddField(new DiscordEmbedField("▶", "▶", true))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️" + Convert.ToString(gemCurrentUpgrade) + " ]", true));

                    await ctx.RespondAsync(embedBuilder);
                    break;
                }
                case 1: // Success
                {
                    await database.AddEquipValue(ctx, EquipCalculator.EquipCutNum * EquipCalculator.EquipCutNum);

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                        .WithThumbnail("https://media.tenor.com/FBQM1OsZwwAAAAAd/gwent-gwentcard.gif")
                        .WithColor(DiscordColor.Green)
                        .AddField(new DiscordEmbedField(VEmoji.HammerAndPick + " " + name, "────────", false))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️" + Convert.ToString(gemCurrentUpgrade) + " ]", true))
                        .AddField(new DiscordEmbedField("▶", "▶", true))
                        .AddField(new DiscordEmbedField("[ " + VEmoji.Gem + " ]", "[ +️" + Convert.ToString(gemCurrentUpgrade + 1) + " ]", true));

                    var message = await ctx.RespondAsync(embedBuilder);
                    if (message != null && 6 <= gemCurrentUpgrade)
                    {
                        await message.PinAsync();
                    }

                    break;
                }
                default:
                    break;
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
    }

    [Command, Aliases("ggp", "보석수급"), Cooldown(1, 3600, CooldownBucketType.User, true)]
    public async Task GetGemPay(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int tridentUpgrade = EquipCalculator.GetTridentUpgradeInfo(userDatabase.equipvalue) * 9;
        int gemCurrentUpgrade = EquipCalculator.GetGemUpgradeInfo(userDatabase.equipvalue) + tridentUpgrade;

        int gemPay = gemCurrentUpgrade * EquipCalculator.Pay_GemUpgradeMultiplier;
        
        GoldQuery query = new GoldQuery(gemPay);
        await database.UpdateUserGold(ctx, query);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://i.pinimg.com/originals/36/6f/10/366f10fa1064662651463d3f058854c6.gif")
            .WithColor(DiscordColor.Gold)
            .AddField(new DiscordEmbedField(VEmoji.Gem + " " + name, "[ + " + VEmoji.Money + Convert.ToString(gemPay) + " ]" ));
        
        await ctx.RespondAsync(embedBuilder);
    }
    
    [Command, Aliases("bxp", "경험치구매")]
    public async Task BuyXp(CommandContext ctx, [RemainingText] string? xpCommand)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser userDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        
        if (EquipCalculator.LevelUpgradeMoney > userDatabase.gold)
        {
            await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark + "(" + Convert.ToString(EquipCalculator.LevelUpgradeMoney) + ")");
            return;
        }
        
        int level = EquipCalculator.GetLevel(userDatabase.equipvalue);
        int xp = EquipCalculator.GetXp(userDatabase.equipvalue);

        if (level <= xp + 1)
        {
            // level
            await database.AddEquipValue(ctx, EquipCalculator.LevelCutNum * EquipCalculator.XpCutNum);
            // xp
            await database.AddEquipValue(ctx, -(xp * EquipCalculator.LevelCutNum));
        }
        else
        {
            // xp
            await database.AddEquipValue(ctx, EquipCalculator.LevelCutNum);
        }

        GoldQuery query = new GoldQuery(-EquipCalculator.LevelUpgradeMoney);
        await database.UpdateUserGold(ctx, query);
        
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        DatabaseUser afterUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);
        int afterLevel = EquipCalculator.GetLevel(afterUserDatabase.equipvalue);
        int afterXp = EquipCalculator.GetXp(afterUserDatabase.equipvalue);
        int xpPercentage = 0;
        if (0 != afterXp)
        {
            float xpPercentageFloat = (float) afterXp / afterLevel;
            xpPercentage = (int)(xpPercentageFloat * 100.0f);   
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://upload2.inven.co.kr/upload/2017/04/12/bbs/i16195673110.gif")
            .WithColor(DiscordColor.Green)
            .AddField(new DiscordEmbedField(name + " " + VEmoji.Books + " ..!", "[ - " + VEmoji.Money + Convert.ToString(EquipCalculator.LevelUpgradeMoney) + " ]", false))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Level + "  ]", "Lv " + Convert.ToString(afterLevel), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Books + "  ]", Convert.ToString(xpPercentage) + "%", true));
        
        await ctx.RespondAsync(embedBuilder);
    }
}