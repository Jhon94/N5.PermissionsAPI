using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace N5.Permissions.Infrastructure.Data.Context
{
    public class PermissionsDbContextFactory : IDesignTimeDbContextFactory<PermissionsDbContext>
    {
        public PermissionsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PermissionsDbContext>();

            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=N5PermissionsDB;Integrated Security=true;TrustServerCertificate=true;";

            optionsBuilder.UseSqlServer(connectionString);

            return new PermissionsDbContext(optionsBuilder.Options);
        }
    }
}
