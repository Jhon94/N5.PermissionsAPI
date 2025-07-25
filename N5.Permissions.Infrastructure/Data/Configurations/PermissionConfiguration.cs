using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Infrastructure.Data.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            builder.Property(p => p.EmployeeForename)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.EmployeeSurname)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.PermissionDate)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            // Foreign key relationship
            builder.HasOne(p => p.PermissionType)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.PermissionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(p => new { p.EmployeeForename, p.EmployeeSurname })
                .HasDatabaseName("IX_Permissions_Employee");

            builder.HasIndex(p => p.PermissionDate)
                .HasDatabaseName("IX_Permissions_Date");

            builder.HasIndex(p => p.PermissionTypeId)
                .HasDatabaseName("IX_Permissions_TypeId");
        }
    }
}
