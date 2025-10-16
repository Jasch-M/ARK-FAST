using Discord;
using Discord.WebSocket;

namespace ArkaZilla;

public class ChannelMaker
{
    private SocketVoiceChannel AudioChannel { get; set; }

    private DiscordSocketClient Client { get; set; }

    public ChannelMaker(SocketVoiceChannel audioChannel, DiscordSocketClient client)
    {
        AudioChannel = audioChannel;

        Client = client;

        client.UserVoiceStateUpdated += ClientOnUserVoiceSettingChange;
    }

    private Task ClientOnUserVoiceSettingChange(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        var guildUser = socketUser as SocketGuildUser;
        if (guildUser is null)
            return Task.CompletedTask;

        if (guildUser.VoiceChannel is null && after.VoiceChannel.ConnectedUsers.Count == 0 && after.VoiceChannel.Name.StartsWith("'s Channel"))
        {
            return after.VoiceChannel.DeleteAsync();
        }

        if (before.VoiceChannel.Id != AudioChannel.Id || after.VoiceChannel.Id != AudioChannel.Id)
            return Task.CompletedTask;

        if (before.VoiceChannel.ConnectedUsers.Count == 0 && after.VoiceChannel.ConnectedUsers.Count > 0)
        {
            return CreateChannelWithUser(guildUser);
        }

        return Task.CompletedTask;
    }

    private async Task CreateChannelWithUser(SocketGuildUser guildUser)
    {
        IVoiceChannel? channel = await AudioChannel.Category.Guild.CreateVoiceChannelAsync(guildUser.Username + "'s Channel",
            x => x.CategoryId = AudioChannel.CategoryId);
    }
}
