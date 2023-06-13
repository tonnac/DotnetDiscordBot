using System.Data;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Boss;
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
    public async Task<List<DatabaseUser>> GetAramUser(CommandContext ctx)
    {
        if (null == _connection)
        {
            return new List<DatabaseUser>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select userid FROM USER where guildid=@guildid and aram=@aram";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);
        command.Parameters.AddWithValue("@aram", 1);
        
        return await GetDatabaseUsers(command);
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
    
    public async Task<bool> UpdateBossRaid(CommandContext ctx, BossQuery query)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set bosskillcount = bosskillcount+{query.KillCount}, bosstotaldamage = bosstotaldamage+{query.Damage}, gold = gold+{query.Gold}, combatcount = combatcount+{query.CombatCount} where id=@id";
        command.Parameters.AddWithValue("@id", GetSHA256(ctx.Guild, ctx.User));
        
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
    public async Task<bool> UpdateFishingGold(CommandContext ctx, FishingQuery query)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set gold = gold+{query.Gold} where id=@id";
        command.Parameters.AddWithValue("@id", GetSHA256(ctx.Guild, ctx.User));
        command.Parameters.AddWithValue("@gold", query.Gold);
        
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
    
    public async Task<bool> ResetBossKillCount(CommandContext ctx)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set bosskillcount = 0 where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);
        
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
    public async Task<bool> ResetBossTotalDamage(CommandContext ctx)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set bosstotaldamage = 0 where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);
        
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
    public async Task<bool> ResetGold(CommandContext ctx)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set gold = 0 where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);
        
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
    public async Task<bool> ResetCombatCount(CommandContext ctx)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        
        command.CommandText = $"update USER set combatcount = 0 where guildid=@guildid";
        command.Parameters.AddWithValue("@guildid", ctx.Guild.Id);
        
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
    
    public async Task<bool> ActiveAram(CommandContext ctx, bool isActive)
    {
        if (null == _connection)
        {
            return false;
        }
        
        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = @"update USER set aram=@aram where id=@id";
        command.Parameters.AddWithValue("@aram", isActive ? 1 : 0);
        command.Parameters.AddWithValue("@id", GetSHA256(ctx.Guild, ctx.User));
        
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
    
    private async Task<bool> UserRegister(DiscordGuild guild, DiscordUser user)
    {
        if (null == _connection)
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

    private async Task<bool> UserDelete(DiscordGuild guild, DiscordUser user)
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