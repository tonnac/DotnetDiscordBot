using System.Net;
using DisCatSharp;
using DisCatSharp.Entities;
using DiscordBot.Commands;
using DiscordBot.Music;
using Newtonsoft.Json;

namespace DiscordBot.Core;

public class TerminateReceiver
{
    private static readonly string FileName = "playlists.json";
    private readonly HttpListener _httpListener;
    private readonly Dictionary<DiscordGuild, MusicPlayer> _musicPlayers;
    private readonly DiscordClient _client;
    public TerminateReceiver(Dictionary<DiscordGuild, MusicPlayer> musicPlayers, DiscordClient client)
    {
        _client = client;
        _musicPlayers = musicPlayers;
        _httpListener = new HttpListener();
        
        _httpListener.Prefixes.Add(string.Format($"http://+:{Config.Port}/"));
    }

    public async Task Run()
    {
        if (File.Exists(FileName))
        {
            using StreamReader streamRd = File.OpenText(FileName);
            await using JsonTextReader jsonTextReader = new JsonTextReader(streamRd);
            JsonSerializer jsonSerializer = new JsonSerializer();
            List<Playlist>? playlists = jsonSerializer.Deserialize<List<Playlist>>(jsonTextReader);

            if (playlists != null)
            {
                foreach (Playlist playlist in playlists)
                {
                    foreach (var keyValuePair in _client.Guilds)
                    {
                        if (keyValuePair.Value.Channels.TryGetValue(playlist.Channel, out DiscordChannel? channel))
                        {
                            var musicPlayer = await MusicModules.CreateMusicPlayer(_client, channel, _musicPlayers);
                            if (musicPlayer != null)
                            {
                                await musicPlayer.Play(playlist);
                            }
                        }
                    }
                }

                streamRd.Close();
                File.Create(FileName).Close();
            }
        }

        if (_httpListener.IsListening == false)
        {
            _httpListener.Start();
            Task.Factory.StartNew(Action);
        }
    }

    private async void Action()
    {
        while (true)
        {
            HttpListenerContext context = await this._httpListener.GetContextAsync();

            string rawurl = context.Request.RawUrl;
            string httpmethod = context.Request.HttpMethod;

            string result = "";

            List<Playlist> playlists = new List<Playlist>();
            
            foreach (KeyValuePair<DiscordGuild,MusicPlayer> musicPlayer in _musicPlayers)
            {
                playlists.Add(musicPlayer.Value.GetPlayList());
                await musicPlayer.Value.Connection.DisconnectAsync();
            }
            
            using var sw = new StreamWriter(FileName);
            using var jw = new JsonTextWriter(sw);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(jw, playlists);

            result += $"httpmethod = {httpmethod}\r\n";
            result += $"rawurl = {rawurl}\r\n";
            
            context.Response.Close();
            break;
        }
    }
}