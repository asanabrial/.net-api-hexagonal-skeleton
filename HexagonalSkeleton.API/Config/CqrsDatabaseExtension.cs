using HexagonalSkeleton.Infrastructure.Persistence;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using Microsoft.EntityFrameworkCore;
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
        public static IServiceCollection AddCqrsDatabases(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure command database (PostgreSQL)
            services.AddCommandDatabaseInternal(configuration);
            
            // Configure query database (MongoDB)
            services.AddQueryDatabaseInternal(configuration);
            
            return services;
        }
        
        /// <summary>
        /// Configures the command (write) database with PostgreSQL
        /// Internal method to avoid naming conflicts
        /// </summary>
        private static IServiceCollection AddCommandDatabaseInternal(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionStr = configuration.GetConnectionString("HexagonalSkeleton");
            
            if (string.IsNullOrEmpty(connectionStr))
            {
                throw new InvalidOperationException("Connection string 'HexagonalSkeleton' not found in configuration.");
            }

            // En un patrón CQRS bien implementado, solo necesitamos un contexto para comandos (escritura)
            // CommandDbContext será el único contexto para operaciones de escritura usando PostgreSQL
            services.AddDbContextPool<CommandDbContext>(
                dbContextOptions =>
                    dbContextOptions.UseNpgsql(
                        connectionStr, 
                        options => options.MigrationsAssembly("HexagonalSkeleton.MigrationDb"))
                    // The following options help with debugging
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
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

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
            {
                // Use default values for missing configuration
                connectionString ??= "mongodb://localhost:27017";
                databaseName ??= "HexagonalSkeletonRead";
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
