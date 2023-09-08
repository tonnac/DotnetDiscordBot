using Newtonsoft.Json;

namespace DiscordBot.Database.Tables;

public class DatabaseRoulette
{
    [JsonProperty] public ulong guildid;
    [JsonProperty] public string id;
    [JsonProperty] public DateTime time;
    [JsonProperty] public string winner;
    [JsonProperty] public string messagelink;
}

public class DatabaseRouletteMember
{
    [JsonProperty] public string name;
    [JsonProperty] public string rouletteid;
}

public class RouletteWinRate
{
    [JsonProperty] public string name;
    [JsonProperty] public int wins;
    [JsonProperty] public int takingpartcount;
    [JsonProperty] public float winrate;
}

public class RouletteSpentCount
{
    [JsonProperty] public string name;
    [JsonProperty] public int spentcount;
    [JsonProperty] public int wins;
}

public class RouletteTakingPart
{
    [JsonProperty] public string name;
    [JsonProperty] public int takingpartcount;
    [JsonProperty] public int totalgame;
    [JsonProperty] public float playedgamerate;
}