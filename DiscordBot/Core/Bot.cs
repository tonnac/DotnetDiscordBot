using System.Globalization;
using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DiscordBot.Music;
using DiscordBot.Resource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI_API;

namespace DiscordBot.Core;
public class Bot
{
    private readonly DiscordClient _client;

    public Bot()
    {
        Localization.Culture = new CultureInfo(Config.Locale, false);
        
        _client = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.All,
        });

        _client.UseInteractivity(new InteractivityConfiguration()
        {
            PollBehaviour = PollBehaviour.DeleteEmojis,
            Timeout = TimeSpan.FromSeconds(5)
        });

        var services = new ServiceCollection()
            .AddSingleton<Dictionary<DiscordGuild, MusicPlayer>>()
            .AddSingleton(_client)
            .AddSingleton(new OpenAIAPI(new APIAuthentication(Config.OpenAiApiKey)))
            .BuildServiceProvider();

        CommandsNextExtension? commandNext = _client.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new List<string>{ Config.Prefix },
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
            Port = Config.Port,
            Hostname = Config.Hostname
        };
        
        var lavaConfig = new LavalinkConfiguration
        {
            Password = Config.Authorization,
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