using DisCatSharp.Entities;
using DiscordBot.Resource;

namespace DiscordBot.Equip;

public class BattleSystem
{
    public static UserBattleInfo User_A = new UserBattleInfo();
    public static UserBattleInfo User_B = new UserBattleInfo();

    public static bool IsA_Ready = false;
    public static bool IsB_Ready = false;
    public static bool IsFighting = false;
    

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
        int ringUpgrade = EquipCalculator.GetRingUpgradeInfo(equipValue);
        int weaponUpgrade = EquipCalculator.GetWeaponUpgradeInfo(equipValue);

        return "+" + Convert.ToString(tridentUpgrade) + VEmoji.Trident + ", +" + Convert.ToString(ringUpgrade) + VEmoji.Ring + ", +" + Convert.ToString(weaponUpgrade) + VEmoji.Weapon;
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