using System.Text.RegularExpressions;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DiscordBot.Database;

namespace DiscordBot.Commands;

public class RouletteModules : BaseCommandModule
{
    [Command, Aliases("룰렛등록")]
    public async Task RouletteRegister(CommandContext ctx, [RemainingText] string? members)
    {
        if (members == null || ctx.Member.Id != 194280090589200385)
        {
            return;
        }
        
        Regex regex = new Regex("[a-zA-Z가-힣0-9()]*[^;',/| .]");

        var mc = regex.Matches(members);

        if (mc.Count == 0)
        {
            throw new Exception("can't find unit value");
        }

        List<string> membersList = new List<string>();
        string winner = String.Empty;
        
        foreach (Match match in mc)
        {
            string name = match.Value;
            if (match.Value.Contains("(") || match.Value.Contains(")"))
            {
                 name = match.Value.Replace("(", string.Empty);
                 name = name.Replace(")", string.Empty);
                 winner = name;
            }
            membersList.Add(name);
        }

        if (winner == String.Empty)
        {
            throw new Exception();
        }

        Roulette roulette = new Roulette(Utility.GetCurrentTime(), winner, membersList);
        
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        await database.RegisterRoulette(roulette, ctx.Guild);
        
        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
    }
    
    [Command, Aliases("룰렛정보")]
    public async Task RouletteInfo(CommandContext ctx, [RemainingText] string? name)
    {
        if (name == null)
        {
            return;
        }

        using var database = new DiscordBotDatabase();
        await database.ConnectASync();

        var rouletteList = await database.GetRoulette(ctx.Guild, name);

        int numberOfMan = 0;
        int losses = 0;
        int takingPartCount = rouletteList.Count;

        foreach (var roulette in rouletteList)
        {
            if (roulette.Winner == name)
            {
                losses++;
                numberOfMan += roulette.Members.Count - 1;
            }
        }

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .AddField(new DiscordEmbedField("참가한 게임", takingPartCount.ToString(), true))
            .AddField(new DiscordEmbedField("은혜를 입은 사람", numberOfMan.ToString(), true))
            .AddField(new DiscordEmbedField("염치없게 얻어먹은 게임", (takingPartCount - losses).ToString(), true));

        await ctx.RespondAsync(embedBuilder);
    }

    [Command, Aliases("최근룰렛")]
    public async Task RecentRoulette(CommandContext ctx, [RemainingText] string countString)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();

        int count = int.Parse(countString);

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

        var recentRouletteList = await database.GetRecentRoulette(ctx.Guild, count);
        foreach (var roulette in recentRouletteList)
        {
            messageBuilder.AddEmbed(MakeRouletteEmbedBuilder(roulette));
        }

        await ctx.RespondAsync(messageBuilder);
    }
    
    [Command, Aliases("룰렛랭킹")]
    public async Task RouletteRanking(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var rouletteRanking = await database.GetRouletteRanking(ctx.Guild);

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

        var discordEmbedBuilder = new DiscordEmbedBuilder();
        var embedFields = new List<DiscordEmbedField>();
        var prevRate = new { index = int.MaxValue, rate = float.MaxValue };
        for (var i = 0; i < rouletteRanking.WinRates.Count; i++)
        {
            var elem = rouletteRanking.WinRates[i];
            if (Math.Abs(prevRate.rate - elem.winrate) > 0.0001f)
            {
                prevRate = new { index = i, rate = elem.winrate }; 
            }
            embedFields.Add(new DiscordEmbedField($"{ prevRate.index + 1 }. { elem.name }", $"{elem.winrate:0.0}% ({elem.wins}/{elem.takingpartcount})", true));
        }
        discordEmbedBuilder
            .WithAuthor("좀 걸려라")
            .WithColor(DiscordColor.Wheat)
            .AddFields(embedFields);
        messageBuilder.AddEmbed(discordEmbedBuilder);
        
        discordEmbedBuilder = new DiscordEmbedBuilder();
        embedFields = new List<DiscordEmbedField>();
        prevRate = new { index = int.MaxValue, rate = float.MaxValue };
        for (var i = 0; i < rouletteRanking.TakingParts.Count; i++)
        {
            var elem = rouletteRanking.TakingParts[i];
            if (Math.Abs(prevRate.rate - elem.playedgamerate) > 0.0001f)
            {
                prevRate = new { index = i, rate = elem.playedgamerate }; 
            }
            embedFields.Add(new DiscordEmbedField($"{ prevRate.index + 1 }. { elem.name }", $"{elem.playedgamerate:0.0}% ({elem.takingpartcount}/{elem.totalgame})", true));
        }
        discordEmbedBuilder
            .WithAuthor("참여왕")
            .WithColor(DiscordColor.Wheat)
            .AddFields(embedFields);
        messageBuilder.AddEmbed(discordEmbedBuilder);
        
        discordEmbedBuilder = new DiscordEmbedBuilder();
        embedFields = new List<DiscordEmbedField>();
        var prevSpentCount = new { index = int.MaxValue, spentCount = int.MaxValue };
        for (var i = 0; i < rouletteRanking.SpentCounts.Count; i++)
        {
            var elem = rouletteRanking.SpentCounts[i];
            if (Math.Abs(prevSpentCount.spentCount - elem.spentcount) > 0.0001f)
            {
                prevSpentCount = new { index = i, spentCount = elem.spentcount }; 
            }
            embedFields.Add(new DiscordEmbedField($"{ prevSpentCount.index + 1 }. { elem.name }", $"{elem.spentcount} ({elem.wins})", true));
        }
        discordEmbedBuilder
            .WithAuthor("기부왕")
            .WithColor(DiscordColor.Wheat)
            .AddFields(embedFields);
        messageBuilder.AddEmbed(discordEmbedBuilder);
        await ctx.RespondAsync(messageBuilder);
    }

    private DiscordEmbedBuilder MakeRouletteEmbedBuilder(Roulette roulette)
    {
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

        const int partitionNumber = 4;
        const string timeFormat = "yyyy년 MM월 dd일 HH:mm";

        string MakeMemberString()
        {
            var partitionMembersList = roulette.Members.Partition(partitionNumber);

            List<string> memberRows = new List<string>();

            foreach (var partitionMembers in partitionMembersList)
            {
                memberRows.Add(string.Join(", ", partitionMembers));
            }

            return string.Join("\n", memberRows);
        }

        embedBuilder
            .WithTitle(roulette.Time.ToString(timeFormat))
            .WithColor(DiscordColor.DarkGreen)
            .WithDescription(MakeMemberString())
            .AddField(new DiscordEmbedField("승리", roulette.Winner));

        if (roulette.MessageLink != null)
        {
            embedBuilder.AddField(new DiscordEmbedField("영상", roulette.MessageLink));
        }

        return embedBuilder;
    }
}