using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using HexagonalSkeleton.Application.Services.Features;
using Microsoft.Extensions.DependencyInjection;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Simple CQRS service configuration
    /// Command: PostgreSQL, Query: MongoDB
    /// </summary>
    public static class CqrsServiceExtension
    {
        public static IServiceCollection AddCqrsServices(this IServiceCollection services)
        {
            // Command side (Write)
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // Query side (Read)
            services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();
            
            // Specialized interfaces
            services.AddScoped<IUserExistenceChecker>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);
            services.AddScoped<IUserSearchService>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);
            services.AddScoped<IUserBasicReader>(provider => 
                (provider.GetRequiredService<IUserReadRepository>() as UserReadRepositoryMongoAdapter)!);

            // Sync service for read model consistency
            services.AddScoped<HexagonalSkeleton.Domain.Services.IUserSyncService, UserSyncService>();

            // Application services
            services.AddScoped<IUserRegistrationApplicationService, UserRegistrationApplicationService>();
            services.AddScoped<IUserProfileApplicationService, UserProfileApplicationService>();

            return services;
        }
    }
}
