using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer for Confluent Schema Registry
    /// </summary>
    public class TestcontainersSchemaRegistryContainer : ISchemaRegistryTestContainer
    {
        private readonly IContainer _container;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;
        private readonly DockerConfiguration _dockerConfig;
        private readonly SchemaRegistryConfiguration _schemaRegistryConfig;

        public TestcontainersSchemaRegistryContainer(
            DockerConfiguration dockerConfig,
            SchemaRegistryConfiguration schemaRegistryConfig,
            INetwork? network = null)
        {
            _dockerConfig = dockerConfig;
            _schemaRegistryConfig = schemaRegistryConfig;
            
            var builder = new ContainerBuilder()
                .WithImage(_schemaRegistryConfig.Image)
                .WithPortBinding(_dockerConfig.Ports.SchemaRegistry, true)
                // Schema Registry configuration from settings
                .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS", _schemaRegistryConfig.Environment.KafkastoreBootstrapServers)
                .WithEnvironment("SCHEMA_REGISTRY_HOST_NAME", _schemaRegistryConfig.Environment.HostName)
                .WithEnvironment("SCHEMA_REGISTRY_LISTENERS", _schemaRegistryConfig.Environment.Listeners)
                .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_TOPIC_REPLICATION_FACTOR", _schemaRegistryConfig.Environment.KafkastoreTopicReplicationFactor)
                // Health check specific for Schema Registry
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable((ushort)_dockerConfig.Ports.SchemaRegistry)
                    .UntilHttpRequestIsSucceeded(r => r.ForPort((ushort)_dockerConfig.Ports.SchemaRegistry).ForPath("/subjects")))
                .WithCleanUp(_schemaRegistryConfig.CleanupAfterTest);
                
            if (network != null)
            {
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases(_dockerConfig.NetworkAliases.SchemaRegistry);
            }
                
            _container = builder.Build();
            _httpClient = new HttpClient();
        }

        public string SchemaRegistryUrl => $"http://localhost:{_container.GetMappedPublicPort(_dockerConfig.Ports.SchemaRegistry)}";
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _container.GetMappedPublicPort(_dockerConfig.Ports.SchemaRegistry);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _container.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _container.StopAsync(cancellationToken);
        }

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_container.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
                    return false;

                // Health check: connect to Schema Registry REST API
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                var response = await httpClient.GetAsync($"{SchemaRegistryUrl}/subjects", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                await _container.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
