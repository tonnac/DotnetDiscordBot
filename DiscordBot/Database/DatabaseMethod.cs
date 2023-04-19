using System.Data;
using DisCatSharp.CommandsNext;
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
        if (null == _connection)
        {
            return false;
        }
        // ReSharper disable once StringLiteralTypo
        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"insert into IMAGEONLYCHANNEL (id) values (@id)";
        command.Parameters.AddWithValue("@id", channel.Id);

        bool result = false;
        try
        {
            result = await command.ExecuteNonQueryAsync() == 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return result;
    }
    
    public async Task<bool> UnRegisterImageOnlyChannels(DiscordChannel channel)
    {
        if (null == _connection)
        {
            return false;
        }
        // ReSharper disable once StringLiteralTypo
        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"delete from IMAGEONLYCHANNEL where id=@id";
        command.Parameters.AddWithValue("@id", channel.Id);

        bool result = false;
        try
        {
            result = await command.ExecuteNonQueryAsync() == 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return result;
    }
    
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

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select userid FROM USER where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", guild.Id);

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
        if (null == _connection)
        {
            return new DatabaseUser();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select userid FROM USER where guildid=@guildid and userid=@userid";
        command.Parameters.AddWithValue("guildid", guild.Id);
        command.Parameters.AddWithValue("userid", user.Id);

        try
        {
            await using MySqlDataReader rdr = await command.ExecuteReaderAsync();
            DataTable dataTable = new DataTable();
            dataTable.Load(rdr);
            string jsonString = JsonConvert.SerializeObject(dataTable);
            List<DatabaseUser>? users = JsonConvert.DeserializeObject<List<DatabaseUser>>(jsonString);
            return users != null ? users[0] : new DatabaseUser();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return new DatabaseUser();
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

        var member = await GetDatabaseUser(guild, user);
        if (member.userid != 0)
        {
            return false;
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"insert into USER (id, guildid, userid) values (@id, @guildid, @userid)";
        command.Parameters.AddWithValue("@id", GetSHA256(guild, user));
        command.Parameters.AddWithValue("@guildid", guild.Id);
        command.Parameters.AddWithValue("@userid", user.Id);

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
        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"delete from USER where id=@id";
        command.Parameters.AddWithValue("@id", GetSHA256(guild, user));

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