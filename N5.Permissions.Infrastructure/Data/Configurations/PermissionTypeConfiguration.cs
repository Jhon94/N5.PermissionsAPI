using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Infrastructure.Data.Configurations
{
    public class PermissionTypeConfiguration : IEntityTypeConfiguration<PermissionType>
    {
        public void Configure(EntityTypeBuilder<PermissionType> builder)
        {
            builder.ToTable("PermissionTypes");

            builder.HasKey(pt => pt.Id);

            builder.Property(pt => pt.Id)
                .ValueGeneratedOnAdd();

            builder.Property(pt => pt.Description)
                .IsRequired()
                .HasMaxLength(200);

            // Unique constraint
            builder.HasIndex(pt => pt.Description)
                .IsUnique()
                .HasDatabaseName("IX_PermissionTypes_Description");
        }
    }
}
