using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using HexagonalSkeleton.Test.TestInfrastructure.Mocks;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AutoMapper;
using FluentValidation;
using MediatR;
using MassTransit;
using System;
using System.Threading.Tasks;
using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Configuración simplificada para tests con contenedores Docker.
    /// 
    /// CONCEPTOS CLAVE PARA JUNIORS:
    /// 1. Cada clase de test tiene sus propios contenedores Docker (PostgreSQL, MongoDB, RabbitMQ)
    /// 2. Los repositorios de lectura usan memoria compartida para sincronización CQRS inmediata
    /// 3. Los repositorios de escritura usan PostgreSQL real para validaciones de integridad
    /// 4. El mock de eventos sincroniza inmediatamente entre escritura y lectura
    /// </summary>
    public abstract class AbstractTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime, IDisposable
    {
        #region Configuración de contenedores

        protected abstract ITestContainerOrchestrator CreateContainerOrchestrator();
        private readonly ITestContainerOrchestrator _containerOrchestrator;
        protected ITestContainerOrchestrator ContainerOrchestrator => _containerOrchestrator;

        protected AbstractTestWebApplicationFactory()
        {
            _containerOrchestrator = CreateContainerOrchestrator();
        }

        /// <summary>
        /// Inicia todos los contenedores Docker al comenzar la clase de test
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            await _containerOrchestrator.StartAllAsync();
            await WaitForContainersToBeHealthy();
        }

        /// <summary>
        /// Limpia todos los contenedores Docker al terminar la clase de test
        /// </summary>
        public new virtual async Task DisposeAsync()
        {
            await _containerOrchestrator.DisposeAsync();
            await base.DisposeAsync();
        }

        /// <summary>
        /// Limpieza síncrona para compatibilidad con IDisposable
        /// </summary>
        public new void Dispose()
        {
            _containerOrchestrator.DisposeAsync().GetAwaiter().GetResult();
            base.Dispose();
        }

        private async Task WaitForContainersToBeHealthy()
        {
            const int maxRetries = 30;
            for (int i = 0; i < maxRetries; i++)
            {
                if (await _containerOrchestrator.AreAllHealthyAsync())
                    return;

                await Task.Delay(1000);
            }
            
            throw new InvalidOperationException("Los contenedores Docker no pudieron iniciarse correctamente");
        }

        #endregion

        #region Configuración de la aplicación

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            
            builder.ConfigureServices(services =>
            {
                // 1. Remover servicios de producción que causan conflictos
                RemoveProductionServices(services);
                
                // 2. Configurar bases de datos de test
                ConfigureTestDatabases(services);
                
                // 3. Configurar repositorios para tests
                ConfigureTestRepositories(services);
                
                // 4. Configurar servicios básicos
                ConfigureTestServices(services);
                
                // 5. Asegurar que la base de datos existe
                EnsureDatabaseExists(services);
            });

            ConfigureTestLogging(builder);
        }

        #endregion

        #region Métodos de configuración (privados para simplicidad)

        /// <summary>
        /// Remueve servicios de producción que causan problemas en tests
        /// </summary>
        private static void RemoveProductionServices(IServiceCollection services)
        {
            // Remover servicios de base de datos de producción
            var dbServices = services.Where(s => 
                s.ServiceType == typeof(CommandDbContext) ||
                s.ServiceType.Name.Contains("DbContext") ||
                s.ServiceType.Name.Contains("MassTransit") ||
                s.ServiceType.Name.Contains("Consumer") ||
                s.ImplementationType?.Name.Contains("Repository") == true)
                .ToList();

            foreach (var service in dbServices)
            {
                services.Remove(service);
            }
        }

        /// <summary>
        /// Configura las conexiones a las bases de datos de test (contenedores Docker)
        /// </summary>
        private void ConfigureTestDatabases(IServiceCollection services)
        {
            // PostgreSQL para escritura (comandos)
            services.AddDbContext<CommandDbContext>(options =>
            {
                options.UseNpgsql(ContainerOrchestrator.PostgreSql.ConnectionString);
                options.EnableSensitiveDataLogging(); // Para debug en tests
            });

            // MongoDB para lectura (queries) - aunque usamos memoria compartida
            services.AddSingleton<IMongoClient>(_ => 
                new MongoClient(ContainerOrchestrator.MongoDb.ConnectionString));
        }

        /// <summary>
        /// Configura los repositorios específicos para tests
        /// </summary>
        private static void ConfigureTestRepositories(IServiceCollection services)
        {
            // Repositorio de escritura: usa PostgreSQL real para validaciones de integridad
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // Repositorio de lectura: usa memoria compartida para sincronización inmediata
            services.AddSingleton<IUserReadRepository, InMemoryUserReadRepository>();
            
            // Servicio de eventos: sincronización inmediata entre escritura y lectura
            services.AddScoped<IIntegrationEventService, MockIntegrationEventService>();
        }

        /// <summary>
        /// Configura servicios necesarios para los tests
        /// </summary>
        private static void ConfigureTestServices(IServiceCollection services)
        {
            // MediatR para CQRS
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
                typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly));

            // AutoMapper para mapeos
            services.AddAutoMapper(
                typeof(HexagonalSkeleton.API.Mapping.ApiMappingProfile),
                typeof(HexagonalSkeleton.Application.Mapping.ApplicationMappingProfile));

            // Validadores
            services.AddValidatorsFromAssembly(
                typeof(HexagonalSkeleton.Application.Features.UserProfile.Commands.UpdateProfileUserCommand).Assembly);

            // Autenticación de test
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", _ => { });

            services.AddAuthorization();
            services.AddHttpContextAccessor();
            services.AddControllers();
        }

        /// <summary>
        /// Asegura que la base de datos existe y está lista para usar
        /// </summary>
        private static void EnsureDatabaseExists(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Configura logging mínimo para tests
        /// </summary>
        protected virtual void ConfigureTestLogging(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
        }

        #endregion
    }
}
