using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Channels;
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
    public async Task A3_BattleReset(CommandContext ctx)
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
        
        BattleSystem.IsFighting = false;
        BattleSystem.IsA_Ready = false;
        BattleSystem.IsB_Ready = false;
        
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(VEmoji.GreenCheckBox));
    }

    [Command, Aliases("bj", "전투참여")]
    public async Task A1_BattleJoin(CommandContext ctx)
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
        
        if (!BattleSystem.IsA_Ready)
        {
            BattleSystem.User_A.SetUserBattleInfo(ctx.Guild, ctx.User, ctx.Member);
            BattleSystem.IsA_Ready = true;
            await ctx.RespondAsync(VEmoji.A + "️ Ready !");
        }
        else if (!BattleSystem.IsB_Ready)
        {
            BattleSystem.User_B.SetUserBattleInfo(ctx.Guild, ctx.User, ctx.Member);
            BattleSystem.IsB_Ready = true;
            await ctx.RespondAsync(VEmoji.B + "️ Ready !");
        }
    }

    [Command, Aliases("bs", "전투시작")]
    public async Task A2_BattleStart(CommandContext ctx)
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
        
        string turnText = "!  FIGHT  !";
        
        string userA_HpText = BattleSystem.GetHpText(BattleSystem.User_A.CurrentHp, BattleSystem.User_A.MaxHp);
        string userB_HpText = BattleSystem.GetHpText(BattleSystem.User_B.CurrentHp, BattleSystem.User_B.MaxHp);
        string userA_DamageText = ".";
        string userB_DamageText = ".";
        string userA_WinText = ".";
        string userB_WinText = ".";
        string userA_EquipText = BattleSystem.GetEquipText(BattleSystem.User_A.EquipValue);
        string userB_EquipText = BattleSystem.GetEquipText(BattleSystem.User_B.EquipValue);

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .AddField(new DiscordEmbedField("[ " + BattleSystem.User_A.Name + " ]　" + BattleSystem.GetHpBarString(BattleSystem.User_A.CurrentHp), userA_HpText, true))
            .AddField(new DiscordEmbedField("!　V S　!", turnText, true))
            .AddField(new DiscordEmbedField(BattleSystem.GetHpBarString(BattleSystem.User_B.CurrentHp) + "　[ " + BattleSystem.User_B.Name + " ]", userB_HpText, true))
            .AddField(new DiscordEmbedField(userA_DamageText, ".", true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(userB_DamageText, ".", true))
            .AddField(new DiscordEmbedField(userA_WinText, "──────────────", true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(userB_WinText, "──────────────", true))
            .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_A.Level, userA_EquipText, true))
            .AddField(new DiscordEmbedField("|　　　 |", "|　　　 |", true))
            .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_B.Level, userB_EquipText, true));
        
        DiscordMessage battleBoard = await ctx.RespondAsync(embedBuilder);

        int turn = 0;
        while ( 0 < BattleSystem.User_A.CurrentHp && 0 < BattleSystem.User_B.CurrentHp && BattleSystem.IsFighting)
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
                bool isDraw = 0 == BattleSystem.User_A.CurrentHp && 0 == BattleSystem.User_B.CurrentHp;
                userA_WinText = isDraw ? " ..DRAW.." : BattleSystem.User_A.CurrentHp > BattleSystem.User_B.CurrentHp ? VEmoji.Trophy + " ! WIN ! " + VEmoji.Trophy : VEmoji.Crossbones + " ..LOSE.. " + VEmoji.Crossbones;
                userB_WinText = isDraw ? " ..DRAW.." : BattleSystem.User_A.CurrentHp < BattleSystem.User_B.CurrentHp ? VEmoji.Trophy + " ! WIN ! " + VEmoji.Trophy : VEmoji.Crossbones + " ..LOSE.. " + VEmoji.Crossbones;   
            }
            else
            {
                userA_WinText = ".";
                userB_WinText = ".";
            }
            
            DiscordEmbedBuilder embedBuilder1 = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Magenta)
                .AddField(new DiscordEmbedField("[ " + BattleSystem.User_A.Name + " ]　" + BattleSystem.GetHpBarString((int)userA_hpPercentage), userA_HpText, true))
                .AddField(new DiscordEmbedField("!　V S　!", turnText, true))
                .AddField(new DiscordEmbedField(BattleSystem.GetHpBarString((int)userB_hpPercentage, true) + "　[ " + BattleSystem.User_B.Name + " ]", userB_HpText, true))
                .AddField(new DiscordEmbedField(userA_DamageText, ".", true))
                .AddField(new DiscordEmbedField("|　 🇧 　|", "|　 🇦 　|", true))
                .AddField(new DiscordEmbedField(userB_DamageText, ".", true))
                .AddField(new DiscordEmbedField(userA_WinText, "──────────────", true))
                .AddField(new DiscordEmbedField("|　 🇹 　|", "|　 🇹 　|", true))
                .AddField(new DiscordEmbedField(userB_WinText, "──────────────", true))
                .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_A.Level, userA_EquipText, true))
                .AddField(new DiscordEmbedField("|　 🇱 　|", "|　 🇪 　|", true))
                .AddField(new DiscordEmbedField(VEmoji.Level + " Lv." + BattleSystem.User_B.Level, userB_EquipText, true));
            
            Optional<DiscordEmbed> modifyEmbedBuilder = Optional.Some<DiscordEmbed>(embedBuilder1);
            
            await Task.Delay(1000);
            await battleBoard.ModifyAsync(modifyEmbedBuilder);
        }
        
        BattleSystem.IsFighting = false;
        BattleSystem.IsA_Ready = false;
        BattleSystem.IsB_Ready = false;
    }
}