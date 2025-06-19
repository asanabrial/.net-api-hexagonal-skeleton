using HexagonalSkeleton.API.Mapping;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// AutoMapper service registration
    /// </summary>
    public static class AutoMapperServiceExtension
    {        /// <summary>
        /// Registers AutoMapper with minimal profile for special cases only
        /// </summary>
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                config.AllowNullDestinationValues = true;
                config.AllowNullCollections = true;
            }, typeof(ApiMappingProfile));
            
            return services;
        }
    }
}
