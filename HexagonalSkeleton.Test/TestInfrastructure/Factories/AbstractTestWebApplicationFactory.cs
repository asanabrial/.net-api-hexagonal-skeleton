using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Services;
using HexagonalSkeleton.Infrastructure.Adapters.Command;
using HexagonalSkeleton.Infrastructure.Adapters.Query;
using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Services;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Threading.Tasks;
using HexagonalSkeleton.Domain.Ports;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Simplified configuration for tests with Docker containers.
    /// 
    /// KEY CONCEPTS:
    /// 1. Each test class has its own Docker containers (PostgreSQL, MongoDB, Kafka)
    /// 2. Read repositories use shared memory for immediate CQRS synchronization
    /// 3. Write repositories use real PostgreSQL for integrity validations
    /// 4. Event handling synchronizes immediately between write and read models
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
        /// Starts all Docker containers when test class begins
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            await _containerOrchestrator.StartAllAsync();
            await WaitForContainersToBeHealthy();
            
            // Configure Debezium connector for testing
            await ConfigureDebeziumConnectorAsync();
        }

        /// <summary>
        /// Cleans up all Docker containers when test class finishes
        /// </summary>
        public new virtual async Task DisposeAsync()
        {
            await _containerOrchestrator.DisposeAsync();
            await base.DisposeAsync();
        }

        /// <summary>
        /// Synchronous cleanup for IDisposable compatibility
        /// </summary>
        public new void Dispose()
        {
            _containerOrchestrator.DisposeAsync().GetAwaiter().GetResult();
            base.Dispose();
        }

        private async Task WaitForContainersToBeHealthy()
        {
            const int maxRetries = 20; // More retries but shorter intervals
            const int delayMs = 500; // Reduced from 1000ms to 500ms
            
            for (int i = 0; i < maxRetries; i++)
            {
                if (await _containerOrchestrator.AreAllHealthyAsync())
                    return;

                await Task.Delay(delayMs);
            }
            
            throw new InvalidOperationException($"Docker containers could not start correctly after {maxRetries} attempts");
        }

        /// <summary>
        /// Configures Debezium connector for test containers to enable CDC
        /// </summary>
        private async Task ConfigureDebeziumConnectorAsync()
        {
            // Configure real Debezium Connect connector for testing
            try
            {
                var connectorName = "hexagonal-postgres-connector-test";
                await ContainerOrchestrator.DebeziumConnect.ConfigurePostgreSqlConnectorAsync(
                    connectorName, 
                    ContainerOrchestrator.PostgreSql.ConnectionString);
                
                Console.WriteLine("✅ Debezium Connect configurado para tests CDC");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Failed to configure Debezium connector: {ex.Message}");
                // Continue with tests - TestCdcEventPublisher will be used as fallback
            }
        }

        #endregion

        #region Configuración de la aplicación

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            
            // Configure test-specific settings
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Generate unique GroupId for test run to ensure clean kafka consumer state
                var uniqueGroupId = $"hexagonal-test-{Guid.NewGuid():N}";
                
                // Configure container ports dynamically
                var connectionStrings = new Dictionary<string, string?>
                {
                    {"ConnectionStrings:CommandDatabase", ContainerOrchestrator.PostgreSql.ConnectionString},
                    {"ConnectionStrings:QueryDatabase", ContainerOrchestrator.MongoDb.ConnectionString},
                    {"Debezium:Producer:BootstrapServers", ContainerOrchestrator.Kafka.BootstrapServers},
                    {"Debezium:Consumer:BootstrapServers", ContainerOrchestrator.Kafka.BootstrapServers},
                    {"Debezium:Consumer:GroupId", uniqueGroupId} // Unique group ID per test run
                };

                // Only configure Debezium Connect if available
                if (ContainerOrchestrator.IsDebeziumConnectAvailable)
                {
                    connectionStrings["Debezium:Connect:Url"] = ContainerOrchestrator.DebeziumConnect.ConnectUrl;
                }
                
                config.AddInMemoryCollection(connectionStrings);
            });
            
            builder.ConfigureServices(services =>
            {
                // 1. Remove production services that cause conflicts
                RemoveProductionServices(services);
                
                // 2. Configure test databases
                ConfigureTestDatabases(services);
                
                // 3. Configure repositories for tests
                ConfigureTestRepositories(services);
                
                // 4. Configure basic services
                ConfigureTestServices(services);
                
                // 5. Configure Test CDC Event Publisher to simulate Debezium
                ConfigureTestCdcServices(services);
                
                // 6. Ensure database exists
                EnsureDatabaseExists(services);
            });

            ConfigureTestLogging(builder);
        }

        #endregion

        #region Configuration methods (private for simplicity)

        /// <summary>
        /// Removes production services that cause problems in tests
        /// </summary>
        private static void RemoveProductionServices(IServiceCollection services)
        {
            // Remove production database services
            var dbServices = services.Where(s => 
                s.ServiceType == typeof(CommandDbContext) ||
                s.ServiceType.Name.Contains("DbContext") ||
                s.ServiceType.Name.Contains("MassTransit") ||
                (s.ServiceType.Name.Contains("Consumer") && !s.ServiceType.Name.Contains("DebeziumConsumer")) ||
                s.ImplementationType?.Name.Contains("Repository") == true)
                .ToList();

            foreach (var service in dbServices)
            {
                services.Remove(service);
            }
        }

        /// <summary>
        /// Configures database connections for tests (Docker containers)
        /// </summary>
        private void ConfigureTestDatabases(IServiceCollection services)
        {
            // PostgreSQL para escritura (comandos)
            services.AddDbContext<CommandDbContext>(options =>
            {
                options.UseNpgsql(ContainerOrchestrator.PostgreSql.ConnectionString);
                options.EnableSensitiveDataLogging(); // For debugging in tests
            });

            // MongoDB for reading (queries) - complete configuration for CDC
            services.AddSingleton<IMongoClient>(_ => 
                new MongoClient(ContainerOrchestrator.MongoDb.ConnectionString));
            
            // IMongoDatabase for direct access (needed for TestCdcSynchronizer)
            services.AddScoped<IMongoDatabase>(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase("hexagonal_test"));
            
            // QueryDbContext for MongoDB
            services.AddScoped<QueryDbContext>(sp => 
                new QueryDbContext(sp.GetRequiredService<IMongoClient>(), "hexagonal_test"));
            
            // Auxiliary services for MongoDB
            services.AddScoped<IMongoFilterBuilder, MongoFilterBuilder>();
            services.AddScoped<IMongoSortBuilder, MongoSortBuilder>();
        }

        /// <summary>
        /// Configures test-specific repositories
        /// </summary>
        private static void ConfigureTestRepositories(IServiceCollection services)
        {
            // For real integration tests with CDC: use complete CQRS architecture
            // Write repository: uses PostgreSQL 
            services.AddScoped<IUserWriteRepository, UserCommandRepository>();
            
            // Read repository: uses MongoDB with real CDC synchronization
            services.AddScoped<IUserReadRepository, UserReadRepositoryMongoAdapter>();
            
            // Enable CDC services for complete integration tests
            // CDC will automatically synchronize between PostgreSQL (write) and MongoDB (read)
        }

        /// <summary>
        /// Configures services needed for tests
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

            // Test authentication
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", _ => { });

            services.AddAuthorization();
            services.AddHttpContextAccessor();
            services.AddControllers();
        }

        /// <summary>
        /// Configures Test CDC services to simulate Debezium events for testing
        /// CDC is always enabled in this CQRS architecture
        /// </summary>
        private void ConfigureTestCdcServices(IServiceCollection services)
        {
            // CDC is always enabled in this CQRS architecture
            Console.WriteLine("✅ CDC enabled for integration tests");
            
            // Do NOT configure ProducerConfig here - use the one from CdcServiceExtension.cs
            // which gets the correct configuration from in-memory config overrides
            
            // Register TestCdcEventPublisher - it will use the ProducerConfig from CdcServiceExtension.cs
            services.AddSingleton<TestInfrastructure.Services.TestCdcEventPublisher>();
            
            // Register CDC Synchronization Helper for elegant test synchronization without delays
            services.AddSingleton<TestInfrastructure.Services.CdcSynchronizationHelper>();
            
            // Register Test Debezium Event Processor Decorator for CDC synchronization
            services.AddScoped<TestInfrastructure.Services.TestDebeziumEventProcessorDecorator>();
            
            // Register convenient CDC Test Helper for easy test usage
            services.AddScoped<TestInfrastructure.Helpers.CdcTestHelper>();
            
            // Register MongoDB Sync Helper - Simple polling-based synchronization (more reliable)
            services.AddScoped<TestInfrastructure.Helpers.MongoDbSyncHelper>();
        }

        /// <summary>
        /// Ensures that the database exists and is ready for use
        /// </summary>
        private static void EnsureDatabaseExists(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Configures minimal logging for tests
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
