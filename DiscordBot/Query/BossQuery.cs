namespace DiscordBot.Boss;

public class BossQuery
{
    public BossQuery(ulong damage, int killCount, int gold, int combatCount)
    {
        Damage = damage;
        KillCount = killCount;
        Gold = gold;
        CombatCount = combatCount;
    }

    public ulong Damage { get; }
    public int KillCount { get; }
    public int Gold { get; }
    public int CombatCount { get; }
}