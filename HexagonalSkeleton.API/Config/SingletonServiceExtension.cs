using HexagonalSkeleton.API.Handler.ExceptionMapping;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HexagonalSkeleton.API.Config
{
    public static class SingletonServiceExtension
    {
        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Exception mapping services - Using Singleton lifetime
            services.AddSingleton<ExceptionMappingService>();
            services.AddSingleton<IExceptionMapper, DomainExceptionMapper>();
            services.AddSingleton<IExceptionMapper, ApplicationExceptionMapper>();
            services.AddSingleton<IExceptionMapper, ValidationExceptionMapper>();
            
            // Exception mapper factory for Open/Closed Principle compliance
            services.AddSingleton<IExceptionMapperFactory, ExceptionMapperFactory>();
            
            return services;
        }
    }
}
