using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DiscordBot.Database;
using DiscordBot.Database.Tables;

namespace DiscordBot.Core;

public class DiscordMessageHandler
{
    private readonly SortedSet<ulong> _disableChatChannels = new();
    private readonly SortedSet<ulong> _noticeChannels = new();
    private readonly SortedSet<ulong> _saveChannels = new();
    public DiscordMessageHandler(DiscordClient client)
    {
        client.MessageCreated += MessageCreated;
        client.MessageDeleted += OnMessageDeleted;
        
        DiscordBotDatabase.OnChannelContentsChanged += OnChannelContentsChanged;
    }

    private void OnChannelContentsChanged(ChannelContents channel)
    {
        if (_disableChatChannels.Contains(channel.channelid) && ((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.DisableChat) == false)
        {
            _disableChatChannels.Remove(channel.channelid);
        }
        else if (_noticeChannels.Contains(channel.channelid) &&
                 ((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Notice) == false)
        {
            _noticeChannels.Remove(channel.channelid);
        }
        else if (_saveChannels.Contains(channel.channelid) &&
                 ((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Save) == false)
        {
            _saveChannels.Remove(channel.channelid);
        }
        else if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.DisableChat))
        {
            _disableChatChannels.Add(channel.channelid);
        }
        else if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Notice))
        {
            _noticeChannels.Add(channel.channelid);
        }
        else if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Save))
        {
            _saveChannels.Add(channel.channelid);
        }
    }

    public bool IsDisableChatChannel(DiscordChannel discordChannel)
    {
        return _disableChatChannels.Contains(discordChannel.Id);
    }
    private bool IsNoticeChannel(DiscordChannel discordChannel)
    {
        return _noticeChannels.Contains(discordChannel.Id);
    }
    
    private bool IsSaveChannel(DiscordChannel discordChannel)
    {
        return _saveChannels.Contains(discordChannel.Id);
    }

    public async Task RunASync()
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var channels = await database.GetChannelContents();
        foreach (var channel in channels)
        {
            if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.DisableChat))
            {
                _disableChatChannels.Add(channel.channelid);
            }
            
            if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Notice))
            {
                _noticeChannels.Add(channel.channelid);
            }
            
            if (((ContentsFlag)channel.contentsvalue).HasFlag(ContentsFlag.Save))
            {
                _saveChannels.Add(channel.channelid);
            }
        }
    }

    private async Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
    {
        if (IsSaveChannel(args.Channel))
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.DeleteMessage(args);
        }
    }
    
    private async Task MessageCreated(DiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Author.IsBot)
        {
            return;
        }

        async void DeleteMessage()
        {
            await args.Message.DeleteAsync();

            Task.Run(async () =>
            {
                DiscordMessage? message = await args.Channel.SendMessageAsync("채팅을 할수없는 채널입니다.");
                await Task.Delay(5000);
                if (message != null)
                {
                    await message.DeleteAsync();
                }
            });
        }

        if (IsDisableChatChannel(args.Channel) && args.Message.Content != string.Empty)
        {
            DeleteMessage();
        }
        
        if (IsNoticeChannel(args.Channel))
        {
            DeleteMessage();
        }
        
        if (IsSaveChannel(args.Channel))
        {
            using var database = new DiscordBotDatabase();
            await database.ConnectASync();
            await database.RegisterMessage(args);
        }
    }
}