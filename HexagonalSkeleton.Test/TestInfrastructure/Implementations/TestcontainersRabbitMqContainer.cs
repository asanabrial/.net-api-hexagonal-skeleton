using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using Testcontainers.RabbitMq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation for RabbitMQ container
    /// </summary>
    public class TestcontainersRabbitMqContainer : IRabbitMqTestContainer
    {
        private readonly RabbitMqContainer _container;
        private readonly string _username;
        private bool _disposed = false;

        public TestcontainersRabbitMqContainer(
            string image = "rabbitmq:3-management-alpine",
            string username = "test_user",
            string password = "test_password")
        {
            _username = username;
            _container = new RabbitMqBuilder()
                .WithImage(image)
                .WithUsername(username)
                .WithPassword(password)
                .WithCleanUp(true)
                .Build();
        }

        public string ConnectionString => _container.GetConnectionString();
        
        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;
        
        public string Username => _username;
        
        public int Port => _container.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort);
        
        public int ManagementPort => _container.GetMappedPublicPort(15672); // RabbitMQ management port

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
                // Try to execute a RabbitMQ status command to verify the container is healthy
                var result = await _container.ExecAsync(new[] { "rabbitmqctl", "status" }, cancellationToken);
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
