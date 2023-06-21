using DisCatSharp.Enums;
using DiscordBot.Resource;

namespace DiscordBot.Boss;

public class BossMonster
{
    public BossType Type { get; set; }
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
        Type = type;
        BossEmojiCode = BossInfo.GetBossEmojiCode(type);
        CurrentHp = BossInfo.GetBossMaxHp(type);
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

    public bool IsBossType(BossType bossType)
    {
        return Type == bossType;
    }
}