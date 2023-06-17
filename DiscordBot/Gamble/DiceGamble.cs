namespace DiscordBot.Boss;

public class DiceGamble
{
    public int DoDiceGamble(int ante, out int userNum, out int comNum, int userRingUpgrade)
    {
        int resultMoney = 0;
        
        var rand = new Random();
        userNum = rand.Next(1, 101);
        comNum = rand.Next(1, 101);

        if (userNum + userRingUpgrade > comNum)
        {
            resultMoney = ante * 2;
        }

        return resultMoney;
    }
}