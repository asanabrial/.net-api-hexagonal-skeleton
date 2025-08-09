using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// CQRS service configuration following Hexagonal Architecture principles.
    /// Separates Command (Write) and Query (Read) responsibilities with optimized data stores.
    /// Implements Interface Segregation Principle with specialized service contracts.
    /// Command: PostgreSQL for transactional consistency, Query: MongoDB for read performance
    /// </summary>
    public static class CqrsServiceExtension
    {
        public static IServiceCollection AddCqrsServices(this IServiceCollection services)
        {
            // === COMMAND SIDE (Write Operations) ===
            // PostgreSQL for ACID transactions and data consistency
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // === QUERY SIDE (Read Operations) ===
            // MongoDB for optimized read performance and denormalized data
            services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();
            
            // === INTERFACE SEGREGATION PRINCIPLE ===
            // Specialized interfaces for specific read operations (ISP compliance)
            services.AddScoped<IUserExistenceChecker>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not registered"));
            
            services.AddScoped<IUserSearchService>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not registered"));
            
            services.AddScoped<IUserBasicReader>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not registered"));

            // === EVENTUAL CONSISTENCY ===
            // Sync service for maintaining read model consistency via domain events
            services.AddScoped<HexagonalSkeleton.Domain.Services.IUserSyncService, UserSyncService>();

            // Note: Application Services removed as they were legacy code not used by controllers
            // Controllers use MediatR pattern directly with Command/Query handlers

            return services;
        }
    }
}
