using System.ComponentModel.DataAnnotations;
using Discord;

namespace ArkaZilla.Data.Models.Logging;

public class LogLocation : IEntity<ulong>
{
    public ulong Id { get; set; }

    [StringLength(100, ErrorMessage = "The identifier name must be less than 100 characters.")]
    public required string IdentifierName { get; set; }

    [StringLength(100, ErrorMessage = "The display name must be less than 100 characters.")]
    public required string DisplayName { get; set; }

    public required Logging Logging { get; set; }

    public bool Enabled { get; set; }
}
