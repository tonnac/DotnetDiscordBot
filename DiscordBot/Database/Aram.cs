using MySqlConnector;
using DisCatSharp.CommandsNext;
using DiscordBot.Database.Tables;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<bool> ActiveAram(CommandContext ctx, bool isActive)
    {
        return await ExecuteNonQueryASync(
            $"update USER set aram={(isActive ? 1 : 0)} where id='{GetSHA256(ctx.Guild, ctx.User)}'");
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

        return await GetDatabaseTable<DatabaseUser>(command);
    }
}