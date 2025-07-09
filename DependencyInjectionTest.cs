using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.Domain.Ports;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Simple test to verify dependency injection configuration is correct
    /// Tests that all required services can be resolved without circular dependencies
    /// </summary>
    public class DependencyInjectionTest
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Testing dependency injection configuration...");

                // Set up configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

                // Set up service collection
                var services = new ServiceCollection();
                
                // Configure logging
                services.AddLogging();
                
                // Configure AutoMapper
                services.AddAutoMapper(typeof(Program).Assembly);
                
                // Configure MediatR
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
                
                // Configure CQRS databases
                services.AddCommandDatabase(configuration);
                services.AddQueryDatabase(configuration);
                
                // Configure CQRS services
                services.AddCqrsServices();
                
                // Build service provider
                var serviceProvider = services.BuildServiceProvider();
                
                // Test that key services can be resolved
                Console.WriteLine("Testing IUserReadRepository resolution...");
                var readRepository = serviceProvider.GetRequiredService<IUserReadRepository>();
                Console.WriteLine("✓ IUserReadRepository resolved successfully");
                
                Console.WriteLine("Testing IUserWriteRepository resolution...");
                var writeRepository = serviceProvider.GetRequiredService<IUserWriteRepository>();
                Console.WriteLine("✓ IUserWriteRepository resolved successfully");
                
                Console.WriteLine("✓ All dependency injection tests passed!");
                Console.WriteLine("The CQRS dependency injection has been configured correctly.");
                
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dependency injection test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                Environment.Exit(1);
            }
        }
    }
}
