using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Database;
using DiscordBot.Equip;
using DiscordBot.Resource;

namespace DiscordBot.Commands;

public class UserGameInfoModules : BaseCommandModule
{   
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

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://cdn-icons-png.flaticon.com/512/943/943579.png")
            .WithColor(DiscordColor.Black)
            .AddField(new DiscordEmbedField(VEmoji.Magnifier + " " + name, "───────────────", false))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Money + "  ]", Convert.ToString(myUserDatabase.gold), false))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Gem + "  ]", "+" + Convert.ToString(gemUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Ring + "  ]", "+" + Convert.ToString(ringUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Weapon + "  ]", "+" + Convert.ToString(weaponUpgrade), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Crossbones + "  ]", Convert.ToString(myUserDatabase.bosskillcount), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.CrossSword + "  ]", Convert.ToString(myUserDatabase.combatcount), true))
            .AddField(new DiscordEmbedField("[  " + VEmoji.Boom + "  ]", Convert.ToString(myUserDatabase.bosstotaldamage), true));
        
        await ctx.RespondAsync(embedBuilder);
    }
}