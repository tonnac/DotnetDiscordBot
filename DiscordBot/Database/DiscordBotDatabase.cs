using System.Security.Cryptography;
using System.Text;
using DisCatSharp.Entities;
using DiscordBot.Music;
using MySqlConnector;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase : IDisposable
{
    private MySqlConnection? _connection = null;

    public async Task<bool> RegisterMessage(string message)
    {
        return await ExecuteNonQueryASync(
            $"insert into MESSAGE (time, message) values ('{Utility.GetCurrentTime().ToString(Utility.TimeFormat)}', '{message}')");
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
        }
    }

    public async Task ConnectASync()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = Config.DatabaseServer,
            UserID = "root",
            Port = Config.DatabasePort,
            Password = Config.DatabasePassword,
            Database = Config.DatabaseName
        };

        _connection = new MySqlConnection(builder.ConnectionString);
        try
        {
            await _connection.OpenAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _connection = null;
        }
    }

    // ReSharper disable once InconsistentNaming
    private static string GetSHA256(DiscordGuild guild, DiscordUser user)
    {
        return GetSHA256_Internal(Encoding.UTF8.GetBytes($"{guild.Id}{user.Id}"));
    }
    // ReSharper disable once InconsistentNaming
    private static string GetSHA256(DiscordGuild guild, DiscordChannel channel)
    {
        return GetSHA256_Internal(Encoding.UTF8.GetBytes($"{guild.Id}{channel.Id}"));
    }

    // ReSharper disable once InconsistentNaming
    private static string GetSHA256(MusicTrack track)
    {
        return GetSHA256_Internal(Encoding.UTF8.GetBytes($"{track.AddedTime.Ticks}{track.LavaLinkTrack.Info.Identifier}{track.User.Id}"));
    }
    // ReSharper disable once InconsistentNaming
    private static string GetSHA256(DateTime time, ulong guildid)
    {
        return GetSHA256_Internal(Encoding.UTF8.GetBytes($"{guildid}{time}"));
    }
    
    private static string GetSHA256_Internal(byte[] inBytes)
    {
        SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(inBytes);

        StringBuilder builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }
}