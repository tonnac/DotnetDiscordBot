using System.Data;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using MySqlConnector;
using Newtonsoft.Json;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    private async Task<bool> UserRegister(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync(
            $"insert into USER (id, guildid, userid) values ('{GetSHA256(guild, user)}', '{guild.Id}', '{user.Id}')");
    }

    public async Task<bool> UserDelete(CommandContext ctx)
    {
        return await UserDelete(ctx.Guild, ctx.User);
    }

    private async Task<bool> UserDelete(DiscordGuild guild, DiscordUser user)
    {
        return await ExecuteNonQueryASync($"delete from USER where id='{GetSHA256(guild, user)}'");
    }

    public async Task<List<DatabaseUser>> GetDatabaseUsers(CommandContext ctx)
    {
        if (null == _connection)
        {
            return new List<DatabaseUser>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM USER where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);

        return await GetDatabaseUsers(command);
    }

    private async Task<List<DatabaseUser>> GetDatabaseUsers(MySqlCommand command)
    {
        try
        {
            await using MySqlDataReader rdr = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(rdr);
            string jsonString = JsonConvert.SerializeObject(dataTable);
            List<DatabaseUser>? users = JsonConvert.DeserializeObject<List<DatabaseUser>>(jsonString);
            return users ?? new List<DatabaseUser>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new List<DatabaseUser>();
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

    private async Task<DatabaseUser?> GetDatabaseUser_Private(DiscordGuild guild, DiscordUser user)
    {
        if (null == _connection)
        {
            return new DatabaseUser();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM USER where id=@id";
        command.Parameters.AddWithValue("id", GetSHA256(guild, user));

        try
        {
            await using MySqlDataReader rdr = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(rdr);
            string jsonString = JsonConvert.SerializeObject(dataTable);
            List<DatabaseUser>? users = JsonConvert.DeserializeObject<List<DatabaseUser>>(jsonString);
            return users?[0];
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
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
}