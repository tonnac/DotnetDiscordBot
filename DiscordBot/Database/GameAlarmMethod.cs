using DisCatSharp.ApplicationCommands.Context;
using MySqlConnector;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Commands;
using DiscordBot.Database.Tables;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<bool> SubscribeGame(CommandContext ctx, SubscribeGameFlag gameFlag, bool isActive)
    {
        return await SubscribeGame(ctx.Guild, ctx.Member, gameFlag, isActive);
    }
    
    public async Task<bool> SubscribeGame(InteractionContext ctx, SubscribeGameFlag gameFlag, bool isActive)
    {
        return await SubscribeGame(ctx.Guild, ctx.Member, gameFlag, isActive);
    }
    private async Task<bool> SubscribeGame(DiscordGuild guild, DiscordMember member, SubscribeGameFlag gameFlag, bool isActive)
    {
        string symbol = isActive ? "|" : "&";
        int flag = (int)(isActive ? gameFlag : ~gameFlag);
        bool result = await ExecuteNonQueryASync(
            $"update USER set gameflag = gameflag{symbol}{flag} where id='{GetSHA256(guild, member)}'");
        return result;
    }
    
    public async Task<List<DatabaseUser>> GetSubscribedUser(InteractionContext ctx, GameFlag gameFlag)
    {
        return await GetSubscribedUser(ctx.Guild, gameFlag);
    }
    public async Task<List<DatabaseUser>> GetSubscribedUser(CommandContext ctx, GameFlag gameFlag)
    {
        return await GetSubscribedUser(ctx.Guild, gameFlag);
    }
    private async Task<List<DatabaseUser>> GetSubscribedUser(DiscordGuild guild, GameFlag gameFlag)
    {
        if (null == _connection)
        {
            return new List<DatabaseUser>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select userid FROM USER where guildid='{guild.Id}' and gameflag & '{(int)gameFlag}'";
        return await GetDatabaseTable<DatabaseUser>(command);
    }
}