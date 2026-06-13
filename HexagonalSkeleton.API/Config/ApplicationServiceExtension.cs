using System.Reflection;
using FluentValidation;
using HexagonalSkeleton.Application.Common.Messaging;

namespace HexagonalSkeleton.API.Config
{
    public static class ApplicationServiceExtension
    {
        
        public static IServiceCollection AddCqrsLayer(this IServiceCollection services)
        {
            // Auto-descubre assemblies que contengan handlers o validators
            var applicationAssemblies = GetApplicationAssemblies();

            services.AddRequestHandlers([.. applicationAssemblies]);
            services.AddValidatorsFromAssemblies(applicationAssemblies);

            return services;
        }

        private static IEnumerable<Assembly> GetApplicationAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                    // Solo assemblies de nuestro proyecto
                    assembly.FullName!.StartsWith("HexagonalSkeleton.Application", StringComparison.OrdinalIgnoreCase) &&
                    // Que contengan handlers o validators
                    (HasRequestHandlers(assembly) || HasFluentValidators(assembly)));
        }

        private static bool HasRequestHandlers(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Any(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    type.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));
            }
            catch
            {
                return false; // En caso de que no se pueda cargar el assembly
            }
        }

        private static bool HasFluentValidators(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Any(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    type.IsSubclassOf(typeof(AbstractValidator<>).GetGenericTypeDefinition()) ||
                    type.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IValidator<>)));
            }
            catch
            {
                return false;
            }
        }
    }
}
