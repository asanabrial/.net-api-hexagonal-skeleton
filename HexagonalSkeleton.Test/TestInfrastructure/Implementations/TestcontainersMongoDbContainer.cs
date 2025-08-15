using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using Testcontainers.MongoDb;
using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Networks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation for MongoDB container
    /// </summary>
    public class TestcontainersMongoDbContainer : IMongoDbTestContainer
    {
        private readonly MongoDbContainer _container;
        private readonly string _database;
        private readonly string _username;
        private bool _disposed = false;

        public TestcontainersMongoDbContainer(
            string image = "mongo:7",
            string database = "hexagonal_test",
            string username = "test_user",
            string password = "test_password",
            INetwork? network = null)
        {
            _database = database;
            _username = username;
            
            var builder = new MongoDbBuilder()
                .WithImage(image)
                .WithUsername(username)
                .WithPassword(password)
                .WithCleanUp(true);
                
            if (network != null)
            {
                builder = builder.WithNetwork(network);
            }
            
            _container = builder.Build();
        }

        public string ConnectionString => _container.GetConnectionString();
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;
        
        public string DatabaseName => _database;
        
        public string Username => _username;
        
        public int Port => _container.GetMappedPublicPort(MongoDbBuilder.MongoDbPort);

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
                // Try to execute a MongoDB ping command to verify the container is healthy
                var result = await _container.ExecAsync(new[] { "mongosh", "--eval", "db.runCommand('ping')" }, cancellationToken);
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
