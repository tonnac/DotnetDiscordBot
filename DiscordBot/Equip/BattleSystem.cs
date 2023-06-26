using DiscordBot.Boss;
using DiscordBot.Database;
using DiscordBot.Resource;

namespace DiscordBot.Equip;

public class BattleSystem
{
    public static UserBattleInfo User_A = new UserBattleInfo();
    public static UserBattleInfo User_B = new UserBattleInfo();

    public static bool IsA_Ready = false;
    public static bool IsB_Ready = false;
    public static bool IsFighting = false;

    public static int FightMoney = 0;
    

    /*
  - ⬛⬛⬛⬛⬛ 0 
  - 🟧⬛⬛⬛⬛ 1~10
  - 🟥⬛⬛⬛⬛ 11~20
  - 🟥🟧⬛⬛⬛ 21~30
  - 🟥🟥⬛⬛⬛ 31~40
  - 🟥🟥🟧⬛⬛ 41~50
  - 🟥🟥🟥⬛⬛ 51~60
  - 🟥🟥🟥🟧⬛ 61~70
  - 🟥🟥🟥🟥⬛ 71~80
  - 🟥🟥🟥🟥🟧 81~90
  - 🟥🟥🟥🟥🟥 91~100
    */

    public static void ResetSystem()
    {
        User_A.Reset();
        User_B.Reset();

        IsA_Ready = false;
        IsB_Ready = false;
        IsFighting = false;
        FightMoney = 0;
    }

    public static async Task CalculateFightMoney(bool isUserAWin)
    {
        if (isUserAWin)
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.GetDatabaseUser(User_A.Guild, User_A.User);
            
            GoldQuery query = new GoldQuery(FightMoney*2);
            await database.UpdateUserGold(User_A.Guild, User_A.User, query);
        }
        else
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.GetDatabaseUser(User_B.Guild, User_B.User);
            
            GoldQuery query = new GoldQuery(FightMoney*2);
            await database.UpdateUserGold(User_B.Guild, User_B.User, query);
        }
    }
    
    public static string GetHpText(int currentHp, int maxHp)
    {
        return VEmoji.Heart + " " + Convert.ToString(currentHp) + " / " + Convert.ToString(maxHp);
    }

    public static string GetDamageText(int damage, int weaponDamage, bool isCritical)
    {
        string weaponText = 0 == weaponDamage ? "" : " +" + Convert.ToString(weaponDamage) + VEmoji.Weapon;
        string attackTypeEmoji = isCritical ? VEmoji.Fire : VEmoji.Boom;
        return 0 == damage ? VEmoji.SpiralEyes + " 0" : attackTypeEmoji + " " + Convert.ToString(damage) + weaponText;
    }

    public static string GetEquipText(int equipValue)
    {
        int tridentUpgrade = EquipCalculator.GetTridentUpgradeInfo(equipValue);
        int gemUpgrade = EquipCalculator.GetGemUpgradeInfo(equipValue);
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(equipValue);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(equipValue);

        return "+" + Convert.ToString(tridentUpgrade) + VEmoji.Trident + ", +" + Convert.ToString(gemUpgrade) + VEmoji.Gem + ", +" + Convert.ToString(ringUpgrade) + VEmoji.Ring + ", +" + Convert.ToString(weaponUpgrade) + VEmoji.Weapon;
    }
    
    public static string GetHpBarString(int hpPercentage, bool bRightSide = false)
    {
        string hpString = "";
        
        if (0 < hpPercentage)
        {
            if (!bRightSide)
            {
                for (int hp = 11; hp <= 100; hp += 20)
                {
                    if (hp < hpPercentage)
                    {
                        hpString += "🟥";
                    }
                    else if (hp-10 <= hpPercentage)
                    {
                        hpString += "🟧";
                    }
                }
            
                for (int hp = 81; hp > 0; hp -= 20)
                {
                    if (hp > hpPercentage)
                    {
                        hpString += "⬛";
                    }
                }
            }
            else
            {
                for (int hp = 81; hp > 0; hp -= 20)
                {
                    if (hp > hpPercentage)
                    {
                        hpString += "⬛";
                    }
                }
                
                for (int hp = 81; hp >= 1; hp -= 20)
                {
                    if (hp+10 <= hpPercentage)
                    {
                        hpString += "🟥";
                    }
                    else if (hp <= hpPercentage)
                    {
                        hpString += "🟧";
                    }
                }
                
                // 81 61 41 21 1
                // 34
                // ⬛⬛⬛⬛⬛ 0 
                // ⬛⬛⬛⬛🟧 1~10
                // ⬛⬛⬛🟧🟥 21~30
                // ⬛⬛⬛🟥🟥 31~40
                // 🟧🟥🟥🟥🟥 81~90
                // 🟥🟥🟥🟥🟥 91~100
            }
        }
        else
        {
            hpString += "⬛⬛⬛⬛⬛";
        }

        return hpString;
    }
}