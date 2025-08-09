using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.Domain.Ports;
using AutoMapper;
using MediatR;

namespace HexagonalSkeleton.Test
{
    public class DITest
    {
        public static void RunTest()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("../HexagonalSkeleton.API/appsettings.json", optional: false)
                .AddJsonFile("../HexagonalSkeleton.API/appsettings.Development.json", optional: true)
                .Build();

            var services = new ServiceCollection();

            // Configure logging
            services.AddLogging();

            // Configure AutoMapper with all profiles
            services.AddAutoMapper(
                typeof(HexagonalSkeleton.API.Config.ApplicationServiceExtension),
                typeof(HexagonalSkeleton.Infrastructure.Mapping.InfrastructureMappingProfile)
            );

            // Configure MediatR (only for Commands/Queries, not for domain events)
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(HexagonalSkeleton.API.Config.ApplicationServiceExtension).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(HexagonalSkeleton.Application.Features.UserRegistration.Commands.RegisterUserCommand).Assembly);
            });

            // Configure CQRS databases
            services.AddCqrsDatabases(configuration);

            // Configure CQRS services
            services.AddCqrsServices();

            try 
            {
                Console.WriteLine("Building service provider...");
                var serviceProvider = services.BuildServiceProvider();
                
                Console.WriteLine("Testing IUserReadRepository resolution...");
                var readRepository = serviceProvider.GetRequiredService<IUserReadRepository>();
                Console.WriteLine("✓ IUserReadRepository resolved successfully");
                
                Console.WriteLine("Testing IUserWriteRepository resolution...");
                var writeRepository = serviceProvider.GetRequiredService<IUserWriteRepository>();
                Console.WriteLine("✓ IUserWriteRepository resolved successfully");
                
                Console.WriteLine("✅ All dependency injection tests passed!");
                Console.WriteLine("The CQRS dependencies are correctly configured.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dependency injection test failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
