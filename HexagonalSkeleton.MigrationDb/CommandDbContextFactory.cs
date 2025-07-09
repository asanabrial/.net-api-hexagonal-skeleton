using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using HexagonalSkeleton.Infrastructure.Persistence.Command;

namespace HexagonalSkeleton.MigrationDb
{
    /// <summary>
    /// Design-time factory for creating CommandDbContext instances
    /// Used by Entity Framework tools for migrations
    /// </summary>
    public class CommandDbContextFactory : IDesignTimeDbContextFactory<CommandDbContext>
    {
        public CommandDbContext CreateDbContext(string[] args)
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
            }
            
            var optionsBuilder = new DbContextOptionsBuilder<CommandDbContext>();
            
            // Use PostgreSQL for command database
            optionsBuilder.UseNpgsql(connectionString, 
                options => options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"));

            return new CommandDbContext(optionsBuilder.Options);
        }
    }
}
