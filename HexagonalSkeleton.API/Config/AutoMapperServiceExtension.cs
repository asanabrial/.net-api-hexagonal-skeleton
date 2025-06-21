using HexagonalSkeleton.API.Mapping;
using HexagonalSkeleton.Infrastructure.Mapping;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// AutoMapper service registration
    /// </summary>
    public static class AutoMapperServiceExtension
    {        /// <summary>
        /// Registers AutoMapper with all profiles and attribute-based mappings
        /// </summary>
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                // Configuración para mapeo automático más permisivo
                config.AllowNullDestinationValues = true;
                config.AllowNullCollections = true;

            }, 
            // Escanear múltiples assemblies para encontrar perfiles y atributos [AutoMap]
            typeof(ApiMappingProfile).Assembly,                    // API assembly
            typeof(InfrastructureMappingProfile).Assembly,         // Infrastructure assembly
            typeof(HexagonalSkeleton.Application.Command.LoginCommand).Assembly  // Application assembly
            );
            
            return services;
        }
    }
}
