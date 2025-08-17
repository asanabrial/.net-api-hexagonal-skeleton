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
        /// Configures domain and application services following Hexagonal Architecture principles.
        /// Registers ports and adapters with proper separation of concerns and dependency inversion.
        /// Follows naming convention focused on business domain rather than technical concerns.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Domain Services (DDD)
            services.AddScoped<UserDomainService>();
            
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
