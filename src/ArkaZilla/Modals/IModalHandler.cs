using Discord.WebSocket;

namespace ArkaZilla.Modals;

public interface IModalHandler
{
    public string ModalId { get; }

    public Task HandleModal(SocketModal modal);
}
