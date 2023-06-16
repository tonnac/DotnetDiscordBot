using DisCatSharp.CommandsNext;

namespace DiscordBot.Boss;

public class FundsGamble
{
    public int WinMoney { get; set; }
    public int WinPer { get; set; }
    public int Ante { get; set; }
    public int Charge { get; set; }
    public int Multiple { get; set; }

    public Dictionary<string, int> JoinPlayerInfoDictionary { get; set; }

    public FundsGamble(int winPer, int ante, int charge, int multiple)
    {
        JoinPlayerInfoDictionary = new Dictionary<string, int>();
        
        WinPer = winPer;
        Ante = ante;
        Charge = charge;
        Multiple = multiple;

        WinMoney = Ante * Multiple;
    }

    public int DoFundsGamble(CommandContext ctx)
    {
        int resultMoney = 0;

        WinMoney += Ante - Charge;
        
        var rand = new Random();
        int gambleRandom = rand.Next(1, 101);
        
        if (100 - WinPer < gambleRandom)
        {
            resultMoney = WinMoney;
            WinMoney = Ante * Multiple;
            JoinPlayerInfoDictionary.Clear();
        }
        else
        {
            string name = Utility.GetMemberDisplayName(ctx.Member);
            if (JoinPlayerInfoDictionary.ContainsKey(name))
            {
                int joinTotalMoney = JoinPlayerInfoDictionary[name] + (Ante - Charge);
                JoinPlayerInfoDictionary.Remove(name);
                JoinPlayerInfoDictionary.Add(name, joinTotalMoney);
            }
            else
            {
                JoinPlayerInfoDictionary.Add(name, Ante - Charge);
            }
        }

        return resultMoney;
    }
    
    public Dictionary<string, int> GetWinMoneyShareSortDictionary()
    {
        Dictionary<string, int> sortDict = JoinPlayerInfoDictionary.OrderByDescending(item => item.Value).ToDictionary(x => x.Key, x => x.Value);

        return sortDict;
    }
}