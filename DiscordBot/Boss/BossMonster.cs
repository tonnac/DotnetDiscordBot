using DisCatSharp.Enums;

namespace DiscordBot.Boss;

public enum BossType : int
{
    Bat = 0,
    Octopus = 1,
    Shark = 2,
    Unicorn = 3,
    Mammoth = 4,   
    Devil = 5,
    Vampire = 6,
    Dragon = 7,
    Alien = 8,
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

    public bool IsKilledByDamage(string attacker, int damage)
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
        int bossType = rand.Next((int) BossType.Bat, (int) BossType.End);
        SetBossMonsterInfo( (BossType)bossType );
    }

    private int GetBossMaxHp(BossType type)
    {
        switch (type)
        {
            default:
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
            case BossType.Vampire:
                return 700;
            case BossType.Dragon:
                return 1000;
            case BossType.Alien:
                return 1234;
        }
    }
    
    private string GetBossEmojiCode(BossType type)
    {
        switch (type)
        {
            default:
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
            case BossType.Vampire:
                return "\uD83E\uDDDB";
            case BossType.Dragon:
                return "\uD83D\uDC09";
            case BossType.Alien:
                return "\uD83D\uDC7D";
        }
    }
}