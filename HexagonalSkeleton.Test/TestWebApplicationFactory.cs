using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HexagonalSkeleton.API;
using System.Linq;
using HexagonalSkeleton.Infrastructure.Persistence;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Custom WebApplicationFactory for integration testing
    /// Completely overrides production database configuration with in-memory database
    /// Clean separation - tests handle their own infrastructure needs
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all production database services
                RemoveDbContextServices(services);
                
                // Configure test-specific in-memory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Ensure database is created for tests
                EnsureTestDatabaseCreated(services);
            });
        }

        /// <summary>
        /// Removes all production database-related services to avoid conflicts
        /// Includes both DbContext and internal EF Core services
        /// </summary>
        private static void RemoveDbContextServices(IServiceCollection services)
        {
            var descriptorsToRemove = services
                .Where(d => 
                    // DbContext services
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    // Generic DbContext services
                    (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                    // EF Core internal services that might cause conflicts
                    d.ServiceType.Name.Contains("DbContext") ||
                    d.ServiceType.Name.Contains("EntityFramework") ||
                    // MySQL provider services
                    d.ServiceType.FullName?.Contains("MySql") == true ||
                    d.ServiceType.FullName?.Contains("Pomelo") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        /// <summary>
        /// Ensures the test database is created and ready for use
        /// </summary>
        private static void EnsureTestDatabaseCreated(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }
    }
}
