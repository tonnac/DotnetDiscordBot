using DisCatSharp.Enums;

namespace DiscordBot.Boss;

public enum BossType : int
{
    Start = 0,
    Mosquito = 1,
    Bat = 2,
    Octopus = 3,
    Shark = 4,
    Unicorn = 5,
    Skeleton = 6,   
    Devil = 7,
    SlotMachine = 8,
    Alien = 9,
    AngryDevil = 10,
    Trex = 11,
    Dragon = 12,
    TheOffice = 13,
    End,
}

public class BossMonster
{
    public string BossEmojiCode { get; set; }
    public int CurrentHp { get; set; }
    public int CurrentMaxHp { get; set; }
    public int HitCount { get; set; }
    private Dictionary<ulong, BossUserInfo> TotalDamageDictionary { get; set; }

    public BossMonster(BossType type)
    {
        TotalDamageDictionary = new Dictionary<ulong, BossUserInfo>();
        SetBossMonsterInfo(type);
    }

    private void SetBossMonsterInfo(BossType type)
    {
        BossEmojiCode = GetBossEmojiCode(type);
        CurrentHp = GetBossMaxHp(type);
        CurrentMaxHp = CurrentHp;
        HitCount = 0;
        TotalDamageDictionary.Clear();
    }

    public KeyValuePair<ulong, BossUserInfo> GetBestDealer()
    {
        Dictionary<ulong, BossUserInfo> sortDict = TotalDamageDictionary.OrderByDescending(item => item.Value.TotalDamage).ToDictionary(x => x.Key, x => x.Value);

        KeyValuePair<ulong, BossUserInfo> bestDealer = new KeyValuePair<ulong, BossUserInfo>();
        foreach (var item in sortDict)
        {
            bestDealer = item;
            break;
        }

        return bestDealer;
    }

    public bool IsKilledByDamage(BossUserInfo info, out KeyValuePair<ulong, BossUserInfo> bestDealerInfo)
    {
        if (CurrentHp <= info.TotalDamage)
        {
            info.TotalDamage = CurrentHp;
        }
        
        if (!TotalDamageDictionary.ContainsKey(info.User.Id))
        {
            TotalDamageDictionary.Add(info.User.Id, info);
        }
        else
        {
            BossUserInfo bossUserInfo = TotalDamageDictionary[info.User.Id];
            bossUserInfo.TotalDamage += info.TotalDamage;
            TotalDamageDictionary.Remove(info.User.Id);
            TotalDamageDictionary.Add(info.User.Id, bossUserInfo);
        }
        
        bestDealerInfo = GetBestDealer();
        
        HitCount++;
        CurrentHp -= info.TotalDamage;

        if (0 >= CurrentHp)
        {
            ResetBossMonster();
            return true;
        }

        return false;
    }

    public void ResetBossMonster()
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
            case BossType.Mosquito:
                return 10;
            case BossType.Bat:
                return 100;
            case BossType.Octopus:
                return 200;
            case BossType.Shark:
                return 300;
            case BossType.Unicorn:
                return 400;
            case BossType.Skeleton:
                return 444;
            case BossType.Devil:
                return 666;
            case BossType.SlotMachine:
                return 777;
            case BossType.Alien:
                return 800;
            case BossType.AngryDevil:
                return 999;
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
            case BossType.Mosquito:
                return "\uD83E\uDD9F";
            case BossType.Bat:
                return "\uD83E\uDD87";
            case BossType.Octopus:
                return "\uD83D\uDC19";
            case BossType.Shark:
                return "\uD83E\uDD88";
            case BossType.Unicorn:
                return "\uD83E\uDD84";
            case BossType.Skeleton:
                return "\uD83D\uDC80";
            case BossType.Devil:
                return "\uD83D\uDE08";
            case BossType.SlotMachine:
                return "\uD83C\uDFB0";
            case BossType.Alien:
                return "\uD83D\uDC7D";
            case BossType.AngryDevil:
                return "\uD83D\uDC7F";
            case BossType.Trex:
                return "\uD83E\uDD96";
            case BossType.Dragon:
                return "\uD83D\uDC09";
            case BossType.TheOffice:
                return "\uD83C\uDFEC";
        }
    }
}