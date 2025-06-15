using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HexagonalSkeleton.API.Config
{
    public static class SingletonServiceExtension
    {
        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }
}
