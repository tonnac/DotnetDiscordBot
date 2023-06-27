using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Boss;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<bool> UpdateBossRaid(CommandContext ctx, BossQuery query)
    {
        return await ExecuteNonQueryASync($"update USER set bosskillcount = bosskillcount+{query.KillCount}, bosstotaldamage = bosstotaldamage+{query.Damage}, gold = gold+{query.Gold}, combatcount = combatcount+{query.CombatCount} where id='{GetSHA256(ctx.Guild, ctx.User)}'");
    }

    public async Task<bool> UpdateUserGold(CommandContext ctx, GoldQuery query)
    {
        return await ExecuteNonQueryASync(
            $"update USER set gold = gold+{query.Gold} where id='{GetSHA256(ctx.Guild, ctx.User)}'");
    }
    
    public async Task<bool> UpdateUserGold(DiscordGuild guild, DiscordUser user, GoldQuery query)
    {
        return await ExecuteNonQueryASync(
            $"update USER set gold = gold+{query.Gold} where id='{GetSHA256(guild, user)}'");
    }

    public async Task<bool> ResetBossKillCount(CommandContext ctx)
    {
        return await ExecuteNonQueryASync($"update USER set bosskillcount = 0 where guildid='{ctx.Guild.Id}'");
    }

    public async Task<bool> ResetBossTotalDamage(CommandContext ctx)
    {
        return await ExecuteNonQueryASync($"update USER set bosstotaldamage = 0 where guildid='{ctx.Guild.Id}'");
    }

    public async Task<bool> ResetGold(CommandContext ctx)
    {
        return await ExecuteNonQueryASync($"update USER set gold = 0 where guildid='{ctx.Guild.Id}'");
    }

    public async Task<bool> ResetCombatCount(CommandContext ctx)
    {
        return await ExecuteNonQueryASync($"update USER set combatcount = 0 where guildid='{ctx.Guild.Id}'");
    }
    
    public async Task<bool> ResetEquipValue(CommandContext ctx)
    {
        return await ExecuteNonQueryASync($"update USER set equipvalue = 0 where guildid='{ctx.Guild.Id}'");
    }
    
    public async Task<bool> AddEquipValue(CommandContext ctx, int addEquipValue)
    {
        return await ExecuteNonQueryASync(
            $"update USER set equipvalue = equipvalue+{addEquipValue} where id='{GetSHA256(ctx.Guild, ctx.User)}'");
    }

    public async Task<bool> UpdateYachtWin(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync(
            $"update USER set yachtwin += 1 where id='{GetSHA256(guild, user)}'");
    }

    public async Task<bool> UpdateYachtLose(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync(
            $"update USER set yachtlose += 1 where id='{GetSHA256(guild, user)}'");
    }

    public async Task<bool> UpdateYachtDraw(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync(
            $"update USER set yachtDraw += 1 where id='{GetSHA256(guild, user)}'");
    }
}