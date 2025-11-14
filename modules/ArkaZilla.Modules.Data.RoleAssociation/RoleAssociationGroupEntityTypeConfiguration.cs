using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArkaZilla.Modules.Data.RoleAssociation;

public class RoleAssociationGroupEntityTypeConfiguration : IEntityTypeConfiguration<RoleAssociationGroup>
{
    public void Configure(EntityTypeBuilder<RoleAssociationGroup> builder)
    {
        builder.ToTable("Groups", RoleAssociationDbContext.Schema);
    }
}
