using System.Text;
using DisCatSharp.Entities;
using DiscordBot.Database.Tables;
using MySqlConnector;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<List<Roulette>> GetRoulette(DiscordGuild guild, string name)
    {
        if (null == _connection)
        {
            return new List<Roulette>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText =
            $"select * FROM ROULETTEMEMBER A INNER JOIN ROULETTEMEMBER B on B.name = '{name}' and A.rouletteid = B.rouletteid";

        var databaseMembers = await GetDatabaseTable<DatabaseRouletteMember>(command);

        if (databaseMembers.Count == 0)
        {
            return new List<Roulette>();
        }

        Dictionary<string, List<string>> rouletteMembers = new Dictionary<string, List<string>>();

        foreach (var databaseRouletteMember in databaseMembers)
        {
            if (rouletteMembers.TryGetValue(databaseRouletteMember.rouletteid, out List<string>? vv))
            {
                vv.Add(databaseRouletteMember.name);
            }
            else
            {
                var news = new List<string>() { databaseRouletteMember.name };
                rouletteMembers.TryAdd(databaseRouletteMember.rouletteid, news);
            }
        }


        command.CommandText = $"select * from ROULETTE where guildid='{guild.Id}' and (";

        var keys = rouletteMembers.Keys.ToList();
        for (var i = 0; i < keys.Count; i++)
        {
            var id = $"id='{keys[i]}'";

            if (i != keys.Count - 1)
            {
                id += " or ";
            }
            else
            {
                id += ")";
            }

            command.CommandText += id;
        }

        var databaseRouletteList = await GetDatabaseTable<DatabaseRoulette>(command);

        if (databaseRouletteList.Count == 0)
        {
            return new List<Roulette>();
        }

        List<Roulette> roulette = new List<Roulette>();

        foreach (var databaseRoulette in databaseRouletteList)
        {
            if (rouletteMembers.TryGetValue(databaseRoulette.id, out List<string>? value))
            {
                roulette.Add(new Roulette(databaseRoulette.time, databaseRoulette.winner, value, databaseRoulette.messagelink));
            }
        }

        return roulette;
    }

    public async Task<List<Roulette>> GetRecentRoulette(DiscordGuild guild, int count)
    {
        if (null == _connection)
        {
            return new List<Roulette>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText =
            $"select * from ROULETTE where guildid='{guild.Id}' order by time desc limit {count}";

        var databaseRouletteList = await GetDatabaseTable<DatabaseRoulette>(command);

        List<Roulette> rouletteList = new List<Roulette>();

        foreach (var databaseRoulette in databaseRouletteList)
        {
            command.CommandText =
                $"select * from ROULETTEMEMBER where rouletteid='{databaseRoulette.id}'";

            var databaseRouletteMembers = await GetDatabaseTable<DatabaseRouletteMember>(command);
            var rouletteMembers = databaseRouletteMembers.Select((member => member.name)).ToList();
            if (databaseRouletteMembers.Count > 0)
            {
                rouletteList.Add(new Roulette(databaseRoulette.time, databaseRoulette.winner, rouletteMembers, databaseRoulette.messagelink));
            }
        }

        return rouletteList;
    }

    public async Task RegisterRoulette(Roulette roulette, DiscordGuild guild)
    {
        var key = GetSHA256(roulette.Time, guild.Id);
        bool result = await ExecuteNonQueryASync(
            $"insert into ROULETTE (id, guildid, time, winner) values ('{key}', '{guild.Id}', '{roulette.Time.ToString(Utility.TimeFormat)}', '{roulette.Winner}')");

        if (result)
        {
            string query = $"insert into ROULETTEMEMBER (id, rouletteid, name) values ";

            for (int i = 0; i < roulette.Members.Count; i++)
            {
                var rouletteMember = roulette.Members[i];
                var value =
                    $"('{GetSHA256_Internal(Encoding.UTF8.GetBytes($"{rouletteMember}{key}"))}', '{key}', '{rouletteMember}')";

                if (i != roulette.Members.Count - 1)
                {
                    value += ", ";
                }

                query += value;
            }

            await ExecuteNonQueryASync(query);
        }
    }
    public async Task<RouletteRanking> GetRouletteRanking(DiscordGuild guild)
    {
        if (null == _connection)
        {
            throw new Exception();
        }

        const int limitCount = 3;

        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText =
            $@"
            select name, wins, takingpartcount, (wins / takingpartcount) * 100 as winrate from
            (
            select ROULETTEMEMBER.name, count(ROULETTE.winner) as wins, count(ROULETTEMEMBER.rouletteid) as takingpartcount
            from ROULETTEMEMBER
            left join ROULETTE
            on ROULETTEMEMBER.name = ROULETTE.winner 
            and ROULETTE.guildid = '{guild.Id}'
            and ROULETTEMEMBER.rouletteid = ROULETTE.id
            group by ROULETTEMEMBER.name
            ) A
            where takingpartcount > (
            select avg(tc) from 
            (select name, count(name) as tc from ROULETTEMEMBER
            group by name
            having count(name) > 1) B)
            order by winrate
            limit {limitCount}";

        var winRates = await GetDatabaseTable<RouletteWinRate>(command);
        
        command.CommandText =
            $@"
            select name, spentcount, wins from
            (
            select ROULETTEMEMBER.name, count(ROULETTE.winner) as wins
            from ROULETTEMEMBER
            left join ROULETTE on ROULETTEMEMBER.name = ROULETTE.winner 
            and ROULETTE.guildid = '{guild.Id}'
            and ROULETTEMEMBER.rouletteid = ROULETTE.id
            group by ROULETTEMEMBER.name
            ) A
            left join (select
            ROULETTE.winner as rightwinner, 
            count(ROULETTEMEMBER.rouletteid) as spentcount
            from ROULETTEMEMBER
            inner join ROULETTE on ROULETTE.id = ROULETTEMEMBER.rouletteid 
            and ROULETTE.winner != ROULETTEMEMBER.name
            and ROULETTE.guildid = '{guild.Id}'
            group by rightwinner
            order by spentcount desc) B
            on A.name = B.rightwinner
            group by name
            order by spentcount desc
            limit {limitCount}";

        var spentCounts = await GetDatabaseTable<RouletteSpentCount>(command);

        command.CommandText =
            $@"
            select ROULETTEMEMBER.name, count(ROULETTEMEMBER.name) as takingpartcount,
            (select count(*) from ROULETTE where ROULETTE.guildid = '{guild.Id}') as totalgame, 
            count(ROULETTEMEMBER.name) / (select count(*) from ROULETTE) * 100 as playedgamerate
            FROM ROULETTE
            inner join ROULETTEMEMBER on ROULETTEMEMBER.rouletteid = ROULETTE.id
            where ROULETTE.guildid = '{guild.Id}'
            group by ROULETTEMEMBER.name
            order by takingpartcount desc
            limit {limitCount}";

        var takingParts = await GetDatabaseTable<RouletteTakingPart>(command);
        
        return new RouletteRanking(winRates, spentCounts, takingParts);
    }
}