using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using Testcontainers.PostgreSql;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            string image = "postgres:15-alpine",
            string database = "hexagonal_test",
            string username = "test_user",
            string password = "test_password")
        {
            _database = database;
            _username = username;
            _container = new PostgreSqlBuilder()
                .WithImage(image)
                .WithDatabase(database)
                .WithUsername(username)
                .WithPassword(password)
                .WithCleanUp(true)
                .Build();
        }

        public string ConnectionString => _container.GetConnectionString();
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;
        
        public string DatabaseName => _database;
        
        public string Username => _username;
        
        public int Port => _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);

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
                // Try to execute a simple command to verify the container is healthy
                var result = await _container.ExecAsync(new[] { "pg_isready", "-U", Username }, cancellationToken);
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
