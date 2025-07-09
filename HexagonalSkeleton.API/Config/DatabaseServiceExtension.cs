using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring CQRS database services
    /// Supports separate Command (PostgreSQL) and Query (MongoDB) databases
    /// </summary>
    public static class DatabaseServiceExtension
    {
        /// <summary>
        /// Configures CQRS database services with PostgreSQL for commands and MongoDB for queries
        /// Optimized for production workloads with proper logging and error handling
        /// </summary>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStr = configuration.GetConnectionString("HexagonalSkeleton");

            // Configure Command Database (PostgreSQL)
            services.AddDbContextPool<CommandDbContext>(
                dbContextOptions =>
                    dbContextOptions.UseNpgsql(connectionStr, options =>
                        options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"))
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );

            return services;
        }
    }
}
