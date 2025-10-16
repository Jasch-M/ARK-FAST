using Discord.WebSocket;

namespace ArkaZilla.Modals;

public interface IButtonHandler
{
    public string ButtonId { get; }

    public Task HandleButton(SocketMessageComponent component);
}
