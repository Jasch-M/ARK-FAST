using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArkaZilla.Modules.Data.RoleAssociation;

public class RoleAssociationChannelEntityTypeConfiguration : IEntityTypeConfiguration<RoleAssociationChannel>
{
    public void Configure(EntityTypeBuilder<RoleAssociationChannel> builder)
    {
        builder.ToTable("Channels", RoleAssociationDbContext.Schema);
    }
}
