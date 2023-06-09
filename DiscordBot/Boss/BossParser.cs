namespace DiscordBot.Boss;

public class BossParser
{
    private Dictionary<string, int> KillCount { get; set; }
    private Dictionary<string, int> TotalGold { get; set; }
    private Dictionary<string, int> TotalDeal { get; set; }

    public BossParser()
    {
        KillCount = new Dictionary<string, int>();
        TotalGold = new Dictionary<string, int>();
        TotalDeal = new Dictionary<string, int>();
    }

    public void AddKillCount(string attacker, int addKillCount)
    {
        if (!KillCount.ContainsKey(attacker))
        {
            KillCount.Add(attacker, addKillCount);
        }
        else
        {
            int value = KillCount[attacker] + addKillCount;
            KillCount.Remove(attacker);
            KillCount.Add(attacker, value);
        }
    }
    
    public void AddTotalGold(string attacker, int addGold)
    {
        if (!TotalGold.ContainsKey(attacker))
        {
            TotalGold.Add(attacker, addGold);
        }
        else
        {
            int value = TotalGold[attacker] + addGold;
            TotalGold.Remove(attacker);
            TotalGold.Add(attacker, value);
        }
    }
    
    public void AddTotalDeal(string attacker, int addDeal)
    {
        if (!TotalDeal.ContainsKey(attacker))
        {
            TotalDeal.Add(attacker, addDeal);
        }
        else
        {
            int value = TotalDeal[attacker] + addDeal;
            TotalDeal.Remove(attacker);
            TotalDeal.Add(attacker, value);
        }
    }

    public Dictionary<string, int> GetKillCountRankDictionary()
    {
        Dictionary<string, int> sortDict = KillCount.OrderByDescending(item => item.Value).ToDictionary(x => x.Key, x => x.Value);

        return sortDict;
    }
    
    public Dictionary<string, int> GetTotalGoldRankDictionary()
    {
        Dictionary<string, int> sortDict = TotalGold.OrderByDescending(item => item.Value).ToDictionary(x => x.Key, x => x.Value);

        return sortDict;
    }
    
    public Dictionary<string, int> GetTotalDealRankDictionary()
    {
        Dictionary<string, int> sortDict = TotalDeal.OrderByDescending(item => item.Value).ToDictionary(x => x.Key, x => x.Value);

        return sortDict;
    }

    public void ResetBossParser()
    {
        KillCount.Clear();
        TotalGold.Clear();
        TotalDeal.Clear();
    }
}