using HexagonalSkeleton.API.Mapping;
using HexagonalSkeleton.Infrastructure.Mapping;
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
            // Scan multiple assemblies for profiles and [AutoMap] attributes
            typeof(ApiMappingProfile).Assembly,                    // API assembly
            typeof(InfrastructureMappingProfile).Assembly,         // Infrastructure assembly  
            typeof(CqrsMappingProfile).Assembly,                   // CQRS mapping profile
            typeof(ApplicationMappingProfile).Assembly             // Application assembly (for [AutoMap] attributes)
            );
            
            return services;
        }
    }
}
