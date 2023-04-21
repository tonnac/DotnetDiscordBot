using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DiscordBot.Database;

namespace DiscordBot.Core;

public class DiscordMessageHandler
{
    private readonly SortedSet<ulong> _imageOnlyChannels = new();
    public DiscordMessageHandler(DiscordClient client)
    {
        client.MessageCreated += MessageCreated;
    }

    public async Task ToggleChannel(CommandContext ctx)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        if (_imageOnlyChannels.Contains(ctx.Channel.Id))
        {
            var result = await database.UnRegisterImageOnlyChannels(ctx.Channel);
            if (result)
            {
                _imageOnlyChannels.Remove(ctx.Channel.Id);
                await ctx.RespondAsync("채팅이 가능합니다.");
            }
        }
        else
        {
            var result = await database.RegisterImageOnlyChannels(ctx.Channel);
            if (result)
            {
                _imageOnlyChannels.Add(ctx.Channel.Id);
                await ctx.RespondAsync("채팅이 금지됐습니다.");
            }
        }
    }

    public bool IsImageOnlyChannel(DiscordChannel discordChannel)
    {
        return _imageOnlyChannels.Contains(discordChannel.Id);
    }

    public async Task RunASync()
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var channels = await database.GetImageOnlyChannels();
        foreach (var imageOnlyChannel in channels)
        {
            _imageOnlyChannels.Add(imageOnlyChannel.id);
        }
    }
    
    private async Task MessageCreated(DiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Author.IsBot)
        {
            return;
        }

        if (_imageOnlyChannels.Contains(args.Channel.Id) == false)
        {
            return;
        }
        
        if (args.Message.Content == string.Empty)
        {
            return;
        }
        
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
}