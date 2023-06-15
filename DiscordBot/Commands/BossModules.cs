using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class BossModules : BaseCommandModule
{
    private readonly SortedSet<ulong> _bossChannels = new();
    
    private readonly BossMonster _bossMonster;

    public BossModules()
    {
        var rand = new Random();
        int bossType = rand.Next((int)BossType.Start + 1, (int) BossType.End);
        _bossMonster = new BossMonster((BossType)bossType);
    }
    
    //[Command, Aliases("ba")]
    [Command, Aliases("ba", "보스공격"), Cooldown(1, 300, CooldownBucketType.UserAndChannel, true, true, 10)]
    public async Task BossAttack(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        if (!_bossChannels.Contains(ctx.Channel.Id))
        {
            var message = await ctx.RespondAsync("보스공격이 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        var rand = new Random();

        // start,, calc final damage
        int missPer = 10;
        int critPer = 15;
        int massacrePer = 1;
        int attackPer = 100 - missPer - critPer - massacrePer;
        int FinalDamage = rand.Next(1, 101);
        int AttackChance = rand.Next(1, 101);
        string CritAddText = "";
        string DamageTypeEmojiCode = "\uD83D\uDCA5 ";
        string AttackGifurl = "https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif";

        if (missPer >= AttackChance) // miss
        {
            FinalDamage = 0;
            DamageTypeEmojiCode = "\ud83d\ude35\u200d\ud83d\udcab ";
            AttackGifurl = "https://media.tenor.com/ov3Jx6Fu-6kAAAAM/dark-souls-dance.gif";
        }
        else
        {
            // attack
            
            if (missPer + attackPer < AttackChance) // critical
            {
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
        
        bool bIsOverKill = FinalDamage >= _bossMonster.CurrentHp;

        string name = Utility.GetMemberDisplayName(ctx.Member);

        // hit embed
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail(AttackGifurl)
            .WithColor(DiscordColor.HotPink)
            .WithAuthor("\u2694\uFE0F " + name)
            .AddField(new DiscordEmbedField(DamageTypeEmojiCode + Convert.ToString(FinalDamage) + CritAddText + "    +\uD83D\uDCB0",
                _bossMonster.BossEmojiCode + " " + Convert.ToString(bIsOverKill ? 0 : _bossMonster.CurrentHp - FinalDamage) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        await ctx.RespondAsync(embedBuilder);

        // add parser
        int validDamage = bIsOverKill ? _bossMonster.CurrentHp : FinalDamage;
        
        // dead check
        int hitCount = _bossMonster.HitCount;
        int killedBossGetGold = 777 == _bossMonster.CurrentMaxHp ? 7777 : _bossMonster.CurrentMaxHp;
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<string, int> bestDealerInfo;
        
        if( _bossMonster.IsKilledByDamage(name, FinalDamage, out bestDealerInfo) )
        {
            DiscordEmbedBuilder killEmbedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("[ \uD83C\uDF8A" + name + "\uD83C\uDF8A ]  +\uD83D\uDCB0" + Convert.ToString(killedBossGetGold) )
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " \u2694\uFE0F " + Convert.ToString(hitCount + 1), 
                    "\uD83E\uDD47" + bestDealerInfo.Key + "   " + "\uD83D\uDCA5" + Convert.ToString(bestDealerInfo.Value),
                    false));

            
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.GetDatabaseUser(ctx.Guild, ctx.User);
            BossQuery query = new BossQuery((ulong)validDamage, 1, killedBossGetGold + validDamage, 1);
            await database.UpdateBossRaid(ctx, query);
        
            var message = await ctx.Channel.SendMessageAsync(killEmbedBuilder);

            // if( message != null )
            // {
            //     await message.PinAsync();
            // }
        }
        else
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.GetDatabaseUser(ctx.Guild, ctx.User);
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
        
        Dictionary<string, int> combatCountRankDictionary = users.Where(user => user.combatcount > 0).OrderByDescending(user => user.combatcount).ToDictionary(user =>
        {
            if (ctx.Guild.Members.TryGetValue(user.userid, out DiscordMember? member))
            {
                return Utility.GetMemberDisplayName(member);
            }
            return "X";
        }, user => user.combatcount);
        
        List<string> killRankUser = new List<string>();
        List<int> killRankCount = new List<int>();
        List<string> goldRankUser = new List<string>();
        List<int> goldRankCount = new List<int>();
        List<string> dealRankUser = new List<string>();
        List<ulong> dealRankCount = new List<ulong>();
        List<string> combatCountRankUser = new List<string>();
        List<int> combatCountRankCount = new List<int>();

        for (int index = 0; index < 3; ++index)
        {
            killRankUser.Add(index+1 <= killCountRankDictionary.Keys.ToList().Count ? killCountRankDictionary.Keys.ToList()[index] : "X");
            killRankCount.Add(index+1 <= killCountRankDictionary.Values.ToList().Count ? killCountRankDictionary.Values.ToList()[index] : 0);
            goldRankUser.Add(index+1 <= goldRankDictionary.Keys.ToList().Count ? goldRankDictionary.Keys.ToList()[index] : "X");
            goldRankCount.Add(index+1 <= goldRankDictionary.Values.ToList().Count ? goldRankDictionary.Values.ToList()[index] : 0);
            dealRankUser.Add(index+1 <= dealRankDictionary.Keys.ToList().Count ? dealRankDictionary.Keys.ToList()[index] : "X");
            dealRankCount.Add(index+1 <= dealRankDictionary.Values.ToList().Count ? dealRankDictionary.Values.ToList()[index] : 0);
            combatCountRankUser.Add(index+1 <= combatCountRankDictionary.Keys.ToList().Count ? combatCountRankDictionary.Keys.ToList()[index] : "X");
            combatCountRankCount.Add(index+1 <= combatCountRankDictionary.Values.ToList().Count ? combatCountRankDictionary.Values.ToList()[index] : 0);
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/1021/1021100.png")
            .WithColor(DiscordColor.Brown)
            .AddField(new DiscordEmbedField("────\uD83C\uDFC6────", "[  \uD83D\uDCB0  ]", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47" + goldRankUser[0], Convert.ToString(goldRankCount[0]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48" + goldRankUser[1], Convert.ToString(goldRankCount[1]), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49" + goldRankUser[2], Convert.ToString(goldRankCount[2]), true))
            // .AddField(new DiscordEmbedField("──────────", "[  \u2694\uFE0F  ]", false))
            // .AddField(new DiscordEmbedField("\uD83E\uDD47" + combatCountRankUser[0], Convert.ToString(combatCountRankCount[0]), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD48" + combatCountRankUser[1], Convert.ToString(combatCountRankCount[1]), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD49" + combatCountRankUser[2], Convert.ToString(combatCountRankCount[2]), true))
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
    
    [Command, Aliases("tbc")]
    public async Task ToggleBossChannel(CommandContext ctx)
    {
        bool result = false;
        string emoji = "❌";
        if (0 != (ctx.Member.Permissions & Permissions.Administrator))
        {
            if (_bossChannels.Contains(ctx.Channel.Id))
            {
                _bossChannels.Remove(ctx.Channel.Id);
                emoji = "❌";
            }
            else
            {
                _bossChannels.Add(ctx.Channel.Id);
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