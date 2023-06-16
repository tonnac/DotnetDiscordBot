namespace DiscordBot.Equip;

public class UpgradePercentage
{
    public int SuccessPer;
    public readonly int FailPer;
    public readonly int BrokenPer;
    
    public UpgradePercentage(int successPer, int brokenPer)
    {
        SuccessPer = successPer;
        BrokenPer = brokenPer;
        FailPer = 100 - successPer - brokenPer;
    }
}