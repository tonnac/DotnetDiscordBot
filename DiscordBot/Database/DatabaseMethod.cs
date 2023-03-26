using System.Data;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using MySqlConnector;
using Newtonsoft.Json;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<List<DatabaseUser>> GetDatabaseUsers(CommandContext ctx)
    {
        return await GetDatabaseUsers(ctx.Guild);
    }
    
    public async Task<List<DatabaseUser>> GetDatabaseUsers(DiscordGuild guild)
    {
        if (null == _connection)
        {
            return new List<DatabaseUser>();
        }

        MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select userid FROM USER where {guild.Id}";

        MySqlDataReader rdr = await command.ExecuteReaderAsync();
        DataTable dataTable = new DataTable();
        dataTable.Load(rdr);
        string jsonString = JsonConvert.SerializeObject(dataTable);
        List<DatabaseUser>? users = JsonConvert.DeserializeObject<List<DatabaseUser>>(jsonString);
        return users ?? new List<DatabaseUser>();
    }

    public async Task<bool> UserRegister(CommandContext ctx)
    {
        return await UserRegister(ctx.Guild, ctx.User);
    }
    
    public async Task<bool> UserRegister(DiscordGuild guild, DiscordUser user)
    {
        if (null == _connection)
        {
            return false;
        }

        MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"insert into USER (id, guildid, userid) values (@id, @guildid, @userid)";
        command.Parameters.AddWithValue("@id", GetSHA256(guild, user));
        command.Parameters.AddWithValue("@guildid", guild.Id);
        command.Parameters.AddWithValue("@userid", user.Id);

        return await command.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> UserDelete(CommandContext ctx)
    {
        return await UserDelete(ctx.Guild, ctx.User);
    }

    public async Task<bool> UserDelete(DiscordGuild guild, DiscordUser user)
    {
        if (null == _connection)
        {
            return false;
        }
        // ReSharper disable once StringLiteralTypo
        MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"delete from USER where id=@id";
        command.Parameters.AddWithValue("@id", GetSHA256(guild, user));

        return await command.ExecuteNonQueryAsync() == 1;
    }
    
}