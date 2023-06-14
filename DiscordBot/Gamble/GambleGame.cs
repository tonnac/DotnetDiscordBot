namespace DiscordBot.Boss;

public class GambleGame
{
    public int GameAnte  { get; set; }
    
    public int Percentage_GoldPrize { get; set; }
    public int Percentage_SilverPrize { get; set; }
    public int Percentage_BronzePrize { get; set; }
    
    public int Reward_GoldPrize { get; set; }
    public int Reward_SilverPrize { get; set; }
    public int Reward_BronzePrize { get; set; }

    public void SetPercentage(int gold, int silver, int bronze)
    {
        Percentage_GoldPrize = gold;
        Percentage_SilverPrize = silver;
        Percentage_BronzePrize = bronze;
    }
    public void SetReward(int gold, int silver, int bronze)
    {
        Reward_GoldPrize = gold;
        Reward_SilverPrize = silver;
        Reward_BronzePrize = bronze;
    }

    public int DoGamble()
    {
        int result = 0;
        int nonePercentage = 100 - (Percentage_GoldPrize + Percentage_SilverPrize + Percentage_BronzePrize);
        
        var rand = new Random();
        int gambleRandom = rand.Next(1, 101);

        if (nonePercentage < gambleRandom)
        {
            result = Reward_BronzePrize;
        }
        if (nonePercentage + Percentage_BronzePrize < gambleRandom)
        {
            result = Reward_SilverPrize;
        }
        if (nonePercentage + Percentage_BronzePrize + Percentage_SilverPrize < gambleRandom)
        {
            result = Reward_GoldPrize;
        }

        return result;
    }
}