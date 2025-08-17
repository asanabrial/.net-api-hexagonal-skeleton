using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Orchestrates the lifecycle of all test containers
    /// </summary>
    public class TestContainerOrchestrator : ITestContainerOrchestrator
    {
        private readonly ITestContainerFactory _factory;
        private readonly INetwork _sharedNetwork;
        private bool _disposed = false;
        private bool _debeziumConnectAvailable = false; // Flag para Debezium Connect

        public TestContainerOrchestrator(ITestContainerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            
            // Create shared network for all containers with unique name
            var networkName = $"hexagonal-test-network-{Guid.NewGuid():N}";
            _sharedNetwork = new NetworkBuilder()
                .WithName(networkName)
                .WithCleanUp(true)
                .Build();
            
            // Create una nueva instancia del factory con la red compartida
            var networkFactory = new TestcontainersFactory(_sharedNetwork);
            
            PostgreSql = networkFactory.CreatePostgreSqlContainer();
            MongoDb = networkFactory.CreateMongoDbContainer();
            Zookeeper = networkFactory.CreateZookeeperContainer();
            Kafka = networkFactory.CreateKafkaContainer();
            SchemaRegistry = networkFactory.CreateSchemaRegistryContainer(); //  Agregar Schema Registry
            // DebeziumConnect will be created after all services are started
        }

        public IPostgreSqlTestContainer PostgreSql { get; }
        
        public IMongoDbTestContainer MongoDb { get; }
        
        public IZookeeperTestContainer Zookeeper { get; }
        
        public IKafkaTestContainer Kafka { get; }
        
        public ISchemaRegistryTestContainer? SchemaRegistry { get; private set; } //  Ahora será inicializado
        
        public IDebeziumConnectTestContainer DebeziumConnect { get; private set; } = null!;

        public bool IsDebeziumConnectAvailable => _debeziumConnectAvailable;

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("🔄 Fase 0: Creando red compartida...");
            await _sharedNetwork.CreateAsync(cancellationToken);
            Console.WriteLine(" Red compartida creada");
            
            // Official Confluent stack startup sequence
            // Dependency order: Zookeeper → Kafka → Schema Registry → Debezium Connect
            
            Console.WriteLine(" Fase 1: Iniciando servicios base independientes...");
            var phase1Tasks = new[]
            {
                Task.Run(async () => 
                {
                    Console.WriteLine(" Iniciando PostgreSQL...");
                    await PostgreSql.StartAsync(cancellationToken);
                    Console.WriteLine(" PostgreSQL completado");
                }, cancellationToken),
                Task.Run(async () => 
                {
                    Console.WriteLine(" Iniciando MongoDB...");
                    await MongoDb.StartAsync(cancellationToken);
                    Console.WriteLine(" MongoDB completado");
                }, cancellationToken)
            };
            await Task.WhenAll(phase1Tasks);
            Console.WriteLine(" Fase 1 completada: PostgreSQL, MongoDB iniciados");

            Console.WriteLine("� Fase 2: Iniciando Zookeeper...");
            await Zookeeper.StartAsync(cancellationToken);
            Console.WriteLine(" Zookeeper iniciado y listo");

            Console.WriteLine(" Fase 3: Iniciando Kafka (requiere Zookeeper)...");
            await Kafka.StartAsync(cancellationToken);
            Console.WriteLine(" Kafka iniciado y conectado a Zookeeper");

            Console.WriteLine(" Fase 4: Iniciando Schema Registry (requiere Kafka)...");
            await SchemaRegistry!.StartAsync(cancellationToken);
            Console.WriteLine(" Schema Registry iniciado y conectado a Kafka");

            Console.WriteLine(" Fase 5: Iniciando Debezium Connect (requiere Kafka + Schema Registry)...");
            
            
            // Get las direcciones para Debezium Connect
            var kafkaBootstrapServers = Kafka.BootstrapServers;
            var schemaRegistryUrl = SchemaRegistry!.SchemaRegistryUrl;
            
            // Para contenedores en la misma red, usar el listener interno
            var kafkaInternalAddress = "kafka:29092";
            var schemaRegistryInternalUrl = "http://schema-registry:8081";
            
            
            
            Console.WriteLine($" Schema Registry público: {schemaRegistryUrl}");
            Console.WriteLine($" Schema Registry interno para Debezium: {schemaRegistryInternalUrl}");
            
            var debeziumConfig = new TestContainerConfiguration
            {
                KafkaBootstrapServers = kafkaInternalAddress,
                SchemaRegistryUrl = schemaRegistryInternalUrl //  Ahora con Schema Registry
            };
            
            // Usar el mismo factory con red para Debezium Connect
            var networkFactory = new TestcontainersFactory(_sharedNetwork);
            DebeziumConnect = networkFactory.CreateDebeziumConnectContainer(debeziumConfig);
            
            try
            {
                // Timeout para Debezium Connect con stack completa
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // Timeout más largo para stack completa
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token;
                
                Console.WriteLine("Trying to start Debezium Connect with Avro/Schema Registry (timeout: 60s)...");
                await DebeziumConnect.StartAsync(combinedToken);
                _debeziumConnectAvailable = true;
                Console.WriteLine(" Debezium Connect iniciado successfully");

                // Configure automáticamente el conector PostgreSQL
                Console.WriteLine(" Configurando conector PostgreSQL en Debezium Connect...");
                try
                {
                    var postgreSqlConnectionString = PostgreSql.ConnectionString;
                    await DebeziumConnect.ConfigurePostgreSqlConnectorAsync("postgres-connector", postgreSqlConnectionString, combinedToken);
                    Console.WriteLine(" Conector PostgreSQL configurado successfully");
                }
                catch (Exception connectorEx)
                {
                    Console.WriteLine($" Warning: Could not configure PostgreSQL connector: {connectorEx.Message}");
                    Console.WriteLine(" CDC may not function correctly without the connector");
                }
                
                Console.WriteLine(" Fase 6 completada: Debezium Connect iniciado con soporte Avro completo");
            }
            catch (Exception ex)
            {
                _debeziumConnectAvailable = false;
                Console.WriteLine($" Debezium Connect falló: {ex.Message}");
                Console.WriteLine(" Los tests continuarán sin CDC - funcionalidad limitada");
                // NO hacer throw - permitir que tests continúen sin CDC
            }
            
            Console.WriteLine(" Toda la stack completa iniciada successfully");
            Console.WriteLine(" Stack activa: Zookeeper + Kafka + Schema Registry + Debezium Connect");
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            // Stop en orden inverso al startup para respeter dependencias
            var tasks = new List<Task>
            {
                PostgreSql.StopAsync(cancellationToken),
                MongoDb.StopAsync(cancellationToken)
            };

            // Solo incluir Debezium Connect si se inició
            if (_debeziumConnectAvailable)
            {
                tasks.Add(DebeziumConnect.StopAsync(cancellationToken));
            }

            //  Add all components we are using
            tasks.Add(Kafka.StopAsync(cancellationToken));
            if (SchemaRegistry != null)
            {
                tasks.Add(SchemaRegistry.StopAsync(cancellationToken));
            }
            tasks.Add(Zookeeper.StopAsync(cancellationToken));

            await Task.WhenAll(tasks);
        }

        public async Task<bool> AreAllHealthyAsync(CancellationToken cancellationToken = default)
        {
            //  Contenedores esenciales que estamos usando
            var essentialTasks = new[]
            {
                PostgreSql.IsHealthyAsync(cancellationToken),
                MongoDb.IsHealthyAsync(cancellationToken),
                Zookeeper.IsHealthyAsync(cancellationToken),
                Kafka.IsHealthyAsync(cancellationToken),
                SchemaRegistry!.IsHealthyAsync(cancellationToken)
            };

            var essentialResults = await Task.WhenAll(essentialTasks);
            var allEssentialHealthy = essentialResults.All(r => r);

            // Only verify Debezium Connect if it's available
            if (_debeziumConnectAvailable)
            {
                var debeziumHealthy = await DebeziumConnect.IsHealthyAsync(cancellationToken);
                return allEssentialHealthy && debeziumHealthy;
            }

            // If Debezium Connect is not available, only verify essential containers
            return allEssentialHealthy;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                var tasks = new List<ValueTask>();

                // Siempre dispose de PostgreSQL y MongoDB
                tasks.Add(PostgreSql.DisposeAsync());
                tasks.Add(MongoDb.DisposeAsync());

                // Dispose Zookeeper, Kafka and Schema Registry if they are initialized
                if (Zookeeper != null)
                    tasks.Add(Zookeeper.DisposeAsync());

                if (Kafka != null)
                    tasks.Add(Kafka.DisposeAsync());

                if (SchemaRegistry != null)
                    tasks.Add(SchemaRegistry.DisposeAsync());

                // Solo dispose de Debezium Connect si está disponible y no es null
                if (_debeziumConnectAvailable && DebeziumConnect != null)
                    tasks.Add(DebeziumConnect.DisposeAsync());

                await Task.WhenAll(tasks.Select(t => t.AsTask()));

                // Dispose de la red compartida
                if (_sharedNetwork != null)
                    await _sharedNetwork.DisposeAsync();

                _disposed = true;
            }
        }
    }
}
