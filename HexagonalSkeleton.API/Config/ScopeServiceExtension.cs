using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure;
using HexagonalSkeleton.Infrastructure.Adapters;
using HexagonalSkeleton.Infrastructure.Services;
using HexagonalSkeleton.Application.Services;

namespace HexagonalSkeleton.API.Config
{
    public static class ScopeServiceExtension
    {
        /// <summary>
        /// Extension method for IServiceCollection to register application services for dependency injection.
        /// Follows Hexagonal Architecture principles by registering ports and adapters
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            // Note: Repository registrations are now handled by AddCqrsServices() 
            // to properly implement CQRS pattern with separate read/write stores
            
            // Domain Services (DDD)
            services.AddScoped<UserDomainService>();
            services.AddScoped<IDomainServiceFactory, DomainServiceFactory>();
            services.AddScoped<IUserDomainService, EnhancedUserDomainService>();
            
            // Application Services (Clean Architecture - Application Layer)
            services.AddScoped<IUserSpecificationService, UserSpecificationService>();
            
            // Infrastructure Services (following Single Responsibility Principle)
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IApplicationSettings, ApplicationSettingsAdapter>();
            services.AddScoped<IMongoFilterBuilder, MongoFilterBuilder>();
            services.AddScoped<IMongoSortBuilder, MongoSortBuilder>();

            return services;
        }
    }
}
