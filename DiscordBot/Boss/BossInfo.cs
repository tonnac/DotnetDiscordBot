using DiscordBot.Resource;

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
    MrKrabs = 12,
    Dragon = 13,
    TheOffice = 14,
    End = 15,
}

public class BossInfo
{
    public static int GetBossMaxHp(BossType type)
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
            case BossType.MrKrabs:
                return 1111;
            case BossType.Dragon:
                return 1500;
            case BossType.TheOffice:
                return 1818;
        }
    }
    
    public static int GetBossDropGold(BossType type)
    {
        switch (type)
        {
            default:
            case BossType.Mosquito:
            case BossType.Bat:
            case BossType.Octopus:
            case BossType.Shark:
            case BossType.Unicorn:
            case BossType.Skeleton:
            case BossType.Devil:
            case BossType.Alien:
            case BossType.AngryDevil:
            case BossType.Trex:
            case BossType.Dragon:
            case BossType.TheOffice:
                return GetBossMaxHp(type);
            case BossType.SlotMachine:
                return 7777;
            case BossType.MrKrabs:
                return 11111;
        }
    }
    
    public static string GetBossEmojiCode(BossType type)
    {
        switch (type)
        {
            default:
            case BossType.Mosquito:
                return VEmoji.Mosquito;
            case BossType.Bat:
                return VEmoji.Bat;
            case BossType.Octopus:
                return VEmoji.Octopus;
            case BossType.Shark:
                return VEmoji.Shark;
            case BossType.Unicorn:
                return VEmoji.Unicorn;
            case BossType.Skeleton:
                return VEmoji.Skeleton;
            case BossType.Devil:
                return VEmoji.Devil;
            case BossType.SlotMachine:
                return VEmoji.SlotMachine;
            case BossType.Alien:
                return VEmoji.Alien;
            case BossType.AngryDevil:
                return VEmoji.AngryDevil;
            case BossType.Trex:
                return VEmoji.Trex;
            case BossType.MrKrabs:
                return VEmoji.Crab;
            case BossType.Dragon:
                return VEmoji.Dragon;
            case BossType.TheOffice:
                return VEmoji.TheOffice;
        }
    }
}