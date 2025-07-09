using HexagonalSkeleton.Infrastructure.Persistence.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring Query database services (MongoDB)
    /// Handles the read side of CQRS pattern
    /// </summary>
    public static class QueryDatabaseServiceExtension
    {
        /// <summary>
        /// Configures Query database services with MongoDB for read operations
        /// Optimized for high-performance queries and complex filtering
        /// </summary>
        public static IServiceCollection AddQueryDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("HexagonalSkeletonRead");
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "HexagonalSkeletonRead";

            // Register MongoDB client as singleton
            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var mongoClient = new MongoClient(connectionString);
                return mongoClient;
            });

            // Register QueryDbContext as scoped
            services.AddScoped<QueryDbContext>(serviceProvider =>
            {
                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                return new QueryDbContext(mongoClient, databaseName);
            });

            return services;
        }
    }
}
