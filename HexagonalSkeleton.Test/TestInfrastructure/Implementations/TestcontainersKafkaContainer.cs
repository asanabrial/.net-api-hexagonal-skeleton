using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer para Kafka oficial de Confluent con Zookeeper
    /// </summary>
    public class TestcontainersKafkaContainer : IKafkaTestContainer
    {
        private readonly IContainer _container;
        private bool _disposed = false;
        private readonly int _port = 9092;

        public TestcontainersKafkaContainer(
            string image = "confluentinc/cp-kafka:6.2.0", // ✅ Usar versión anterior que no requiere KAFKA_PROCESS_ROLES
            INetwork? network = null)
        {
            Console.WriteLine($"🔧 Configurando Kafka con imagen: {image}");
            
            var builder = new ContainerBuilder()
                .WithImage(image)
                .WithPortBinding(9092, 9092) // ✅ Puerto 9092:9092 como en el ejemplo
                // Traditional mode with Zookeeper (not KRaft)
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", "zookeeper:2181")
                // Dual listeners: INTERNAL for containers, EXTERNAL for host
                .WithEnvironment("KAFKA_LISTENERS", "INTERNAL://0.0.0.0:29092,EXTERNAL://0.0.0.0:9092")
                .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "INTERNAL://kafka:29092,EXTERNAL://localhost:9092")
                .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "INTERNAL:PLAINTEXT,EXTERNAL:PLAINTEXT")
                .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "INTERNAL")
                .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
                .WithEnvironment("KAFKA_LOG_CLEANER_DELETE_RETENTION_MS", "5000")
                .WithEnvironment("KAFKA_BROKER_ID", "1")
                .WithEnvironment("KAFKA_MIN_INSYNC_REPLICAS", "1")
                .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
                .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
                .WithEnvironment("KAFKA_ZOOKEEPER_SESSION_TIMEOUT_MS", "30000")
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECTION_TIMEOUT_MS", "20000")
                .WithEnvironment("KAFKA_SOCKET_SEND_BUFFER_BYTES", "102400")
                .WithEnvironment("KAFKA_SOCKET_RECEIVE_BUFFER_BYTES", "102400")
                .WithEnvironment("KAFKA_REQUEST_TIMEOUT_MS", "30000")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable(9092))
                .WithCleanUp(true);
                
            if (network != null)
            {
                Console.WriteLine($"🌐 Kafka usando red compartida con alias 'kafka'");
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases("kafka");
            }
                
            _container = builder.Build();
        }

        public string BootstrapServers => "localhost:9092"; // ✅ Puerto fijo como en el ejemplo

        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _container.GetMappedPublicPort(_port);

        public string IpAddress => _container.IpAddress;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"🚀 Iniciando contenedor Kafka...");
            await _container.StartAsync(cancellationToken);
            Console.WriteLine($"✅ Kafka iniciado - URL: {BootstrapServers}");
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _container.StopAsync(cancellationToken);
        }

        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Simpler and faster verification: just check that the container is running
                if (_container.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
                    return Task.FromResult(false);

                // Health check más simple sin ejecutar comandos complejos
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _container.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
