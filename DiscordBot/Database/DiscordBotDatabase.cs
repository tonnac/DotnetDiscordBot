using System.Security.Cryptography;
using System.Text;
using DisCatSharp.Entities;
using MySqlConnector;

namespace DiscordBot.Database;

public partial class DiscordBotDatabase : IDisposable
{
    private MySqlConnection? _connection = null;

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
        SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes($"{guild.Id}{user.Id}"));

        StringBuilder builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

}