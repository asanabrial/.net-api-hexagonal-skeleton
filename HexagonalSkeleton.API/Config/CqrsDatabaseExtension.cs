using HexagonalSkeleton.Infrastructure.Persistence;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring CQRS databases
    /// Following CQRS architecture principles with clean separation of read and write responsibilities
    /// </summary>
    public static class CqrsDatabaseExtension
    {
        /// <summary>
        /// Configures all CQRS databases (command and query stores)
        /// Entry point for database configuration
        /// </summary>
        public static IServiceCollection AddCqrsDatabases(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Configure command database (PostgreSQL)
            services.AddCommandDatabaseInternal(configuration, environment);
            
            // Configure query database (MongoDB)
            services.AddQueryDatabaseInternal(configuration);
            
            return services;
        }
        
        /// <summary>
        /// Configures the command (write) database with PostgreSQL
        /// Internal method to avoid naming conflicts
        /// </summary>
        private static IServiceCollection AddCommandDatabaseInternal(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var connectionStr = configuration.GetConnectionString("HexagonalSkeleton");

            if (string.IsNullOrEmpty(connectionStr))
            {
                throw new InvalidOperationException("Connection string 'HexagonalSkeleton' not found in configuration.");
            }

            // In a well-implemented CQRS pattern, we only need one context for commands (write)
            // CommandDbContext will be the only context for write operations using PostgreSQL
            services.AddDbContext<CommandDbContext>(
                dbContextOptions =>
                {
                    dbContextOptions.UseNpgsql(
                        connectionStr,
                        options => options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"))
                        // The following options help with debugging
                        .LogTo(Console.WriteLine, LogLevel.Information)
                        .EnableDetailedErrors();

                    // Sensitive data logging exposes SQL parameter values (passwords, tokens).
                    // Restrict it to Development so credentials never reach production logs.
                    if (environment.IsDevelopment())
                    {
                        dbContextOptions.EnableSensitiveDataLogging();
                    }
                }
            );

            return services;
        }

        /// <summary>
        /// Configures the query (read) database with MongoDB
        /// Internal method to avoid naming conflicts
        /// </summary>
        private static IServiceCollection AddQueryDatabaseInternal(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("HexagonalSkeletonRead");
            var databaseName = configuration["MongoDb:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'HexagonalSkeletonRead' not found in configuration.");
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("MongoDb:DatabaseName not found in configuration.");
            }

            // Register MongoDB client as singleton
            services.AddSingleton<IMongoClient>(sp => 
                new MongoClient(connectionString));

            // Register query database context
            services.AddScoped<QueryDbContext>(sp => 
                new QueryDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));

            return services;
        }
    }
}
