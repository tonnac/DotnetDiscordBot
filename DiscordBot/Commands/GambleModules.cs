using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Boss;
using DiscordBot.Database;

/*
    [500]
    -%	0
    5%	1000	0.14
    3%	2000	0.12
    1%	7777	0.15
 */
namespace DiscordBot.Commands;

public class GambleModules : BaseCommandModule
{
    private readonly GambleGame _gambleGame_SlotMachine;
    // private readonly GambleGame _gambleGame_Gacha;
    // private readonly GambleGame _gambleGame_Roulette;
    
    public GambleModules()
    {
        _gambleGame_SlotMachine = new GambleGame();
        _gambleGame_SlotMachine.GameAnte = 500;
        _gambleGame_SlotMachine.SetPercentage(1, 3, 5);
        _gambleGame_SlotMachine.SetReward(7777, 2000, 1000);
        
        // _gambleGame_Gacha = new GambleGame();
        // _gambleGame_Gacha.GameAnte = 1000;
        // _gambleGame_Gacha.SetPercentage(1, 3, 7);
        // _gambleGame_Gacha.SetReward(2000, 600, 200);
        //
        // _gambleGame_Roulette = new GambleGame();
        // _gambleGame_Roulette.GameAnte = 5000;
        // _gambleGame_Roulette.SetPercentage(1, 3, 7);
        // _gambleGame_Roulette.SetReward(00, 1500, 400);
    }

    [Command, Aliases("ggl"), Cooldown(1, 10, CooldownBucketType.User)]
    public async Task GambleGameList(CommandContext ctx, [RemainingText] string? gambleCommand)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithThumbnail("https://img.freepik.com/premium-photo/classic-casino-roulette_103577-4040.jpg")
            .WithColor(DiscordColor.White)
            // .AddField(new DiscordEmbedField("──────────", "[  \uD83D\uDDF3\uFE0F  ]" + " \uD83D\uDCB0" + Convert.ToString(_gambleGame_Gacha.GameAnte) + " ─── (g)", false))
            // .AddField(new DiscordEmbedField("\uD83E\uDD47 " + Convert.ToString(_gambleGame_Gacha.Percentage_GoldPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Gacha.Reward_GoldPrize), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD48 " + Convert.ToString(_gambleGame_Gacha.Percentage_SilverPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Gacha.Reward_SilverPrize), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD49 " + Convert.ToString(_gambleGame_Gacha.Percentage_BronzePrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Gacha.Reward_BronzePrize), true))
            // .AddField(new DiscordEmbedField("──────────", "[  \uD83E\uDDFF  ]" + " \uD83D\uDCB0" + Convert.ToString(_gambleGame_Roulette.GameAnte) + " ─── (r)", false))
            // .AddField(new DiscordEmbedField("\uD83E\uDD47 " + Convert.ToString(_gambleGame_Roulette.Percentage_GoldPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Roulette.Reward_GoldPrize), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD48 " + Convert.ToString(_gambleGame_Roulette.Percentage_SilverPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Roulette.Reward_SilverPrize), true))
            // .AddField(new DiscordEmbedField("\uD83E\uDD49 " + Convert.ToString(_gambleGame_Roulette.Percentage_BronzePrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_Roulette.Reward_BronzePrize), true))
            .AddField(new DiscordEmbedField("──────────", "[  \uD83C\uDFB0  ]" + " \uD83D\uDCB0" + Convert.ToString(_gambleGame_SlotMachine.GameAnte) + " ─── (s)", false))
            .AddField(new DiscordEmbedField("\uD83E\uDD47 " + Convert.ToString(_gambleGame_SlotMachine.Percentage_GoldPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_SlotMachine.Reward_GoldPrize), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD48 " + Convert.ToString(_gambleGame_SlotMachine.Percentage_SilverPrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_SlotMachine.Reward_SilverPrize), true))
            .AddField(new DiscordEmbedField("\uD83E\uDD49 " + Convert.ToString(_gambleGame_SlotMachine.Percentage_BronzePrize) + "%", "\uD83D\uDCB0" + Convert.ToString(_gambleGame_SlotMachine.Reward_BronzePrize), true));
        
        await ctx.RespondAsync(embedBuilder);
    }

    [Command, Aliases("dgg"), Cooldown(1, 5, CooldownBucketType.User)]
    public async Task DoGambleGame(CommandContext ctx, [RemainingText] string? gambleCommand)
    {
        string gambleEmoji = "\uD83D\uDDF3\uFE0F";
        int gameAnte = 0;
        GambleGame gamble = new GambleGame();
        string name = Utility.GetMemberDisplayName(ctx.Member);
        
        if (string.IsNullOrEmpty(gambleCommand) || "s" == gambleCommand)
        {
            gamble = _gambleGame_SlotMachine;
            gameAnte = _gambleGame_SlotMachine.GameAnte;
            gambleEmoji = "\uD83D\uDDF3\uFE0F";
        }
        // else if ("r" == gambleCommand)
        // {
        //     gamble = _gambleGame_Roulette;
        //     gameAnte = _gambleGame_Roulette.GameAnte;
        //     gambleEmoji = "\uD83E\uDDFF";
        // }
        // else if ("s" == gambleCommand)
        // {
        //     gamble = _gambleGame_SlotMachine;
        //     gameAnte = _gambleGame_SlotMachine.GameAnte;
        //     gambleEmoji = "\uD83C\uDFB0";
        // }
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser gambleUserDatabase= await database.GetDatabaseUser(ctx.Guild, ctx.User);

        if (gambleUserDatabase.gold >= gameAnte)
        {
            int gambleReward = gamble.DoGamble();

            string resultEmoji = "\uD83D\uDE2D";
            if (gambleReward == gamble.Reward_GoldPrize)
            {
                resultEmoji = "\uD83E\uDD47";
            }
            else if (gambleReward == gamble.Reward_SilverPrize)
            {
                resultEmoji = "\uD83E\uDD48";
            }
            else if (gambleReward == gamble.Reward_BronzePrize)
            {
                resultEmoji = "\uD83E\uDD49";
            }
                
            GoldQuery query = new GoldQuery(gambleReward - gameAnte);
            await database.UpdateUserGold(ctx, query);
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail("https://i.gifer.com/E3xX.gif")
                .WithColor(DiscordColor.Gold)
                .AddField(new DiscordEmbedField(gambleEmoji + " " + name, "[ - \uD83D\uDCB0" + Convert.ToString(gameAnte) + " ]", false))
                .AddField(new DiscordEmbedField(resultEmoji + " ", "[ + \uD83D\uDCB0" + Convert.ToString(gambleReward) + " ]", false));
        
            await ctx.RespondAsync(embedBuilder);
        }
        else
        {
            await ctx.RespondAsync("\uD83D\uDCB0.. \u2753");
        }
    }
}