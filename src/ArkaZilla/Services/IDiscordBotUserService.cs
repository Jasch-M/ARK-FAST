using ArkaZilla.Extensions.Logging;
using Discord.WebSocket;
using ArkaZilla.Extensions;

namespace ArkaZilla.Services;

public interface IDiscordBotUserService
{
    // Expose properties and methods that you want to be accessible from outside
    DiscordSocketClient Client { get; }
    DiscordLogger? Logger { get; }

    // Add any other methods you want to expose
    // For example:
    // Task SendMessageAsync(ulong channelId, string message);
}
