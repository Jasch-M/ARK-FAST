using System.ComponentModel.DataAnnotations;
using Discord;

namespace ArkaZilla.Data.Models.Logging;

public class Logging : IEntity<ulong>
{
    public ulong Id { get; set; }

    [StringLength(50, ErrorMessage = "The display name must be less than 50 characters.")]
    public required string DisplayName { get; set; }

    [StringLength(50, ErrorMessage = "The call name must be less than 50 characters.")]
    public required string CallName { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }
}
