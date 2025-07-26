using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace N5.Permissions.Infrastructure.Data.Context
{
    public class PermissionsDbContextFactory : IDesignTimeDbContextFactory<PermissionsDbContext>
    {
        public PermissionsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PermissionsDbContext>();

            var connectionString = "Server=localhost,1433;Database=N5PermissionsDB_Dev;User Id=sa;Password=Password123!;TrustServerCertificate=true;Encrypt=false;";

            optionsBuilder.UseSqlServer(connectionString);

            return new PermissionsDbContext(optionsBuilder.Options);
        }
    }
}
