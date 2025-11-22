using Discord;

namespace ArkaZilla.Data.Models.Logging;

public class DefaultLoggingsForServers : IEntity<ulong>
{
    public ulong Id { get; set; }

    public required Logging Logging { get; set; }

    public required ulong GuildId { get; set; }
}
