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
                roulette.Add(new Roulette(databaseRoulette.time, databaseRoulette.winner, value));
            }
        }

        return roulette;
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
}