using Xunit;
using Testcontainers.PostgreSql;
using Testcontainers.MongoDb;
using Testcontainers.Kafka;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;

namespace HexagonalSkeleton.Test.Integration.Infrastructure
{
    /// <summary>
    /// Base class para tests de integración REALES con Testcontainers
    /// No complex abstractions, just direct containers with shared network
    /// </summary>
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly INetwork _network;
        protected readonly PostgreSqlContainer _postgres;
        protected readonly MongoDbContainer _mongodb;
        protected readonly KafkaContainer _kafka;
        protected ServiceProvider? _serviceProvider;
        private readonly TestContainersOptions _options;

        protected BaseIntegrationTest()
        {
            Console.WriteLine("Configuring containers for integration...");
            
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            
            _options = new TestContainersOptions();
            configuration.GetSection(TestContainersOptions.SectionName).Bind(_options);
            
            var dockerConfig = new DockerConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:Docker").Bind(dockerConfig);
            
            // Create red compartida para comunicación entre contenedores
            _network = new NetworkBuilder()
                .WithName($"integration-network-{Guid.NewGuid():N}")
                .Build();
            
            _postgres = new PostgreSqlBuilder()
                .WithImage(dockerConfig.Images.PostgreSQL)
                .WithDatabase(_options.Database)
                .WithUsername(_options.Username)
                .WithPassword(_options.Password)
                .WithNetwork(_network)
                .WithNetworkAliases(dockerConfig.NetworkAliases.PostgreSQL)
                .Build();

            _mongodb = new MongoDbBuilder()
                .WithImage(dockerConfig.Images.MongoDB)
                .WithNetwork(_network)
                .WithNetworkAliases(dockerConfig.NetworkAliases.MongoDB)
                .Build();
                
            _kafka = new KafkaBuilder()
                .WithImage(dockerConfig.Images.Kafka)
                .WithNetwork(_network)
                .WithNetworkAliases(dockerConfig.NetworkAliases.Kafka)
                .Build();
        }

        public virtual async Task InitializeAsync()
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine("Starting network and containers...");
            
            // First create the network
            await _network.CreateAsync();
            Console.WriteLine(" Red Docker creada");
            
            // Luego iniciar contenedores básicos en la red compartida
            await Task.WhenAll(
                _postgres.StartAsync(),
                _mongodb.StartAsync(),
                _kafka.StartAsync()
            );
            
            var elapsed = DateTime.UtcNow - startTime;
            Console.WriteLine($" Contenedores listos en {elapsed.TotalSeconds:F1}s");
            
            await ConfigureServicesAsync();
        }

        protected virtual async Task ConfigureServicesAsync()
        {
            var services = new ServiceCollection();
            
            // Real configuration with containers
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"ConnectionStrings:CommandDatabase", _postgres.GetConnectionString()},
                    {"ConnectionStrings:HexagonalSkeletonRead", _mongodb.GetConnectionString()},
                    {"MongoDb:DatabaseName", "hexagonal_test"},
                    {"Kafka:BootstrapServers", _kafka.GetBootstrapAddress()}
                })
                .Build();
                
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            
            // PostgreSQL real para comandos
            services.AddDbContext<CommandDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
            
            // MongoDB real para queries
            services.AddScoped<QueryDbContext>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new QueryDbContext(config);
            });
            
            // Register test helpers for CDC integration tests
            services.AddScoped<HexagonalSkeleton.Test.TestInfrastructure.Helpers.MongoDbSyncHelper>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Aplicar migraciones reales
            using var scope = _serviceProvider.CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            await commandDb.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Obtiene el DbContext de comandos (PostgreSQL)
        /// </summary>
        protected CommandDbContext GetCommandDbContext()
        {
            return _serviceProvider!.GetRequiredService<CommandDbContext>();
        }

        /// <summary>
        /// Obtiene el DbContext de queries (MongoDB)
        /// </summary>
        protected QueryDbContext GetQueryDbContext()
        {
            return _serviceProvider!.GetRequiredService<QueryDbContext>();
        }

        /// <summary>
        /// Obtiene un scope de servicios
        /// </summary>
        protected IServiceScope CreateScope()
        {
            return _serviceProvider!.CreateScope();
        }

        /// <summary>
        /// Real connection strings from containers
        /// </summary>
        protected string PostgreSqlConnectionString => _postgres.GetConnectionString();
        protected string MongoDbConnectionString => _mongodb.GetConnectionString();
        protected string KafkaBootstrapServers => _kafka.GetBootstrapAddress();

        public virtual async Task DisposeAsync()
        {
            Console.WriteLine("🧹 Limpiando contenedores...");
            
            _serviceProvider?.Dispose();
            
            // Cleanup optimizado en paralelo para máximo rendimiento
            var disposeTasks = new List<Task>();
            
            // Core containers (siempre presentes)
            disposeTasks.Add(_postgres.DisposeAsync().AsTask());
            disposeTasks.Add(_mongodb.DisposeAsync().AsTask());
            disposeTasks.Add(_kafka.DisposeAsync().AsTask());
            
            // Ejecutar cleanup en paralelo con timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                await Task.WhenAll(disposeTasks).WaitAsync(cts.Token);
                Console.WriteLine(" Contenedores eliminados");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(" Timeout en cleanup - forzando limpieza");
            }
            
            // Limpiar la red al final
            try
            {
                await _network.DisposeAsync();
                Console.WriteLine(" Red Docker eliminada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error limpiando red: {ex.Message}");
            }
            
            Console.WriteLine(" Limpieza completa");
        }
    }
}
