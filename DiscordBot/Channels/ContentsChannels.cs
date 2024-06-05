using DisCatSharp;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DiscordBot.Commands;
using DiscordBot.Core;
using DiscordBot.Database;
using DiscordBot.Database.Tables;

namespace DiscordBot.Channels;

public class ContentsChannels
{
    private readonly DiscordClient _client;

    public ContentsChannels(DiscordClient client)
    {
        _client = client;
    }
    public static async Task<string> ToggleChannelContent(DiscordChannel channel, ContentsFlag contentsFlag)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        return await database.ToggleChannelContents(channel, contentsFlag);
    }
    public async Task SendNotice()
    {
        foreach (var (_, discordGuild) in _client.Guilds)
        {
            ulong noticeChannelId = await GetNoticeChannelId(discordGuild);

            if (discordGuild.Channels.TryGetValue(noticeChannelId, out DiscordChannel? channel))
            {
                IReadOnlyList<DiscordMessage>? messages = await channel.GetMessagesAsync();
                if (messages != null)
                {
                    foreach (DiscordMessage discordMessage in messages)
                    {
                        await discordMessage.DeleteAsync();
                    }
                }

                await channel.SendMessageAsync(Help.GetHelp(_client));
            }
        }
    }
    private async Task<ulong> GetNoticeChannelId(DiscordGuild guild)
    {
        return await GetChannelId(guild, ContentsFlag.Notice);
    }
    
    public async Task<ulong> GetMusicChannelId(DiscordGuild guild)
    {
        return await GetChannelId(guild, ContentsFlag.Music);
    }

    private async Task<ulong> GetChannelId(DiscordGuild guild, ContentsFlag flag)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        var channelContents = await database.GetChannelContents(guild);
        foreach (ChannelContents content in channelContents)
        {
            if (((ContentsFlag)content.contentsvalue).HasFlag(flag))
            {
                return content.channelid;
            }
        }
        return 0;
    }

    public async Task<bool> IsBossGameChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.BossGame);
    }
    public async Task<bool> IsFishingChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.Fishing);
    }
    public async Task<bool> IsGambleChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.Gamble);
    }
    public async Task<bool> IsForgeChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.Forge);
    }
    public async Task<bool> IsBattleChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.Battle);
    }
    public async Task<bool> IsDisableChatChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.DisableChat);
    }
    public async Task<bool> IsMusicChannel(CommandContext ctx)
    {
        return await IsAvailableContents(ctx.Channel, ContentsFlag.Music);
    }

    private async Task<bool> IsAvailableContents(DiscordChannel channel, ContentsFlag flag)
    {
        using var database = new DiscordBotDatabase();
        await database.ConnectASync();
        ChannelContents channelContents = await database.GetChannelContent(channel);
        return ((ContentsFlag)channelContents.contentsvalue).HasFlag(flag);
    }
}