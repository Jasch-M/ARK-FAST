using Discord.WebSocket;

namespace ArkaZilla;

public class Channel
{
    public ISocketAudioChannel AudioChannel { get; set; }

    private List<SocketGuildUser> Blacklist { get; set; } = [];

    private List<SocketGuildUser> Whitelist { get; set; } = [];

    private bool IsVisible { get; set; } = true;

    public Channel(ISocketAudioChannel audioChannel)
    {
        AudioChannel = audioChannel;

    }
}
