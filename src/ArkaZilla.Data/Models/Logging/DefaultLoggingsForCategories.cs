using Discord;

namespace ArkaZilla.Data.Models.Logging;

public class DefaultLoggingsForCategories : IEntity<ulong>
{
    public ulong Id { get; set; }

    public required Logging Logging { get; set; }

    public required ulong CategoryId { get; set; }

    public required ulong GuildId { get; set; }
}
