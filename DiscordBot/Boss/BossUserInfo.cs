using DisCatSharp.Entities;

namespace DiscordBot.Boss;

public class BossUserInfo
{
    public DiscordGuild Guild { get; set; }
    public DiscordUser User { get; set; }
    public DiscordMember Member { get; set; }
    public int TotalDamage { get; set; }
}