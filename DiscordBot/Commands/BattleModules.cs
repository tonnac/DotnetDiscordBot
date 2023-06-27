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

public class BattleModules : BaseCommandModule
{
    private readonly ContentsChannels _contentsChannels;
    
    public BattleModules(ContentsChannels contentsChannels)
    {
        _contentsChannels = contentsChannels;
    }
    
    [Command, Aliases("br", "전투초기화")]
    public async Task A4_BattleReset(CommandContext ctx)
    {
        bool isBattleChannel = await _contentsChannels.IsBattleChannel(ctx);
        if (isBattleChannel == false)
        {
            var message = await ctx.RespondAsync("전투가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }

        if (0 != BattleSystem.FightMoney)
        {
            if (BattleSystem.IsFighting)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.RedCrossMark));
                return;
            }
            
            if (BattleSystem.IsA_Ready)
            {
                using var database = new DiscordBotDatabase();
                await database.ConnectASync();
                await database.GetDatabaseUser(BattleSystem.User_A.Guild, BattleSystem.User_A.User);
            
                GoldQuery query = new GoldQuery(BattleSystem.FightMoney);
                await database.UpdateUserGold(BattleSystem.User_A.Guild, BattleSystem.User_A.User, query);
            }
            if (BattleSystem.IsB_Ready)
            {
                using var database = new DiscordBotDatabase();
                await database.ConnectASync();
                await database.GetDatabaseUser(BattleSystem.User_B.Guild, BattleSystem.User_B.User);
            
                GoldQuery query = new GoldQuery(BattleSystem.FightMoney);
                await database.UpdateUserGold(BattleSystem.User_B.Guild, BattleSystem.User_B.User, query);
            }
        }
        
        BattleSystem.ResetSystem();

        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.GreenCheckBox));
    }

    [Command, Aliases("bj", "전투참여")]
    public async Task A1_BattleJoin(CommandContext ctx, [RemainingText] string? battleCommand)
    {
        BattleJoin(ctx, battleCommand, true);
    }
    
    [Command, Aliases("bbj", "기본전투참여")]
    public async Task A2_BasicBattleJoin(CommandContext ctx, [RemainingText] string? battleCommand)
    {
        BattleJoin(ctx, battleCommand, false);
    }

    public async void BattleJoin(CommandContext ctx, [RemainingText] string? battleCommand, bool isItemBattle)
    {
        bool isBattleChannel = await _contentsChannels.IsBattleChannel(ctx);
        if (isBattleChannel == false)
        {
            var message = await ctx.RespondAsync("전투가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        if (BattleSystem.IsA_Ready && BattleSystem.IsB_Ready)
        {
            await ctx.RespondAsync(VEmoji.CrossSword + " all ready... ");
            return;
        }

        if (0 == BattleSystem.FightMoney && !BattleSystem.IsA_Ready && !string.IsNullOrEmpty(battleCommand))
        {
            int tempFightMoney = 0;
            Int32.TryParse(battleCommand, out tempFightMoney);
            tempFightMoney = Math.Max(0, tempFightMoney);
            
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            DatabaseUser battleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);

            if (tempFightMoney > battleUserDatabase.gold)
            {
                await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
                return;
            }
            
            BattleSystem.FightMoney = tempFightMoney;
            
            GoldQuery query = new GoldQuery(-BattleSystem.FightMoney );
            await database.UpdateUserGold(ctx, query);
        }
        else if (0 != BattleSystem.FightMoney)
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            DatabaseUser battleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);

            if (BattleSystem.FightMoney > battleUserDatabase.gold)
            {
                await ctx.RespondAsync(VEmoji.Money + ".. " + VEmoji.QuestionMark);
                return;
            }
            
            GoldQuery query = new GoldQuery(-BattleSystem.FightMoney );
            await database.UpdateUserGold(ctx, query);
        }

        string fightMoneyText = 0 == BattleSystem.FightMoney ? "" : " + " + VEmoji.Money;
        
        if (!BattleSystem.IsA_Ready)
        {
            BattleSystem.IsA_BasicReady = !isItemBattle;
            BattleSystem.User_A.SetUserBattleInfo(ctx.Guild, ctx.User, ctx.Member, isItemBattle);
            BattleSystem.IsA_Ready = true;
            await ctx.RespondAsync(VEmoji.A + "️ Ready !" + fightMoneyText);
        }
        else if (!BattleSystem.IsB_Ready)
        {
            BattleSystem.User_B.SetUserBattleInfo(ctx.Guild, ctx.User, ctx.Member, BattleSystem.IsA_BasicReady ? false : isItemBattle);
            BattleSystem.IsB_Ready = true;
            await ctx.RespondAsync(VEmoji.B + "️ Ready !" + fightMoneyText);
        }
    }

    [Command, Aliases("bs", "전투시작")]
    public async Task A3_BattleStart(CommandContext ctx, [RemainingText] string? battleCommand)
    {
        bool isBattleChannel = await _contentsChannels.IsBattleChannel(ctx);
        if (isBattleChannel == false)
        {
            var message = await ctx.RespondAsync("전투가 불가능한 곳입니다.");
            Task.Run(async () =>
            {
                await Task.Delay(4000);
                await message.DeleteAsync();
            });
            return;
        }
        
        if (BattleSystem.IsFighting)
        {
            await ctx.RespondAsync(VEmoji.CrossSword + " ing... ");
            return;
        }

        if (!BattleSystem.IsA_Ready || !BattleSystem.IsB_Ready)
        {
            await ctx.RespondAsync(VEmoji.CrossSword + " not ready... ");
            return;
        }

        BattleSystem.IsFighting = true;

        int turnDelay = 700;
        if (!string.IsNullOrEmpty(battleCommand))
        {
            Int32.TryParse(battleCommand, out turnDelay);
        }

        turnDelay = Math.Clamp(turnDelay, 500, 2000);

        string turnText = "!  FIGHT  !";

        string userA_HpText = BattleSystem.GetHpText(BattleSystem.User_A.CurrentHp, BattleSystem.User_A.MaxHp);
        string userB_HpText = BattleSystem.GetHpText(BattleSystem.User_B.CurrentHp, BattleSystem.User_B.MaxHp);
        string userA_DamageText = ".";
        string userB_DamageText = ".";
        string userA_WinText = ".";
        string userB_WinText = ".";
        string userA_WinMoneyText = ".";
        string userB_WinMoneyText = ".";
        string userA_EquipText = BattleSystem.GetEquipText(BattleSystem.User_A.EquipValue);
        string userB_EquipText = BattleSystem.GetEquipText(BattleSystem.User_B.EquipValue);

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .AddField(new DiscordEmbedField("[ " + BattleSystem.User_A.Name + " ]　" + BattleSystem.GetHpBarString(BattleSystem.User_A.CurrentHp), userA_HpText, true))
            .AddField(new DiscordEmbedField("!　V S　!", turnText, true))
            .AddField(new DiscordEmbedField(BattleSystem.GetHpBarString(BattleSystem.User_B.CurrentHp) + "　[ " + BattleSystem.User_B.Name + " ]", userB_HpText, true))
            .AddField(new DiscordEmbedField(userA_DamageText, userA_WinMoneyText, true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(userB_DamageText, userB_WinMoneyText, true))
            .AddField(new DiscordEmbedField(userA_WinText, "──────────────", true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(userB_WinText, "──────────────", true))
            .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_A.Level, userA_EquipText, true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_B.Level, userB_EquipText, true));

        DiscordMessage battleBoard = await ctx.RespondAsync(embedBuilder);

        bool isUserAWin = false;
        bool isDraw = false;
        int turn = 0;
        while (0 < BattleSystem.User_A.CurrentHp && 0 < BattleSystem.User_B.CurrentHp && BattleSystem.IsFighting)
        {
            bool userA_isCritical = false;
            int userA_damage = 0;
            int userA_weaponDamage = 0;
            BattleSystem.User_A.GetAttackDamage(out userA_damage, out userA_weaponDamage, out userA_isCritical);
            bool userB_isCritical = false;
            int userB_damage = 0;
            int userB_weaponDamage = 0;
            BattleSystem.User_B.GetAttackDamage(out userB_damage, out userB_weaponDamage, out userB_isCritical);

            BattleSystem.User_A.CurrentHp = Math.Max(0, BattleSystem.User_A.CurrentHp - (userB_damage + userB_weaponDamage));
            BattleSystem.User_B.CurrentHp = Math.Max(0, BattleSystem.User_B.CurrentHp - (userA_damage + userA_weaponDamage));

            float userA_hpPercentage = 0 == BattleSystem.User_A.CurrentHp ? 0.0f : ((float)BattleSystem.User_A.CurrentHp / BattleSystem.User_A.MaxHp) * 100.0f;
            float userB_hpPercentage = 0 == BattleSystem.User_B.CurrentHp ? 0.0f : ((float)BattleSystem.User_B.CurrentHp / BattleSystem.User_B.MaxHp) * 100.0f;

            turnText = "⚔️[ " + Convert.ToString(++turn) + " ]⚔️";

            userA_HpText = BattleSystem.GetHpText(BattleSystem.User_A.CurrentHp, BattleSystem.User_A.MaxHp);
            userB_HpText = BattleSystem.GetHpText(BattleSystem.User_B.CurrentHp, BattleSystem.User_B.MaxHp);

            userA_DamageText = BattleSystem.GetDamageText(userA_damage, userA_weaponDamage, userA_isCritical);
            userB_DamageText = BattleSystem.GetDamageText(userB_damage, userB_weaponDamage, userB_isCritical);

            if (0 >= BattleSystem.User_A.CurrentHp || 0 >= BattleSystem.User_B.CurrentHp)
            {
                isUserAWin = BattleSystem.User_A.CurrentHp > BattleSystem.User_B.CurrentHp;
                isDraw = 0 == BattleSystem.User_A.CurrentHp && 0 == BattleSystem.User_B.CurrentHp;
                userA_WinText = isDraw ? " ..DRAW.." : isUserAWin ? VEmoji.Trophy + " ! WIN ! " + VEmoji.Trophy : VEmoji.Crossbones + " ..LOSE.. " + VEmoji.Crossbones;
                userB_WinText = isDraw ? " ..DRAW.." : !isUserAWin ? VEmoji.Trophy + " ! WIN ! " + VEmoji.Trophy : VEmoji.Crossbones + " ..LOSE.. " + VEmoji.Crossbones;
                if (0 != BattleSystem.FightMoney)
                {
                    userA_WinMoneyText = (isUserAWin ? "[ + " : "[ - ") + VEmoji.Money + Convert.ToString(BattleSystem.FightMoney) + " ]";
                    userB_WinMoneyText = (!isUserAWin ? "[ + " : "[ - ") + VEmoji.Money + Convert.ToString(BattleSystem.FightMoney) + " ]";   
                }
            }
            else
            {
                userA_WinText = ".";
                userB_WinText = ".";

                userA_WinMoneyText = ".";
                userB_WinMoneyText = ".";
            }

            DiscordEmbedBuilder embedBuilder1 = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Magenta)
                .AddField(new DiscordEmbedField("[ " + BattleSystem.User_A.Name + " ]　" + BattleSystem.GetHpBarString((int) userA_hpPercentage), userA_HpText, true))
                .AddField(new DiscordEmbedField("!　V S　!", turnText, true))
                .AddField(new DiscordEmbedField(BattleSystem.GetHpBarString((int) userB_hpPercentage, true) + "　[ " + BattleSystem.User_B.Name + " ]", userB_HpText, true))
                .AddField(new DiscordEmbedField(userA_DamageText, userA_WinMoneyText, true))
                .AddField(new DiscordEmbedField("|　 🇧 　|", "|　 🇦 　|", true))
                .AddField(new DiscordEmbedField(userB_DamageText, userB_WinMoneyText, true))
                .AddField(new DiscordEmbedField(userA_WinText, "──────────────", true))
                .AddField(new DiscordEmbedField("|　 🇹 　|", "|　 🇹 　|", true))
                .AddField(new DiscordEmbedField(userB_WinText, "──────────────", true))
                .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_A.Level, userA_EquipText, true))
                .AddField(new DiscordEmbedField("|　 🇱 　|", "|　 🇪 　|", true))
                .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_B.Level, userB_EquipText, true));

            Optional<DiscordEmbed> modifyEmbedBuilder = Optional.Some<DiscordEmbed>(embedBuilder1);

            await Task.Delay(turnDelay);
            await battleBoard.ModifyAsync(modifyEmbedBuilder);
        }

        await BattleSystem.CalculateFightMoney(isUserAWin, isDraw);
        BattleSystem.ResetSystem();
    }
}