using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Infrastructure;

namespace HexagonalSkeleton.API.Config
{
    public static class ScopeServiceExtension
    {
        /// <summary>
        /// Extension method for IServiceCollection to register application services for dependency injection.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddScopes(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
