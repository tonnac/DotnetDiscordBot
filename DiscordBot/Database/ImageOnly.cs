using System.Data;
using DisCatSharp.Entities;
using MySqlConnector;
using Newtonsoft.Json;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<List<ImageOnlyChannel>> GetImageOnlyChannels()
    {
        if (null == _connection)
        {
            return new List<ImageOnlyChannel>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM IMAGEONLYCHANNEL";

        try
        {
            await using MySqlDataReader rdr = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(rdr);
            string jsonString = JsonConvert.SerializeObject(dataTable);
            List<ImageOnlyChannel>? channels = JsonConvert.DeserializeObject<List<ImageOnlyChannel>>(jsonString);
            return channels ?? new List<ImageOnlyChannel>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new List<ImageOnlyChannel>();
    }

    public async Task<bool> RegisterImageOnlyChannels(DiscordChannel channel)
    {
        return await ExecuteNonQueryASync($"insert into IMAGEONLYCHANNEL (id) values ('{channel.Id}')");
    }

    public async Task<bool> UnRegisterImageOnlyChannels(DiscordChannel channel)
    {
        return await ExecuteNonQueryASync($"delete from IMAGEONLYCHANNEL where id='{channel.Id}'");
    }
    
}