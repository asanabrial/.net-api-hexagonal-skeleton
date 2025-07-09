using HexagonalSkeleton.Infrastructure.Persistence.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring Command database services (PostgreSQL)
    /// Handles the write side of CQRS pattern
    /// </summary>
    public static class CommandDatabaseServiceExtension
    {
        /// <summary>
        /// Configures Command database services with PostgreSQL for write operations
        /// Optimized for transactional consistency and data integrity
        /// </summary>
        public static IServiceCollection AddCommandDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStr = configuration.GetConnectionString("HexagonalSkeleton");

            services.AddDbContextPool<CommandDbContext>(
                dbContextOptions =>
                    dbContextOptions.UseNpgsql(connectionStr, options =>
                        options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"))
                    // Command store optimizations
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging(false) // Security: Don't log sensitive data in production
                    .EnableDetailedErrors(false) // Performance: Disable in production
            );

            return services;
        }
    }
}
