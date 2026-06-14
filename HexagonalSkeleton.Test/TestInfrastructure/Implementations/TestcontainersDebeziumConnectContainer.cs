using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer for Debezium Connect
    /// Manages CDC connectors for PostgreSQL
    /// </summary>
    public class TestcontainersDebeziumConnectContainer : IDebeziumConnectTestContainer
    {
        private readonly IContainer _container;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;
        private readonly DockerConfiguration _dockerConfig;
        private readonly DebeziumConnectConfiguration _debeziumConfig;

        public TestcontainersDebeziumConnectContainer(
            DockerConfiguration dockerConfig,
            DebeziumConnectConfiguration debeziumConfig,
            string kafkaBootstrapServers,
            INetwork? network = null)
        {
            _dockerConfig = dockerConfig;
            _debeziumConfig = debeziumConfig;
            
            var builder = new ContainerBuilder(_debeziumConfig.Image)
                .WithPortBinding(_dockerConfig.Ports.DebeziumConnect, _debeziumConfig.InternalPort)
                .WithEnvironment("GROUP_ID", _debeziumConfig.Environment.GroupId)
                .WithEnvironment("CONFIG_STORAGE_TOPIC", _debeziumConfig.Environment.ConfigStorageTopic)
                .WithEnvironment("OFFSET_STORAGE_TOPIC", _debeziumConfig.Environment.OffsetStorageTopic)
                .WithEnvironment("STATUS_STORAGE_TOPIC", _debeziumConfig.Environment.StatusStorageTopic)
                .WithEnvironment("BOOTSTRAP_SERVERS", kafkaBootstrapServers)
                .WithEnvironment("CONNECT_LOG4J_APPENDER_STDOUT_LAYOUT_CONVERSIONPATTERN", _debeziumConfig.Environment.LogLayoutConversionPattern)
                .WithCleanUp(_debeziumConfig.CleanupAfterTest);
            
            if (network != null)
            {
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases(_dockerConfig.NetworkAliases.DebeziumConnect);
            }
            
            _container = builder.Build();
            _httpClient = new HttpClient();
        }

        public string ConnectUrl => $"http://localhost:{_container.GetMappedPublicPort(_dockerConfig.Ports.DebeziumConnect)}";
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _container.GetMappedPublicPort(_dockerConfig.Ports.DebeziumConnect);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _container.StartAsync(cancellationToken);
            
            // Manual health check - wait for Debezium Connect to be ready
            Console.WriteLine("Waiting for Debezium Connect to be ready...");
            
            
            var maxAttempts = 30; // 30 segundos
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(2);
                    var response = await httpClient.GetAsync($"{ConnectUrl}/", cancellationToken);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($" Debezium Connect ready after {i + 1} seconds");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($" Intento {i + 1}: HTTP {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Intento {i + 1}: {ex.GetType().Name} - {ex.Message}");
                    
                    // Show container logs every 5 attempts for intensive debugging
                    if (i % 5 == 4)
                    {
                        try
                        {
                            var (stdout, stderr) = await _container.GetLogsAsync();
                            var recentLogs = stdout.Split('\n').TakeLast(10);
                            
                            foreach (var logLine in recentLogs)
                            {
                                if (!string.IsNullOrWhiteSpace(logLine))
                                    Console.WriteLine($"    {logLine}");
                            }
                            if (!string.IsNullOrWhiteSpace(stderr))
                            {
                                
                                Console.WriteLine($"    {stderr}");
                            }
                        }
                        catch (Exception logEx)
                        {
                            Console.WriteLine($" Could not get logs: {logEx.Message}");
                        }
                    }
                }
                
                await Task.Delay(500, cancellationToken); // Reduced from 1000ms
            }
            
            // Last attempt to get logs before failing
            try
            {
                var (stdout, stderr) = await _container.GetLogsAsync();
                
                Console.WriteLine(stdout);
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    
                    Console.WriteLine(stderr);
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($" Could not get final logs: {logEx.Message}");
            }
            
            throw new TimeoutException($"Debezium Connect did not respond after {maxAttempts} seconds");
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

                // Real health check: try to connect to Debezium Connect REST API
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                var response = await httpClient.GetAsync($"{ConnectUrl}/", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task ConfigurePostgreSqlConnectorAsync(string connectorName, string postgresqlConnectionString, CancellationToken cancellationToken = default)
        {
            try
            {
                // Extraer detalles de la connection string de PostgreSQL
                // Typical format: "Host=localhost;Port=xxxxx;Database=hexagonal_test;Username=test_user;Password=test_password"
                var connectionParams = new Dictionary<string, string>();
                var parts = postgresqlConnectionString.Split(';');
                
                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        connectionParams[keyValue[0].Trim().ToLowerInvariant()] = keyValue[1].Trim();
                    }
                }

                var host = connectionParams.GetValueOrDefault("host", "localhost");
                var port = connectionParams.GetValueOrDefault("port", "5432");
                var database = connectionParams.GetValueOrDefault("database", "hexagonal_test");
                var username = connectionParams.GetValueOrDefault("username", "test_user");
                var password = connectionParams.GetValueOrDefault("password", "test_password");

                // Simplified configuration based on Docker Compose example
                var configDict = new Dictionary<string, object>
                {
                    ["connector.class"] = "io.debezium.connector.postgresql.PostgresConnector",
                    ["database.hostname"] = "postgres", 
                    ["database.port"] = "5432",
                    ["database.user"] = username,
                    ["database.password"] = password,
                    ["database.dbname"] = database,
                    ["database.server.name"] = "hexagonal-postgres", // Match expected topic prefix
                    ["table.include.list"] = "public.users",
                    ["topic.prefix"] = "hexagonal-postgres", // Match expected topic prefix
                    ["plugin.name"] = "pgoutput", // Standard plugin for PostgreSQL >= 10
                    ["slot.name"] = "debezium",
                    ["publication.name"] = "dbz_publication"
                };

                var connectorConfig = new
                {
                    name = connectorName,
                    config = configDict
                };

                var json = JsonSerializer.Serialize(connectorConfig);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Remove existing connector if it exists
                await _httpClient.DeleteAsync($"{ConnectUrl}/connectors/{connectorName}", cancellationToken);
                await Task.Delay(500, cancellationToken); // Reduced from 1000ms
                
                // Create nuevo conector
                var response = await _httpClient.PostAsync($"{ConnectUrl}/connectors", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new InvalidOperationException($"Failed to create Debezium connector: {error}");
                }
                
                Console.WriteLine($" Conector PostgreSQL '{connectorName}' configurado successfully");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to communicate with Debezium Connect: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error configurando conector PostgreSQL: {ex.Message}", ex);
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
