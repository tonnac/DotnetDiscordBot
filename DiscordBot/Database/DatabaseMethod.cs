using System.Data;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Database.Tables;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<bool> UserDelete(CommandContext ctx)
    {
        return await UserDelete(ctx.Guild, ctx.User);
    }

    public async Task<List<DatabaseUser>> GetDatabaseUsers(CommandContext ctx)
    {
        if (null == _connection)
        {
            return new List<DatabaseUser>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM USER where guildid='{ctx.Guild.Id}'";

        return await GetDatabaseTable<DatabaseUser>(command);
    }

    public async Task<DatabaseUser> GetDatabaseUser(DiscordGuild guild, DiscordUser user)
    {
        DatabaseUser? foundUser = await GetDatabaseUser_Private(guild, user);

        if (foundUser != null)
        {
            return foundUser;
        }
        else
        {
            bool result = await UserRegister(guild, user);
            if (result)
            {
                foundUser = await GetDatabaseUser_Private(guild, user);
            }

            return foundUser ?? new DatabaseUser();
        }
    }

    private async Task<bool> UserRegister(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync(
            $"insert into USER (id, guildid, userid) values ('{GetSHA256(guild, user)}', '{guild.Id}', '{user.Id}')");
    }

    private async Task<bool> UserDelete(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync($"delete from USER where id='{GetSHA256(guild, user)}'");
    }

    private async Task<DatabaseUser?> GetDatabaseUser_Private(DiscordGuild guild, DiscordUser user)
    {
        if (null == _connection)
        {
            return new DatabaseUser();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM USER where id='{GetSHA256(guild, user)}'";

        var userList = await GetDatabaseTable<DatabaseUser>(command);
        return userList.Count > 0 ? userList[0] : null;
    }

    private async Task<bool> ExecuteNonQueryASync(string query)
    {
        if (null == _connection)
        {
            return false;
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = query;

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    private async Task<List<T>> GetDatabaseTable<T>(MySqlCommand command)
    {
        try
        {
            await using MySqlDataReader rdr = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(rdr);
            string jsonString = JsonConvert.SerializeObject(dataTable);
            List<T>? tables = JsonConvert.DeserializeObject<List<T>>(jsonString);
            return tables ?? new List<T>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new List<T>();
    }
}