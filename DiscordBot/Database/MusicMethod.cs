using DisCatSharp.Entities;
using DiscordBot.Database.Tables;
using DiscordBot.Music;
using MySqlConnector;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase
{
    public async Task<bool> RegisterMusic(MusicTrack track)
    {
        var member = await track.User.ConvertToMember(track.Channel.Guild);
        string format = "yyyy-MM-dd HH:mm:ss";
        return await ExecuteNonQueryASync(
            $"insert into MUSIC (id, identifier, title, uri, guildid, addedtime, starttime, finishtime, userid, nickname, priority) values ('{GetSHA256(track)}', '{track.LavaLinkTrack.Identifier}', '{track.LavaLinkTrack.Title}', '{track.LavaLinkTrack.Uri}', '{track.Channel.GuildId}', '{track.AddedTime.ToString(format)}', '{track.StartTime.ToString(format)}', '{track.FinishTime.ToString(format)}','{track.User.Id}', '{Utility.GetMemberDisplayName(member)}', '{track.TrackIndex}')");
    }

    public async Task<List<DatabaseMusic>> GetDatabaseMusics(DiscordGuild guild)
    {
        if (null == _connection)
        {
            return new List<DatabaseMusic>();
        }

        await using MySqlCommand command = _connection.CreateCommand();
        command.CommandText = $"select * FROM MUSIC where guildid='{guild.Id}'";

        return await GetDatabaseTable<DatabaseMusic>(command);
    }
}