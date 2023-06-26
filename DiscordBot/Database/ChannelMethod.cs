using DisCatSharp.Entities;
using DiscordBot.Core;
using DiscordBot.Database.Tables;
using MySqlConnector;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    
    public async Task<List<ChannelContents>> GetChannelContents()
    {
        if (null == _connection)
        {
            return new List<ChannelContents>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM CHANNEL";

        return await GetDatabaseTable<ChannelContents>(command);
    }
    public async Task<List<ChannelContents>> GetChannelContents(DiscordGuild guild)
    {
        if (null == _connection)
        {
            return new List<ChannelContents>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM CHANNEL where guildid='{guild.Id}'";

        return await GetDatabaseTable<ChannelContents>(command);
    }
    public async Task<ChannelContents> GetChannelContent(DiscordChannel channel)
    {
        ChannelContents? foundChannel = await GetChannelContent_Private(channel);

        if (foundChannel != null)
        {
            return foundChannel;
        }
        else
        {
            bool result = await ChannelRegister(channel);
            if (result)
            {
                foundChannel = await GetChannelContent_Private(channel);
            }

            return foundChannel ?? new ChannelContents();
        }
    }

    public delegate void ChannelContentsChanged(ChannelContents channelContents);

    public static ChannelContentsChanged OnChannelContentsChanged = null!;

    public async Task<string> ToggleChannelContents(DiscordChannel channel, ContentsFlag flag)
    {
        ChannelContents channelContent = await GetChannelContent(channel);
        bool hasFlag = ((ContentsFlag)channelContent.contentsvalue).HasFlag(flag);
        
        string symbol = hasFlag ? "-" : "+";
        bool result = await ExecuteNonQueryASync(
            $"update CHANNEL set contentsvalue = contentsvalue{symbol}{(ulong)flag} where id='{GetSHA256(channel.Guild, channel)}'");
        
        if (result)
        {
            if (hasFlag)
            {
                channelContent.contentsvalue -= (ulong)flag;
            }
            else
            {
                channelContent.contentsvalue += (ulong)flag;
            }
        
            OnChannelContentsChanged(channelContent);
        }
        return result ? symbol : "";
    }

    private async Task<bool> ChannelRegister(DiscordChannel channel)
    {
        return await ExecuteNonQueryASync(
            $"insert into CHANNEL (id, guildid, channelid) values ('{GetSHA256(channel.Guild, channel)}', '{channel.Guild.Id}', '{channel.Id}')");
    }

    private async Task<ChannelContents?> GetChannelContent_Private(DiscordChannel channel)
    {
        if (null == _connection)
        {
            return new ChannelContents();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM CHANNEL where id='{GetSHA256(channel.Guild, channel)}'";

        List<ChannelContents> channels = await GetDatabaseTable<ChannelContents>(command);
        return channels.Count > 0 ? channels[0] : null;
    }
}