using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HexagonalSkeleton.API;
using System.Linq;
using HexagonalSkeleton.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using AutoMapper;
using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;

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

                // Add FluentValidation for tests (since it's missing from production config)
                services.AddValidatorsFromAssembly(
                    typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly);

                // Override MediatR configuration to include Application assembly
                services.RemoveAll(typeof(MediatR.IMediator));
                services.RemoveAll(typeof(MediatR.ISender));
                services.RemoveAll(typeof(MediatR.IPublisher));
                
                services.AddMediatR(cfg => {
                    cfg.RegisterServicesFromAssemblies(
                        typeof(Program).Assembly,
                        typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly
                    );
                });

                // Add test-specific AutoMapper configuration
                services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<HexagonalSkeleton.API.Mapping.ApiMappingProfile>();
                    cfg.AddProfile<TestMappingProfile>();
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

    /// <summary>
    /// AutoMapper profile specifically for tests
    /// Includes mappings missing from production that tests need
    /// </summary>
    public class TestMappingProfile : Profile
    {
        public TestMappingProfile()
        {
            // Mapping para el registro de usuarios que falta en producción
            CreateMap<RegisterUserDto, LoginResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.AccessToken))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => src.TokenType))
                .ForMember(dest => dest.ExpiresIn, opt => opt.MapFrom(src => src.ExpiresIn))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        }
    }
}
