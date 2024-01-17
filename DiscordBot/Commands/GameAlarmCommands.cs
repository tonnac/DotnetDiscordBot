using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Discord.Interactions;
using DiscordBot.Database;
using InteractionContext = DisCatSharp.ApplicationCommands.Context.InteractionContext;

namespace DiscordBot.Commands;

[Flags]
public enum GameFlag 
{
    [ChoiceName("Lol Aram")]
    LolAram = 0x01,
    [ChoiceName("Eternal Return")]
    Er = LolAram << 1,
    [ChoiceName("ORD")]
    Ord = Er << 1,
    [ChoiceName("Stardew Valley")]
    StardewValley = Ord << 1,
    [ChoiceName("Lethal Company")]
    LethalCompany = StardewValley << 1,
}

[Flags]
public enum SubscribeGameFlag 
{
    [ChoiceName("Lol Aram")]
    LolAram = GameFlag.LolAram,
    [ChoiceName("Eternal Return")]
    Er = GameFlag.Er,
    [ChoiceName("ORD")]
    Ord = GameFlag.Ord,
    [ChoiceName("Stardew Valley")]
    StardewValley = GameFlag.StardewValley,
    [ChoiceName("Lethal Company")]
    LethalCompany = GameFlag.LethalCompany,
    [ChoiceName("All")]
    All = LolAram | Er | Ord | StardewValley | LethalCompany
}

[Flags]
public enum ActionFlag 
{
    [ChoiceName("Register")]
    Register = 0x01,
    [ChoiceName("Unregister")]
    Unregister = Register << 1,
}

public class GameAlarmCommands : ApplicationCommandsModule
{
    [DisCatSharp.ApplicationCommands.Attributes.SlashCommand("GameAlarm", "Send an alarm to registered users.")]
    public static async Task GameAlarm(InteractionContext ctx, [Option("Game", "Select a Game")] GameFlag gameFlag)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder());
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var users = await database.GetSubscribedUser(ctx, gameFlag);

        var calledMember = users.Find((user => user.userid == ctx.Member.Id));
        var embedBuilder = new DiscordEmbedBuilder();
        if (null == calledMember)
        {
            embedBuilder.WithDescription("Unsubscribed games can't send alarms.");
        }
        else
        {
            users.RemoveAll(user => user.userid == ctx.User.Id);
            
            foreach (var databaseUser in users)
            {
                if (ctx.Guild.Members.TryGetValue(databaseUser.userid, out DiscordMember? member))
                {
                    await member.SendMessageAsync(GetGameAlarmEmbed( ctx.Member, member, gameFlag));
                }
            }
            
            if (users.Count > 0)
            {
                embedBuilder.WithDescription("The message has been sent.");
            }
            else
            {
                embedBuilder.WithDescription("The subscribed member does not exist.");
            }
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
    }
    
    [DisCatSharp.ApplicationCommands.Attributes.SlashCommand("SubscribeGame", "Register as a user in the game you want to be alarm about.")]
    public static async Task SubscribeGame(InteractionContext ctx, [Option("Game", "Select a Game")] SubscribeGameFlag gameParameter, [Option("Action", "Select a Action")] ActionFlag actionParameter)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder());
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var user = await database.GetDatabaseUser(ctx.Guild, ctx.User);
        var embedBuilder = new DiscordEmbedBuilder();

        if (actionParameter == ActionFlag.Register)
        {
            if (user.userid != 0 && ((SubscribeGameFlag)user.gameflag).HasFlag(gameParameter))
            {
                embedBuilder.WithDescription("Already Subscribed");
            }
            else
            {
                bool bSuccess = await database.SubscribeGame(ctx, gameParameter, true);

                if (bSuccess)
                {
                    embedBuilder.WithDescription("Success!");
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            if (gameParameter == SubscribeGameFlag.All && user.gameflag != 0 || ((SubscribeGameFlag)user.gameflag).HasFlag(gameParameter))
            {
                bool bSuccess = await database.SubscribeGame(ctx, gameParameter, false);
                if (bSuccess)
                {
                    embedBuilder.WithDescription("Success!");
                }
                else
                {
                    return;
                }
            }
            else 
            {
                embedBuilder.WithDescription("Already Unsubscribed");
            }
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
    }

    private static DiscordEmbedBuilder GetGameAlarmEmbed(DiscordMember callingMember, DiscordMember calledMember, GameFlag gameFlag)
    {
        var embedBuilder = new DiscordEmbedBuilder();
        switch (gameFlag)
        {
            case GameFlag.LolAram:
                SetLolAlarmEmbed(callingMember, calledMember, embedBuilder);
                break;
            case GameFlag.Er:
                SetEternalReturnAlarmEmbed(callingMember, calledMember, embedBuilder);
                break;
            case GameFlag.Ord:
                SetORDAlarmEmbed(callingMember, calledMember, embedBuilder);
                break;
            case GameFlag.StardewValley:
                SetStardewValleyAlarmEmbed(callingMember, calledMember, embedBuilder);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameFlag), gameFlag, null);
        }
        
        return embedBuilder;
    }

    private static void SetLolAlarmEmbed(DiscordMember callingMember, DiscordMember calledMember,
        in DiscordEmbedBuilder embedBuilder)
    {
        string name = Utility.GetMemberDisplayName(callingMember);
        embedBuilder
            .WithColor(DiscordColor.Azure)
            .WithDescription($"{name}님의 칼바람나락 호출이 왔습니다!")
            .WithImageUrl(
                "https://static.wikia.nocookie.net/leagueoflegends/images/5/5f/Howling_Abyss_Map_Preview.jpg/revision/latest?cb=20140612032106");
    }
    
    static readonly string[] EternalReturnImageUrls = 
        { 
            "https://static.inven.co.kr/image_2011/site_image/er/dataninfo/skinimage/skinimage_adina1.jpg?v=230918a", 
            "https://static.inven.co.kr/image_2011/site_image/er/dataninfo/skinimage/skinimage_adina2.jpg?v=230918a", 
            "https://static.inven.co.kr/image_2011/site_image/er/dataninfo/skinimage/skinimage_adina3.jpg?v=230918a", 
        };
    private static void SetEternalReturnAlarmEmbed(DiscordMember callingMember, DiscordMember calledMember,
        in DiscordEmbedBuilder embedBuilder)
    {

        string name = Utility.GetMemberDisplayName(callingMember);
        var rand = new Random();
        string url = EternalReturnImageUrls[rand.Next(0, EternalReturnImageUrls.Length - 1)];
        embedBuilder
            .WithColor(DiscordColor.Azure)
            .WithDescription($"{name}님의 루미아 섬 호출이 왔습니다!")
            .WithImageUrl(url);
    }
    
    // ReSharper disable once InconsistentNaming
    private static void SetORDAlarmEmbed(DiscordMember callingMember, DiscordMember calledMember,
        in DiscordEmbedBuilder embedBuilder)
    {
        string name = Utility.GetMemberDisplayName(callingMember);
        embedBuilder
            .WithColor(DiscordColor.Azure)
            .WithDescription($"{name}님의 항해 호출이 왔습니다!")
            .WithImageUrl(
                "https://e1.pxfuel.com/desktop-wallpaper/916/442/desktop-wallpaper-straw-hat-pirates-one-piece-one-piece-crew.jpg");
    }
    
    // ReSharper disable once InconsistentNaming
    private static void SetStardewValleyAlarmEmbed(DiscordMember callingMember, DiscordMember calledMember,
        in DiscordEmbedBuilder embedBuilder)
    {
        string name = Utility.GetMemberDisplayName(callingMember);
        embedBuilder
            .WithColor(DiscordColor.Azure)
            .WithDescription($"{name}님의 스타듀밸리 호출이 왔습니다!")
            .WithImageUrl(
                "https://nas.battlepage.com/upload/2019/0616/161422100dcf2aaa5da5c42626650f7ff19e651e.png");
    }
}