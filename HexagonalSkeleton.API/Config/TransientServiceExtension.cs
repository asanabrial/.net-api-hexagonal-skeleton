using HexagonalSkeleton.API.Data;

namespace HexagonalSkeleton.API.Config
{
    public static class TransientServiceExtension
    {
        /// <summary>
        /// Extension method for IServiceCollection to register transient services for dependency injection.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Return the service collection for further configuration.</returns>
        public static IServiceCollection AddTransients(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
