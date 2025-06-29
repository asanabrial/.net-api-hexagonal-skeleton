using HexagonalSkeleton.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring production database services
    /// Clean separation - only handles production concerns
    /// </summary>
    public static class DatabaseServiceExtension
    {
        /// <summary>
        /// Configures production database services with PostgreSQL and connection pooling
        /// Optimized for production workloads with proper logging and error handling
        /// </summary>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStr = configuration.GetConnectionString("HexagonalSkeleton");

            services.AddDbContextPool<AppDbContext>(
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
