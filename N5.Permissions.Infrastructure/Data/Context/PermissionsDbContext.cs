using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Infrastructure.Data.Configurations;

namespace N5.Permissions.Infrastructure.Data.Context
{
    public class PermissionsDbContext : DbContext
    {
        public PermissionsDbContext(DbContextOptions<PermissionsDbContext> options) : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionType> PermissionTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionTypeConfiguration());

            // Seed data
            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Permission Types
            modelBuilder.Entity<PermissionType>().HasData(
                new PermissionType { Id = 1, Description = "Vacation Leave" },
                new PermissionType { Id = 2, Description = "Sick Leave" },
                new PermissionType { Id = 3, Description = "Personal Leave" },
                new PermissionType { Id = 4, Description = "Maternity/Paternity Leave" },
                new PermissionType { Id = 5, Description = "Emergency Leave" }
            );
        }
    }
}
