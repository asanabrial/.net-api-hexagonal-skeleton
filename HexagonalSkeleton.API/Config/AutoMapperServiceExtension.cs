using HexagonalSkeleton.API.Mapping;
using HexagonalSkeleton.Application.Mapping;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// AutoMapper service registration with CQRS support
    /// Includes mappings for command/query separation
    /// </summary>
    public static class AutoMapperServiceExtension
    {
        /// <summary>
        /// Registers AutoMapper with all profiles including CQRS mappings
        /// </summary>
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                // Configuration for more permissive automatic mapping
                config.AllowNullDestinationValues = true;
                config.AllowNullCollections = true;
            },
            // Scan remaining assemblies for profiles and [AutoMap] attributes
            typeof(ApiMappingProfile).Assembly,                    // API assembly
            typeof(ApplicationMappingProfile).Assembly             // Application assembly (for [AutoMap] attributes)
            );
            
            return services;
        }
    }
}
