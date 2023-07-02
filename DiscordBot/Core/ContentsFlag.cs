namespace DiscordBot.Core;

[Flags]
public enum ContentsFlag : ulong
{
    Notice = 0x01,
    DisableChat = Notice << 1,
    BossGame = DisableChat << 1,
    Fishing = BossGame << 1,
    Gamble = Fishing << 1,
    Forge = Gamble << 1,
    Battle = Forge << 1,
}