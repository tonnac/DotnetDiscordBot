using DiscordBot.Database.Tables;
namespace DiscordBot;

public class RouletteRanking
{
    public RouletteRanking(List<RouletteWinRate> winRates, List<RouletteSpentCount> spentCounts, List<RouletteTakingPart> takingParts)
    {
        WinRates = winRates;
        SpentCounts = spentCounts;
        TakingParts = takingParts;
    }

    public RouletteRanking()
    {
    }
    
    public List<RouletteWinRate> WinRates { get; }
    public List<RouletteSpentCount> SpentCounts { get; }
    public List<RouletteTakingPart> TakingParts { get; }
}