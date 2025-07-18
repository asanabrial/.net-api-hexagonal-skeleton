using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using HexagonalSkeleton.Application.Services.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// CQRS service configuration
    /// Command: PostgreSQL, Query: MongoDB
    /// </summary>
    public static class CqrsServiceExtension
    {
        public static IServiceCollection AddCqrsServices(this IServiceCollection services)
        {
            // Command side (Write operations)
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // Query side (Read operations)
            services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();
            
            // Interface Segregation Principle - Specialized interfaces
            services.AddScoped<IUserExistenceChecker>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not found"));
            services.AddScoped<IUserSearchService>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not found"));
            services.AddScoped<IUserBasicReader>(provider => 
                provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter 
                ?? throw new InvalidOperationException("UserReadRepositoryMongoAdapter not found"));

            // Sync service for read model consistency
            services.AddScoped<HexagonalSkeleton.Domain.Services.IUserSyncService, UserSyncService>();

            // Application services (Screaming Architecture)
            services.AddScoped<IUserRegistrationApplicationService, UserRegistrationApplicationService>();
            services.AddScoped<IUserProfileApplicationService, UserProfileApplicationService>();

            return services;
        }
    }
}
