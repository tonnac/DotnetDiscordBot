using System.Globalization;
using System.Reflection;
using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DiscordBot.Channels;
using DiscordBot.Music;
using DiscordBot.Resource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI_API;

namespace DiscordBot.Core;
public class Bot
{
    private readonly DiscordClient _client;
    private readonly DiscordMessageHandler _messageHandler;
    private readonly TerminateReceiver _terminateReceiver;
    private readonly ContentsChannels _contentsChannels;

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

        _messageHandler = new DiscordMessageHandler(_client);
        _contentsChannels = new ContentsChannels(_client);

        Dictionary<DiscordGuild, MusicPlayer> musicPlayers = new Dictionary<DiscordGuild, MusicPlayer>();
        _terminateReceiver = new TerminateReceiver(musicPlayers, _client);

        _client.UseInteractivity(new InteractivityConfiguration
        {
            PollBehaviour = PollBehaviour.DeleteEmojis,
            Timeout = TimeSpan.FromSeconds(5)
        });

        var services = new ServiceCollection()
            .AddSingleton(musicPlayers)
            .AddSingleton(_terminateReceiver)
            .AddSingleton(_contentsChannels)
            .AddSingleton(_client)
            .AddSingleton(_messageHandler)
            .AddSingleton(new OpenAIAPI(new APIAuthentication(Config.OpenAiApiKey)))
            .BuildServiceProvider();
        
        _client.GuildDownloadCompleted += ClientOnGuildDownloadCompleted;

        CommandsNextExtension? commandNext = _client.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new List<string> { Config.Prefix },
            ServiceProvider = services
        });
        commandNext.UnregisterCommands(commandNext.FindCommand("help", out string _));
        commandNext.RegisterCommands(Assembly.GetExecutingAssembly());
    }

    private Task ClientOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Factory.StartNew(() => _terminateReceiver.Run());
        Task.Factory.StartNew(() => _contentsChannels.SendNotice());
        return Task.CompletedTask;
    }

    public async Task MainAsync()
    {
        await _client.ConnectAsync();
        await _messageHandler.RunASync();
        await ConnectLaveLinkASync();
        await Task.Delay(-1);
    }

    private async Task ConnectLaveLinkASync()
    {
        var endPoint = new ConnectionEndpoint
        {
            Port = Config.LavaLinkPort,
            Hostname = Config.LavaLinkHostname
        };
        
        var lavaConfig = new LavalinkConfiguration
        {
            Password = Config.LavaLinkAuthorization,
            RestEndpoint = endPoint,
            SocketEndpoint = endPoint
        };
        LavalinkExtension? lavaLink = _client.UseLavalink();
        if (lavaLink == null)
        {
            throw new Exception("LavaLink null");
        }

        await lavaLink.ConnectAsync(lavaConfig);
    }
}