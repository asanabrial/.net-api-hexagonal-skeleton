using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure;
using HexagonalSkeleton.Infrastructure.Adapters;

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
            // Hexagonal Architecture Ports (CQRS pattern)
            services.AddScoped<IUserReadRepository, UserReadRepositoryAdapter>();
            services.AddScoped<IUserWriteRepository, UserWriteRepositoryAdapter>();
            
            // Domain Services (DDD)
            services.AddScoped<UserDomainService>();
            
            // Infrastructure Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IApplicationSettings, ApplicationSettingsAdapter>();

            return services;
        }
    }
}
