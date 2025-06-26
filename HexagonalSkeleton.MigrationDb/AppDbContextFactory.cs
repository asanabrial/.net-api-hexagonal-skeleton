using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using HexagonalSkeleton.Infrastructure.Persistence;

namespace HexagonalSkeleton.MigrationDb
{
    /// <summary>
    /// Design-time factory for creating AppDbContext instances
    /// Used by Entity Framework tools for migrations
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json in the API project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "HexagonalSkeleton.API"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("HexagonalSkeleton");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'HexagonalSkeleton' not found.");
            }            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            
            optionsBuilder.UseMySql(connectionString, serverVersion, options =>
                options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
