using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
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
    /// Testcontainer para Confluent Schema Registry oficial
    /// </summary>
    public class TestcontainersSchemaRegistryContainer : ISchemaRegistryTestContainer
    {
        private readonly IContainer _container;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;
        private readonly int _port = 8081;

        public TestcontainersSchemaRegistryContainer(
            string image = "confluentinc/cp-schema-registry:6.2.0",
            INetwork? network = null)
        {
            Console.WriteLine($"🔧 Configurando Schema Registry oficial con imagen: {image}");
            
            var builder = new ContainerBuilder()
                .WithImage(image)
                .WithPortBinding(_port, true)
                // Configuración oficial de Schema Registry
                .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS", "kafka:29092")
                .WithEnvironment("SCHEMA_REGISTRY_HOST_NAME", "schema-registry")
                .WithEnvironment("SCHEMA_REGISTRY_LISTENERS", "http://0.0.0.0:8081")
                .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_TOPIC_REPLICATION_FACTOR", "1")
                // Health check específico para Schema Registry
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable(8081)
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8081).ForPath("/subjects")))
                .WithCleanUp(true);
                
            if (network != null)
            {
                Console.WriteLine($"🌐 Schema Registry usando red compartida con alias 'schema-registry'");
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases("schema-registry");
            }
                
            _container = builder.Build();
            _httpClient = new HttpClient();
        }

        public string SchemaRegistryUrl => $"http://localhost:{_container.GetMappedPublicPort(_port)}";
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _container.GetMappedPublicPort(_port);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"🚀 Iniciando Schema Registry...");
            await _container.StartAsync(cancellationToken);
            Console.WriteLine($"✅ Schema Registry iniciado - URL: {SchemaRegistryUrl}");
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

                // Health check: conectar a Schema Registry REST API
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
