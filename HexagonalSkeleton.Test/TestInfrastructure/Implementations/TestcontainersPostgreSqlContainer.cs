using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using Testcontainers.PostgreSql;
using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation for PostgreSQL container
    /// </summary>
    public class TestcontainersPostgreSqlContainer : IPostgreSqlTestContainer
    {
        private readonly PostgreSqlContainer _container;
        private readonly string _database;
        private readonly string _username;
        private bool _disposed = false;

        public TestcontainersPostgreSqlContainer(
            string image = "debezium/postgres", // ✅ Usar imagen exacta del ejemplo Docker Compose
            string database = "hexagonal_test",
            string username = "appuser", // ✅ Use same user as in the example
            string password = "qwerty", // ✅ Usar misma contraseña del ejemplo
            INetwork? network = null)
        {
            _database = database;
            _username = username;
            
            var builder = new PostgreSqlBuilder()
                .WithImage(image)
                .WithDatabase(database)
                .WithUsername(username)
                .WithPassword(password)
                .WithPortBinding(6532, 6532)
                .WithCleanUp(true)
                .WithEnvironment("POSTGRES_PASSWORD", password)
                .WithEnvironment("POSTGRES_USER", username)
                // PostgreSQL configuration for CDC with Debezium
                .WithCommand("-c", "wal_level=logical", "-c", "max_replication_slots=4", "-c", "max_wal_senders=4")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("pg_isready", "-U", username));
                
            if (network != null)
            {
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases("postgres");
            }
            
            _container = builder.Build();
        }

        public string ConnectionString => _container.GetConnectionString();
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;
        
        public string DatabaseName => _database;
        
        public string Username => _username;
        
        public int Port => 6532; // ✅ Puerto fijo como en el ejemplo

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
                // ✅ Usar las credenciales correctas del ejemplo (appuser)
                var result = await _container.ExecAsync(new[] { "pg_isready", "-U", "appuser" }, cancellationToken);
                return result.ExitCode == 0;
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
                await _container.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
