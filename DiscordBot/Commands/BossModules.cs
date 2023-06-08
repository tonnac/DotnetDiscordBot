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

        int damage = rand.Next(1, 101);
        int hitCount = _bossMonster.HitCount;
        string deadBossEmojiCode = _bossMonster.BossEmojiCode;
        bool bIsKilled = _bossMonster.IsKilledByDamage(damage);

        if (!bIsKilled)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/D5tuK7HmI3YAAAAi/dark-souls-knight.gif")
                .WithColor(DiscordColor.HotPink)
                .WithAuthor("\u2694\uFE0F " + ctx.Member.Username)
                .AddField(new DiscordEmbedField("\uD83D\uDCA5 " + Convert.ToString(damage),
                    _bossMonster.BossEmojiCode + " " + Convert.ToString(_bossMonster.CurrentHp) + "/" + Convert.ToString(_bossMonster.CurrentMaxHp), false));
        
            await ctx.RespondAsync(embedBuilder);
        }
        else
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://media.tenor.com/mV5aSB_USt4AAAAi/coins.gif")
                .WithColor(DiscordColor.Gold)
                .WithAuthor("\uD83C\uDF8A " + ctx.Member.Username)
                .AddField(new DiscordEmbedField("\u2620\uFE0F " + deadBossEmojiCode + " \u2620\uFE0F", "\u2694\uFE0F " + Convert.ToString(hitCount+1), false));
        
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
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://i.pinimg.com/originals/22/ef/7e/22ef7eba94bc378be084e59e72eb7b25.jpg")
            .WithColor(DiscordColor.Orange)
            //.WithAuthor(_bossMonster.BossEmojiCode)
            .AddField(new DiscordEmbedField(_bossMonster.BossEmojiCode, "----", false))
            .AddField(new DiscordEmbedField("\u2665\uFE0F " + _bossMonster.CurrentHp + "/" + _bossMonster.CurrentMaxHp, "\u2694\uFE0F " + _bossMonster.HitCount, false));
        
        await ctx.RespondAsync(embedBuilder);
    }
}