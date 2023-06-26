using DisCatSharp.Entities;
using DiscordBot.Database;
using DiscordBot.Database.Tables;

namespace DiscordBot.Equip;

public class UserBattleInfo
{
    public DiscordGuild Guild = null as DiscordGuild;
    public DiscordUser User = null as DiscordUser;
    public string Name;
    public int EquipValue;
    
    public int Level;
    public int MaxHp;
    public int CurrentHp;

    public void Reset()
    {
        Guild = null as DiscordGuild;
        User = null as DiscordUser;
        Name = "";
        EquipValue = 0;
        Level = 0;
        MaxHp = 0;
        CurrentHp = 0;
    }

    public async void SetUserBattleInfo(DiscordGuild guild, DiscordUser user, DiscordMember member)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        DatabaseUser battleUserDatabase= await database.GetDatabaseUser(guild, user);

        Name = Utility.GetMemberDisplayName(member);
        
        EquipValue = battleUserDatabase.equipvalue;

        Level = EquipCalculator.GetLevel(EquipValue);
        MaxHp = CurrentHp = (Level * 100) + EquipCalculator.GetGemUpgradeInfo(EquipValue) * EquipCalculator.Battle_GemUpgradeMultiplier + 1000;
    }

    public void GetAttackDamage(out int baseDamage, out int weaponDamage, out bool isCritical)
    {
        int tridentUpgrade = EquipCalculator.GetTridentUpgradeInfo(EquipValue) * 9;
        int weaponUpgrade = (EquipCalculator.GetWeaponUpgradeInfo(EquipValue) + tridentUpgrade) * EquipCalculator.Boss_WeaponUpgradeMultiplier;
        int ringUpgrade = (EquipCalculator.GetRingUpgradeInfo(EquipValue) + tridentUpgrade) * EquipCalculator.Boss_RingUpgradeMultiplier;

        isCritical = false;
        
        var rand = new Random();
        int missPer = 10;
        int critPer = 15;
        int attackPer = 100 - missPer - critPer;
        int damage = rand.Next(1, 101);
        int attackChance = rand.Next(1, 101);
        
        if (missPer >= attackChance + ringUpgrade) // miss
        {
            weaponUpgrade = 0;
            damage = 0;
        }
        else
        {
            // attack
            
            if (missPer + attackPer < attackChance + ringUpgrade) // critical
            {
                isCritical = true;
                weaponUpgrade *= 2;
                damage = damage * 2 + 100;
            }
        }

        baseDamage = damage;
        weaponDamage = weaponUpgrade;
    }
}