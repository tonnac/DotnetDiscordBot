using System.Globalization;
using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DiscordBot.Music;
using DiscordBot.Resource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI_API;

namespace DiscordBot.Core;
public class Bot
{
    private readonly DiscordClient _client;
    private readonly Config? _config;

    public Bot()
    {
        using (StreamReader r = new StreamReader("config.json"))
        {
            string json = r.ReadToEnd();
            _config = JsonConvert.DeserializeObject<Config>(json);
        }

        Localization.Culture = new CultureInfo(_config.Locale, false);
        
        _client = new DiscordClient(new DiscordConfiguration
        {
            Token = _config.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.All,
        });


        var services = new ServiceCollection()
            .AddSingleton<Dictionary<DiscordGuild, MusicPlayer>>()
            .AddSingleton(_client)
            .AddSingleton(_config)
            .AddSingleton(new OpenAIAPI(new APIAuthentication(_config.OpenAIAPIKey)))
            .BuildServiceProvider();

        CommandsNextExtension? commandNext = _client.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new List<string>{ _config.Prefix },
            ServiceProvider = services
        });
        commandNext.UnregisterCommands(commandNext.FindCommand("help", out string rawArguments));
        commandNext.RegisterCommands(Assembly.GetExecutingAssembly());
    }
    
    public async Task MainAsync()
    {
        await _client.ConnectAsync();
        
        var endPoint = new ConnectionEndpoint
        {
            Port = _config.Port,
            Hostname = _config.Hostname
        };
        
        var lavaConfig = new LavalinkConfiguration
        {
            Password = _config.Authorization,
            RestEndpoint = endPoint,
            SocketEndpoint = endPoint
        };
        LavalinkExtension? lavaLink = _client.UseLavalink();
        if (lavaLink == null)
        {
            throw new Exception("LavaLink null");
        }

        await lavaLink.ConnectAsync(lavaConfig);
        await Task.Delay(-1);
    }
}