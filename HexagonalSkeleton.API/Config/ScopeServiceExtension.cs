using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure;
using HexagonalSkeleton.Infrastructure.Adapters;

namespace HexagonalSkeleton.API.Config
{
    public static class ScopeServiceExtension
    {        /// <summary>
        /// Extension method for IServiceCollection to register application services for dependency injection.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            // Hexagonal Architecture Ports (CQRS)
            services.AddScoped<IUserReadRepository, UserReadRepositoryAdapter>();
            services.AddScoped<IUserWriteRepository, UserWriteRepositoryAdapter>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IApplicationSettings, ApplicationSettingsAdapter>();

            return services;
        }
    }
}
