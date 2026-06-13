using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer para Kafka oficial de Confluent con Zookeeper
    /// </summary>
    public class TestcontainersKafkaContainer : IKafkaTestContainer
    {
        private readonly IContainer _container;
        private bool _disposed = false;
        private readonly int _port;
        private readonly KafkaConfiguration _kafkaConfig;

        public TestcontainersKafkaContainer(
            DockerConfiguration dockerConfig,
            KafkaConfiguration kafkaConfig,
            INetwork? network = null)
        {
            _port = dockerConfig.Ports.Kafka;
            _kafkaConfig = kafkaConfig;
            Console.WriteLine($"Configuring Kafka with image: {dockerConfig.Images.Kafka}");
            
            var builder = new ContainerBuilder()
                .WithImage(dockerConfig.Images.Kafka)
                .WithPortBinding(dockerConfig.Ports.Kafka, dockerConfig.Ports.Kafka)
                // Traditional mode with Zookeeper (not KRaft)
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECT", kafkaConfig.Environment.ZookeeperConnect)
                // Dual listeners: INTERNAL for containers, EXTERNAL for host
                .WithEnvironment("KAFKA_LISTENERS", kafkaConfig.Environment.Listeners)
                .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", kafkaConfig.Environment.AdvertisedListeners)
                .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", kafkaConfig.Environment.ListenerSecurityProtocolMap)
                .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", kafkaConfig.Environment.InterBrokerListenerName)
                .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", kafkaConfig.Environment.OffsetsTopicReplicationFactor)
                .WithEnvironment("KAFKA_LOG_CLEANER_DELETE_RETENTION_MS", kafkaConfig.Environment.LogCleanerDeleteRetentionMs)
                .WithEnvironment("KAFKA_BROKER_ID", kafkaConfig.Environment.BrokerId)
                .WithEnvironment("KAFKA_MIN_INSYNC_REPLICAS", kafkaConfig.Environment.MinInSyncReplicas)
                .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", kafkaConfig.Environment.AutoCreateTopicsEnable)
                .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", kafkaConfig.Environment.GroupInitialRebalanceDelayMs)
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", kafkaConfig.Environment.TransactionStateLogReplicationFactor)
                .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", kafkaConfig.Environment.TransactionStateLogMinIsr)
                .WithEnvironment("KAFKA_ZOOKEEPER_SESSION_TIMEOUT_MS", kafkaConfig.Environment.ZookeeperSessionTimeoutMs)
                .WithEnvironment("KAFKA_ZOOKEEPER_CONNECTION_TIMEOUT_MS", kafkaConfig.Environment.ZookeeperConnectionTimeoutMs)
                .WithEnvironment("KAFKA_SOCKET_SEND_BUFFER_BYTES", kafkaConfig.Environment.SocketSendBufferBytes)
                .WithEnvironment("KAFKA_SOCKET_RECEIVE_BUFFER_BYTES", kafkaConfig.Environment.SocketReceiveBufferBytes)
                .WithEnvironment("KAFKA_REQUEST_TIMEOUT_MS", kafkaConfig.Environment.RequestTimeoutMs)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(dockerConfig.Ports.Kafka))
                .WithCleanUp(true);
                
            if (network != null)
            {
                Console.WriteLine($"Kafka using shared network with alias '{dockerConfig.NetworkAliases.Kafka}'");
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases(dockerConfig.NetworkAliases.Kafka);
            }
                
            _container = builder.Build();
        }

        public string BootstrapServers => _kafkaConfig.Environment.BootstrapServers;

        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _container.GetMappedPublicPort(_port);

        public string IpAddress => _container.IpAddress;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Starting Kafka container...");
            await _container.StartAsync(cancellationToken);
            Console.WriteLine($"Kafka started - URL: {BootstrapServers}");
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
