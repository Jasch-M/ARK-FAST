using System.ComponentModel.DataAnnotations.Schema;
using Discord;

namespace ArkaZilla.Modules.Data.RoleAssociation;

public class RoleAssociationGroup : IEntity<ulong>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    public string Name { get; set; }

    public UserInclusionMode UserInclusionMode { get; set; }
}

public enum UserInclusionMode
{
    Strict,
    Loose,
}
