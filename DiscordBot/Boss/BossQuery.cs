namespace DiscordBot.Boss;

public class BossQuery
{
    public BossQuery(ulong damage, int killCount, int gold)
    {
        Damage = damage;
        KillCount = killCount;
        Gold = gold;
    }

    public ulong Damage { get; }
    public int KillCount { get; }
    public int Gold { get; }
}