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

public class BossModules : BaseCommandModule
{   
    private readonly BossMonster _bossMonster;
    private readonly ContentsChannels _contentsChannels;

    public BossModules(ContentsChannels contentsChannels)
    {
        _contentsChannels = contentsChannels;
        var rand = new Random();
        int bossType = rand.Next((int)BossType.Start + 1, (int) BossType.End);
        _bossMonster = new BossMonster((BossType)bossType);
    }
    
    //[Command, Aliases("ba")]
    [Command, Aliases("ba", "공격", "보스공격"), Cooldown(1, 300, CooldownBucketType.UserAndChannel, true, true, 10)]
    public async Task A1_BossAttack(CommandContext ctx, [RemainingText] string? tempCommand)
    {
        bool isBossGameChannel = await _contentsChannels.IsBossGameChannel(ctx);
        if (isBossGameChannel == false)
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
        int tridentUpgrade = EquipCalculator.GetTridentUpgradeInfo(attackUserDatabase.equipvalue) * 9;
        int weaponUpgrade = (EquipCalculator.GetWeaponUpgradeInfo(attackUserDatabase.equipvalue) + tridentUpgrade) * EquipCalculator.Boss_WeaponUpgradeMultiplier;
        int ringUpgrade = (EquipCalculator.GetRingUpgradeInfo(attackUserDatabase.equipvalue) + tridentUpgrade) * EquipCalculator.Boss_RingUpgradeMultiplier;
        int gemUpgrade = (EquipCalculator.GetGemUpgradeInfo(attackUserDatabase.equipvalue) + tridentUpgrade) * EquipCalculator.Gold_GemUpgradeMultiplier;
        float gemPercentage = gemUpgrade / 100.0f;
        
        var rand = new Random();
        
        int missPer = 10;
        int critPer = 15;
        int massacrePer = 1;
        int attackPer = 100 - missPer - critPer - massacrePer;
        int finalDamage = rand.Next(1, 101);
        int attackChance = rand.Next(1, 101);
        string critAddText = "";
        string damageTypeEmojiCode = VEmoji.Boom + " ";
        string attackGifurl = "https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif";
        

        if (missPer >= attackChance + ringUpgrade) // miss
        {
            weaponUpgrade = 0;
            finalDamage = 0;
            damageTypeEmojiCode = VEmoji.SpiralEyes + " ";
            attackGifurl = "https://media.tenor.com/ov3Jx6Fu-6kAAAAM/dark-souls-dance.gif";
        }
        else
        {
            // attack
            
            if (missPer + attackPer < attackChance + ringUpgrade) // critical
            {
                weaponUpgrade *= 2;
                finalDamage = finalDamage * 2 + 100;
                critAddText = " !";
                damageTypeEmojiCode = VEmoji.Fire + " ";
                attackGifurl = "https://media.tenor.com/dhGo-zgViLoAAAAM/soul-dark.gif";
                
                if (missPer + attackPer + critPer < attackChance) // massacre
                {
                    weaponUpgrade = 0;
                    finalDamage = 9999;
                    critAddText = " !!";
                    //DamageTypeEmojiCode = VEmoji.Fire + " ";
                    attackGifurl = "https://media.tenor.com/8ZdT_rjqHzcAAAAd/dark-souls-gwyn.gif";
                }
            }
        }
        // end,, calc final damage
        
        string weaponUpgradePlusText = 0 < weaponUpgrade ? " +" + Convert.ToString(weaponUpgrade) + "🗡️": "";
        
        bool bIsOverKill = (finalDamage + weaponUpgrade) >= _bossMonster.CurrentHp;

        string name = Utility.GetMemberDisplayName(ctx.Member);

        // hit embed
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail(attackGifurl)
            .WithColor(DiscordColor.HotPink)
            .WithAuthor(VEmoji.CrossSword + " " + name + "    +" + VEmoji.Money)
            .AddField(new DiscordEmbedField(damageTypeEmojiCode + Convert.ToString(finalDamage) + critAddText + weaponUpgradePlusText,
                _bossMonster.BossEmojiCode + " " + Convert.ToString(bIsOverKill ? 0 : _bossMonster.CurrentHp - (finalDamage + weaponUpgrade)) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        await ctx.RespondAsync(embedBuilder);

        // add parser
        int validDamage = bIsOverKill ? _bossMonster.CurrentHp : (finalDamage + weaponUpgrade);
        
        // dead check
        int hitCount = _bossMonster.HitCount;
        int killedBossGetGold = BossInfo.GetBossDropGold(_bossMonster.Type);
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<ulong, BossUserInfo> bestDealerInfo;

        BossUserInfo attackerInfo = new BossUserInfo();
        attackerInfo.Guild = ctx.Guild;
        attackerInfo.User = ctx.User;
        attackerInfo.Member = ctx.Member;
        attackerInfo.TotalDamage = finalDamage + weaponUpgrade;
        
        if( _bossMonster.IsKilledByDamage(attackerInfo, out bestDealerInfo) )
        {
            DiscordEmbedBuilder killEmbedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("[ " + VEmoji.ConfettiBall + name + VEmoji.ConfettiBall + " ]  +" + VEmoji.Money + Convert.ToString(killedBossGetGold) )
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " " + VEmoji.CrossSword + " " + Convert.ToString(hitCount + 1), 
                    VEmoji.GoldMedal + Utility.GetMemberDisplayName(bestDealerInfo.Value.Member) + " " + VEmoji.Boom + Convert.ToString(bestDealerInfo.Value.TotalDamage) + " +" + VEmoji.Money, false));

            float addGemPercentageMoney = (killedBossGetGold + validDamage) * (1.0f + gemPercentage);
            BossQuery query = new BossQuery((ulong)validDamage, 1, (int)addGemPercentageMoney, 1);
            await database.UpdateBossRaid(ctx, query);

            DatabaseUser bestDealerUserDatabase = await database.GetDatabaseUser(bestDealerInfo.Value.Guild, bestDealerInfo.Value.User);
            int bestDealerGemUpgrade = EquipCalculator.GetGemUpgradeInfo(bestDealerUserDatabase.equipvalue);
            float bestDealerGemPercentage = (bestDealerGemUpgrade * EquipCalculator.Gold_GemUpgradeMultiplier) / 100.0f;
            float bestDealerAddGemPercentageMoney = bestDealerInfo.Value.TotalDamage * (1.0f + bestDealerGemPercentage);
            
            GoldQuery goldQuery = new GoldQuery((int)bestDealerAddGemPercentageMoney);
            await database.UpdateUserGold(bestDealerInfo.Value.Guild, bestDealerInfo.Value.User, goldQuery);
        
            await ctx.Channel.SendMessageAsync(killEmbedBuilder);
        }
        else
        {
            float addGemPercentageMoney = validDamage * (1.0f + gemPercentage);
            BossQuery query = new BossQuery((ulong)validDamage, 0, (int)addGemPercentageMoney, 1);
            await database.UpdateBossRaid(ctx, query);
        }
    }
    
    [Command, Aliases("bi", "보스정보"), Cooldown(1, 3, CooldownBucketType.User)]
    public async Task A2_BossHuntInfo(CommandContext ctx)
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
    
    [Command, Aliases("bl", "보스리스트"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task A3_BossList(CommandContext ctx)
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

    [Command, Hidden]
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
    
    [Command, Hidden]
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