using HexagonalSkeleton.API.Handler.ExceptionMapping;

namespace HexagonalSkeleton.API.Config
{
    public static class SingletonServiceExtension
    {
        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            // Exception mapping services - Using Singleton lifetime
            services.AddSingleton<ExceptionMappingService>();
            services.AddSingleton<IExceptionMapper, DomainExceptionMapper>();
            services.AddSingleton<IExceptionMapper, ApplicationExceptionMapper>();
            services.AddSingleton<IExceptionMapper, ValidationExceptionMapper>();
            services.AddSingleton<IExceptionMapper, InfrastructureExceptionMapper>();
            
            // Exception mapper factory for Open/Closed Principle compliance
            services.AddSingleton<IExceptionMapperFactory, ExceptionMapperFactory>();
            
            return services;
        }
    }
}
