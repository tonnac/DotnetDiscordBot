using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Boss;

namespace DiscordBot.Commands;

public class BossModules : BaseCommandModule
{
    private readonly BossMonster _bossMonster;

    public BossModules()
    {
        var rand = new Random();
        int bossType = rand.Next((int) BossType.Bat, (int) BossType.End);
        _bossMonster = new BossMonster((BossType) bossType);
    }
    [Command, Aliases("ba"), Cooldown(1, 300, CooldownBucketType.User)]
    public async Task BossAttack(CommandContext ctx)
    {
        var rand = new Random();

        int AttackChance = rand.Next(1, 101);
        string DamageTypeEmojiCode = "\uD83D\uDCA5 ";
        string CritAddText = "";
        int lastDamage = rand.Next(1, 101);
        if (10 >= AttackChance)
        {
            lastDamage = 0;
            DamageTypeEmojiCode = "\ud83d\ude35\u200d\ud83d\udcab ";
        }
        else if (85 <= AttackChance)
        {
            lastDamage = lastDamage * 2 + 100;
            CritAddText = " !";
            DamageTypeEmojiCode = "\uD83D\uDD25 ";
        }
        
        int hitCount = _bossMonster.HitCount;
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        KeyValuePair<string, int> BestDealerInfo;// = _bossMonster.GetBestDealer();
        int lastCurrentHp = _bossMonster.CurrentHp;
        bool bIsKilled = _bossMonster.IsKilledByDamage(ctx.Member.Username, lastDamage, out BestDealerInfo);

        if (!bIsKilled)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif")
                .WithColor(DiscordColor.HotPink)
                .WithAuthor("\u2694\uFE0F " + ctx.Member.Username)
                .AddField(new DiscordEmbedField(DamageTypeEmojiCode + Convert.ToString(lastDamage) + CritAddText,
                    _bossMonster.BossEmojiCode + " " + Convert.ToString(_bossMonster.CurrentHp) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        
            await ctx.RespondAsync(embedBuilder);
        }
        else
        {
            string BestDealer = "";
            int BestDealerTotalDamage = 0;
            
            if (string.IsNullOrEmpty(BestDealerInfo.Key))
            {
                BestDealer = ctx.Member.Username;
                BestDealerTotalDamage = _bossMonster.CurrentMaxHp;
            }
            else
            {
                BestDealer = BestDealerInfo.Key;
                BestDealerTotalDamage = BestDealerInfo.Value;
                
                if (ctx.Member.Username == BestDealerInfo.Key)
                {
                    BestDealerTotalDamage += lastCurrentHp;
                }
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("\uD83C\uDF8A " + ctx.Member.Username)
                .AddField(new DiscordEmbedField(deadBossEmojiCode + " \u2694\uFE0F " + Convert.ToString(hitCount + 1), 
                    "\uD83E\uDD47 " + BestDealer + "   " + "\uD83D\uDCA5 " + Convert.ToString(BestDealerTotalDamage),
                    false));
        
            var message = await ctx.RespondAsync(embedBuilder);

            if( message != null )
            {
                await message.PinAsync();
            }
        }
    }
    [Command, Aliases("bi")]
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
            .WithThumbnail("https://i.pinimg.com/originals/22/ef/7e/22ef7eba94bc378be084e59e72eb7b25.jpg")
            .WithColor(DiscordColor.Orange)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField(_bossMonster.BossEmojiCode, "\u2665\uFE0F " + _bossMonster.CurrentHp + "/" + _bossMonster.CurrentMaxHp, false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47 " + BestDealer + "   " + "\uD83D\uDCA5 " + Convert.ToString(BestDealerTotalDamage),
                "\u2694\uFE0F " + _bossMonster.HitCount, false));
        
        await ctx.RespondAsync(embedBuilder);
    }
}