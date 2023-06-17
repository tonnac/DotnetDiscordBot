namespace DiscordBot.Equip;

/*
 101010
 */

public class EquipCalculator
{
    public static int WeaponUpgradeMoney = 5000;
    public static int RingUpgradeMoney = 10000;

    private static int CutNum = 10;
    
    public static List<UpgradePercentage> UpgradePercentages = new List<UpgradePercentage>()
    {
        new UpgradePercentage(100, 0),
        new UpgradePercentage(90, 5),
        new UpgradePercentage(75, 5),
        new UpgradePercentage(50, 10),
        new UpgradePercentage(35, 10),
        new UpgradePercentage(25, 15),
        new UpgradePercentage(15, 15),
        new UpgradePercentage(10, 20),
        new UpgradePercentage(5, 25)
    };


    public static void SetWeaponUpgradeMoney(int money)
    {
        WeaponUpgradeMoney = money;
    }
    public static void SetRingUpgradeMoney(int money)
    {
        RingUpgradeMoney = money;
    }

    public static int Upgrade(int currentUpgradeNum)
    {
        var rand = new Random();
        int upgradePer = rand.Next(1, 101);

        if (upgradePer <= UpgradePercentages[currentUpgradeNum].BrokenPer)
        {
            return -1; // Broken
        }
        else
        {
            if (upgradePer > UpgradePercentages[currentUpgradeNum].BrokenPer + UpgradePercentages[currentUpgradeNum].FailPer)
            {
                return 1; // Success
            }
            else
            {
                return 0; // fail
            }
        }
    }
    
    public static int GetWeaponUpgradeInfo(int equipValue)
    {
        return equipValue % CutNum;
    }
    
    public static int GetRingUpgradeInfo(int equipValue)
    {
        if (0 >= equipValue)
        {
            return 0;
        }
        
        return (equipValue / CutNum) % CutNum;
    }
}