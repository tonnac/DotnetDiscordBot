namespace DiscordBot.Equip;

/*
 99990999
 레벨  경험치  ????  보석  반지  무기
 99       99     0     9     9    9
 */

public class EquipCalculator
{
    public static int WeaponUpgradeMoney = 2000;
    public static int RingUpgradeMoney = 4000;
    public static int LevelUpgradeMoney = 10000;

    public static int Boss_WeaponUpgradeMultiplier = 5;
    public static int Boss_RingUpgradeMultiplier = 1;
    
    public static int Fish_WeaponUpgradeMultiplier = 1;
    public static int Dice_RingUpgradeMultiplier = 2;

    public static int Gold_GemUpgradeMultiplier = 5;
    public static int Pay_GemUpgradeMultiplier = 1000;

    public static int EquipCutNum = 10;

    public static int LevelCutNum = 10000;
    public static int XpCutNum = 100;
    
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
    public static void SetBoss_WeaponUpgradeMultiplier(int value)
    {
        Boss_WeaponUpgradeMultiplier = value;
    }
    public static void SetBoss_RingUpgradeMultiplier(int value)
    {
        Boss_RingUpgradeMultiplier = value;
    }
    public static void SetFish_WeaponUpgradeMultiplier(int value)
    {
        Fish_WeaponUpgradeMultiplier = value;
    }
    public static void SetDice_RingUpgradeMultiplier(int value)
    {
        Dice_RingUpgradeMultiplier = value;
    }
    public static void SetGold_GemUpgradeMultiplier(int value)
    {
        Gold_GemUpgradeMultiplier = value;
    }
    public static void SetPay_GemUpgradeMultiplier(int value)
    {
        Pay_GemUpgradeMultiplier = value;
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
        return equipValue % EquipCutNum;
    }
    
    public static int GetRingUpgradeInfo(int equipValue)
    {
        if (0 >= equipValue)
        {
            return 0;
        }
        
        return (equipValue / EquipCutNum) % EquipCutNum;
    }
    
    public static int GetGemUpgradeInfo(int equipValue)
    {
        if (0 >= equipValue)
        {
            return 0;
        }
        
        return (equipValue % LevelCutNum) / (EquipCutNum * EquipCutNum);
    }
    
    public static int GetLevel(int equipValue)
    {
        if (0 >= equipValue)
        {
            return 0;
        }
        
        return equipValue / (LevelCutNum * XpCutNum);
    }
    
    public static int GetXp(int equipValue)
    {
        if (0 >= equipValue)
        {
            return 0;
        }

        return (equipValue / LevelCutNum) % XpCutNum;
    }
}