using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Networks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainer para Zookeeper oficial de Confluent
    /// </summary>
    public class TestcontainersZookeeperContainer : IZookeeperTestContainer
    {
        private readonly IContainer _container;
        private bool _disposed = false;
        private readonly DockerConfiguration _dockerConfig;

        public TestcontainersZookeeperContainer(
            DockerConfiguration dockerConfig,
            ZookeeperConfiguration zookeeperConfig,
            INetwork? network = null)
        {
            _dockerConfig = dockerConfig;
            Console.WriteLine($"Configuring Zookeeper with image: {dockerConfig.Images.Zookeeper}");
            
            var builder = new ContainerBuilder()
                .WithImage(dockerConfig.Images.Zookeeper)
                .WithPortBinding(dockerConfig.Ports.Zookeeper, dockerConfig.Ports.Zookeeper)
                .WithEnvironment("ZOOKEEPER_CLIENT_PORT", zookeeperConfig.Environment.ClientPort)
                .WithEnvironment("ZOOKEEPER_TICK_TIME", zookeeperConfig.Environment.TickTime)
                .WithEnvironment("ZOOKEEPER_INIT_LIMIT", zookeeperConfig.Environment.InitLimit)
                .WithEnvironment("ZOOKEEPER_SYNC_LIMIT", zookeeperConfig.Environment.SyncLimit)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable(dockerConfig.Ports.Zookeeper))
                .WithCleanUp(true);
                
            if (network != null)
            {
                Console.WriteLine($"Zookeeper using shared network with alias '{dockerConfig.NetworkAliases.Zookeeper}'");
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases(dockerConfig.NetworkAliases.Zookeeper);
            }
                
            _container = builder.Build();
        }

        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => _dockerConfig.Ports.Zookeeper;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Starting Zookeeper...");
            await _container.StartAsync(cancellationToken);
            Console.WriteLine($"Zookeeper started on port: {Port}");
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _container.StopAsync(cancellationToken);
        }

        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_container.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
                    return Task.FromResult(false);

                // Simple health check: container running
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
