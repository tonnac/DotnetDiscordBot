using DisCatSharp.Enums;

namespace DiscordBot.Boss;

public enum BossType : int
{
    Start = 0,
    Ant = 1,
    Bat = 2,
    Octopus = 3,
    Shark = 4,
    Unicorn = 5,
    Mammoth = 6,   
    Devil = 7,
    SlotMachine = 8,
    Alien = 9,
    Trex = 10,
    Dragon = 11,
    TheOffice = 12,
    End,
}

public class BossMonster
{
    public string BossEmojiCode { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMaxHp { get; set; }
    public int HitCount { get; set; }
    private Dictionary<string, int> TotalDamage { get; set; }

    public BossMonster(BossType type)
    {
        TotalDamage = new Dictionary<string, int>();
        SetBossMonsterInfo(type);
    }

    private void SetBossMonsterInfo(BossType type)
    {
        BossEmojiCode = GetBossEmojiCode(type);
        CurrentHp = GetBossMaxHp(type);
        CurrentMaxHp = CurrentHp;
        HitCount = 0;
        TotalDamage.Clear();
    }

    public KeyValuePair<string, int> GetBestDealer()
    {
        Dictionary<string, int> sortDict = TotalDamage.OrderByDescending(item => item.Value).ToDictionary(x => x.Key, x => x.Value);

        KeyValuePair<string, int> bestDealer = new KeyValuePair<string, int>();
        foreach (var item in sortDict)
        {
            bestDealer = item;
            break;
        }

        return bestDealer;
    }

    public bool IsKilledByDamage(string attacker, int damage, out KeyValuePair<string, int> bestDealerInfo)
    {
        if (CurrentHp <= damage)
        {
            damage = CurrentHp;
        }
        
        if (!TotalDamage.ContainsKey(attacker))
        {
            TotalDamage.Add(attacker, damage);
        }
        else
        {
            int totalDamage = TotalDamage[attacker] + damage;
            TotalDamage.Remove(attacker);
            TotalDamage.Add(attacker, totalDamage);
        }
        
        bestDealerInfo = GetBestDealer();
        
        HitCount++;
        CurrentHp -= damage;

        if (0 >= CurrentHp)
        {
            ResetBossMonster();
            return true;
        }

        return false;
    }

    private void ResetBossMonster()
    {
        var rand = new Random();
        int bossType = rand.Next((int)BossType.Start + 1, (int)BossType.End);
        SetBossMonsterInfo( (BossType)bossType );
    }

    public int GetBossMaxHp(BossType type)
    {
        switch (type)
        {
            default:
            case BossType.Ant:
                return 10;
            case BossType.Bat:
                return 100;
            case BossType.Octopus:
                return 200;
            case BossType.Shark:
                return 300;
            case BossType.Unicorn:
                return 400;
            case BossType.Mammoth:
                return 500;
            case BossType.Devil:
                return 666;
            case BossType.SlotMachine:
                return 777;
            case BossType.Alien:
                return 800;
            case BossType.Trex:
                return 1000;
            case BossType.Dragon:
                return 1500;
            case BossType.TheOffice:
                return 1818;
        }
    }
    
    public string GetBossEmojiCode(BossType type)
    {
        switch (type)
        {
            default:
            case BossType.Ant:
                return "\uD83D\uDC1C";
            case BossType.Bat:
                return "\uD83E\uDD87";
            case BossType.Octopus:
                return "\uD83D\uDC19";
            case BossType.Shark:
                return "\uD83E\uDD88";
            case BossType.Unicorn:
                return "\uD83E\uDD84";
            case BossType.Mammoth:
                return "\uD83E\uDDA3";
            case BossType.Devil:
                return "\uD83D\uDE08";
            case BossType.SlotMachine:
                return "\uD83C\uDFB0";
            case BossType.Alien:
                return "\uD83D\uDC7D";
            case BossType.Trex:
                return "\uD83E\uDD96";
            case BossType.Dragon:
                return "\uD83D\uDC09";
            case BossType.TheOffice:
                return "\uD83C\uDFEC";
        }
    }
}