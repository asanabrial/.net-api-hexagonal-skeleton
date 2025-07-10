using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Adapters;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using HexagonalSkeleton.Infrastructure.EventHandlers;
using HexagonalSkeleton.Application.Services.Features;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring CQRS services
    /// Implements Command Query Responsibility Segregation pattern
    /// Follows clean architecture principles with proper separation of concerns
    /// </summary>
    public static class CqrsServiceExtension
    {
        /// <summary>
        /// Configures CQRS services with separate command and query repositories
        /// Command side: PostgreSQL for transactional consistency
        /// Query side: MongoDB for optimized read operations
        /// Implements Interface Segregation Principle with specialized services
        /// </summary>
        public static IServiceCollection AddCqrsServices(this IServiceCollection services)
        {
            // Command side repositories (Write operations)
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // Query side repositories (Read operations) 
            // services.AddScoped<IUserQueryRepository, UserQueryRepository>();
            
            // Use MongoDB for read operations (CQRS pattern)
            services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();
            
            // Register specialized interfaces for Interface Segregation Principle
            services.AddScoped<IUserExistenceChecker>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);
            services.AddScoped<IUserSearchService>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);
            services.AddScoped<IUserBasicReader>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);

            // Register sync service for eventual consistency between command and query stores
            services.AddScoped<HexagonalSkeleton.Domain.Services.IUserSyncService, UserSyncService>();
            services.AddScoped<UserSyncService>(); // Keep concrete class for any direct dependencies

            // Register pure domain event dispatcher (no MediatR dependency)
            services.AddScoped<HexagonalSkeleton.Application.Ports.IDomainEventDispatcher, HexagonalSkeleton.Infrastructure.Services.PureDomainEventDispatcher>();

            // Register pure domain event handlers (Clean Architecture - no MediatR dependency)
            // Note: Multiple handlers can be registered for the same event type
            services.AddScoped<HexagonalSkeleton.Application.Ports.IDomainEventHandler<HexagonalSkeleton.Domain.Events.UserCreatedEvent>, HexagonalSkeleton.Infrastructure.EventHandlers.Pure.UserCreatedPureHandler>();
            
            // Note: UserCreatedSyncPureHandler is registered separately in AddCqrsSyncHandlers() method
            // This allows conditional registration based on whether query store is configured

            // Register feature-based application services (Screaming Architecture)
            services.AddScoped<IUserRegistrationApplicationService, UserRegistrationApplicationService>();
            services.AddScoped<IUserProfileApplicationService, UserProfileApplicationService>();

            // Legacy adapters - remove these once fully migrated to CQRS
            // services.AddScoped<IUserWriteRepository, UserWriteRepositoryAdapter>();
            // services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();

            return services;
        }

        /// <summary>
        /// Adds CQRS sync handlers for eventual consistency between command and query stores
        /// Only call this method if QueryDbContext is properly configured
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Updated service collection</returns>
        public static IServiceCollection AddCqrsSyncHandlers(this IServiceCollection services)
        {
            // TODO: Implement sync handlers when needed
            // For now, the basic UserCreatedPureHandler handles domain events
            // Future: Add UserCreatedSyncPureHandler for CQRS eventual consistency
            
            return services;
        }
    }
}
