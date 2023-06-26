using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DiscordBot.Boss;
using DiscordBot.Channels;
using DiscordBot.Core;
using DiscordBot.Database;
using DiscordBot.Equip;
using DiscordBot.Resource;

namespace DiscordBot.Commands;

[Hidden]
public class CheatModules : BaseCommandModule
{
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
    
    [Command]
    public async Task SetGoldGemMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
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

            EquipCalculator.SetGold_GemUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }
    }
    
    [Command]
    public async Task SetGemPayMultiplier(CommandContext ctx, [RemainingText] string? setCommand)
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

            EquipCalculator.SetPay_GemUpgradeMultiplier(value);
            result = true;
            emoji = VEmoji.GreenCheckBox;
        }
        
        if (result)
        {
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
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

    [Command] // ToggleBossChannel
    public async Task Bbbb(CommandContext ctx)
    {
        string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.BossGame);
        string emoji = VEmoji.RedCrossMark;
        if ("+" == result)
        {
            emoji = VEmoji.GreenCheckBox;
        }
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
    }
    
    [Command] // ToggleForgeChannel
    public async Task Uuuu(CommandContext ctx)
    {
        string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.Forge);
        string emoji = VEmoji.RedCrossMark;
        if ("+" == result)
        {
            emoji = VEmoji.GreenCheckBox;
        }
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
    }
    
    [Command] // ToggleFishingChannel
    public async Task Ffff(CommandContext ctx)
    {
        string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.Fishing);
        string emoji = VEmoji.RedCrossMark;
        if ("+" == result)
        {
            emoji = VEmoji.GreenCheckBox;
        }
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
    }
    
    [Command] // ToggleGambleChannel
    public async Task Gggg(CommandContext ctx)
    {
        string result = await ContentsChannels.ToggleChannelContent(ctx.Channel, ContentsFlag.Gamble);
        string emoji = VEmoji.RedCrossMark;
        if ("+" == result)
        {
            emoji = VEmoji.GreenCheckBox;
        }
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
    }
}