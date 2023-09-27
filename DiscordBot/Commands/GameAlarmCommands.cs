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
    [ChoiceName("All")]
    All = LolAram | Er | Ord
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
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var users = await database.GetSubscribedUser(ctx, gameFlag);
        
        // users.RemoveAll(user => user.userid == ctx.User.Id);
            
        foreach (var databaseUser in users)
        {
            if (ctx.Guild.Members.TryGetValue(databaseUser.userid, out DiscordMember? member))
            {
                await member.SendMessageAsync(GetGameAlarmEmbed( ctx.Member, member, gameFlag));
            }
        }
        
        if (users.Count > 0)
        {
            // await ctx.RespondAsync("메세지를 전송했습니다.");
        }
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = "123123"
            });
    }
    
    [DisCatSharp.ApplicationCommands.Attributes.SlashCommand("SubscribeGame", "Register as a user in the game you want to be alarm about.")]
    public static async Task SubscribeGame(InteractionContext ctx, [Option("Game", "Select a Game")] SubscribeGameFlag gameParameter, [Option("Action", "Select a Action")] ActionFlag actionParameter)
    {
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

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
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
}