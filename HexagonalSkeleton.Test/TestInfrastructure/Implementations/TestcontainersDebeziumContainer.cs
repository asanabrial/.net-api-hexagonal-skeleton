using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer para Debezium Connect con configuración para CDC testing
    /// </summary>
    public class TestcontainersDebeziumContainer : IAsyncDisposable
    {
        private readonly IContainer _container;
        private readonly string _kafkaBootstrapServers;
        private readonly string _postgresHost;
        private readonly string _postgresPort;

        public TestcontainersDebeziumContainer(
            string kafkaBootstrapServers,
            string postgresHost = "host.docker.internal",
            string postgresPort = "5432",
            INetwork? network = null)
        {
            _kafkaBootstrapServers = kafkaBootstrapServers;
            _postgresHost = postgresHost;
            _postgresPort = postgresPort;

            _container = new ContainerBuilder()
                .WithImage("debezium/connect:2.5")
                .WithPortBinding(8083, true)
                .WithEnvironment("BOOTSTRAP_SERVERS", kafkaBootstrapServers)
                .WithEnvironment("GROUP_ID", "hexagonal-test-connect")
                .WithEnvironment("CONFIG_STORAGE_TOPIC", "hexagonal_test_connect_configs")
                .WithEnvironment("OFFSET_STORAGE_TOPIC", "hexagonal_test_connect_offsets")
                .WithEnvironment("STATUS_STORAGE_TOPIC", "hexagonal_test_connect_statuses")
                .WithEnvironment("CONFIG_STORAGE_REPLICATION_FACTOR", "1")
                .WithEnvironment("OFFSET_STORAGE_REPLICATION_FACTOR", "1")
                .WithEnvironment("STATUS_STORAGE_REPLICATION_FACTOR", "1")
                .WithEnvironment("CONNECT_REST_ADVERTISED_HOST_NAME", "localhost")
                .WithEnvironment("CONNECT_PLUGIN_PATH", "/kafka/connect")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r
                        .ForPath("/")
                        .ForPort(8083)
                        .ForStatusCode(System.Net.HttpStatusCode.OK)))
                .WithNetwork(network)
                .Build();
        }

        public string ConnectUrl => $"http://localhost:{Port}";
        public int Port => _container.GetMappedPublicPort(8083);
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

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
            if (!IsRunning) return false;

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{ConnectUrl}/", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Configures the PostgreSQL connector for CDC
        /// </summary>
        public async Task ConfigurePostgresConnectorAsync(
            string connectorName = "postgres-users-test-connector",
            CancellationToken cancellationToken = default)
        {
            var connectorConfig = new
            {
                name = connectorName,
                config = new
                {
                    connector_class = "io.debezium.connector.postgresql.PostgresConnector",
                    database_hostname = _postgresHost,
                    database_port = _postgresPort,
                    database_user = "hexagonal_user",
                    database_password = "hexagonal_password",
                    database_dbname = "HexagonalSkeleton",
                    database_server_name = "hexagonal-postgres-test",
                    topic_prefix = "hexagonal-postgres-test",
                    table_include_list = "public.Users",
                    plugin_name = "pgoutput",
                    publication_name = "hexagonal_test_publication"
                }
            };

            using var httpClient = new HttpClient();
            var json = System.Text.Json.JsonSerializer.Serialize(connectorConfig);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{ConnectUrl}/connectors", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to configure Debezium connector: {response.StatusCode} - {error}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
