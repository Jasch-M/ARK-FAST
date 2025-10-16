using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace ArkaZilla.Data.Models;

/// <summary>
/// Represents a base implementation of common properties for database entities.
/// </summary>
public sealed class User : IEntity<ulong>
{
    /// <inheritdoc/>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    /// <summary>
    /// Gets the identity date and time for this object.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset IdentityAt { get; init; } = DateTimeOffset.UtcNow;

    public ulong? DiscordId { get; private set; }

    public bool HasConsented { get; private set; }
}
