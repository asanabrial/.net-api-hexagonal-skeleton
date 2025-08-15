using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
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

        public TestcontainersZookeeperContainer(
            string image = "confluentinc/cp-zookeeper", // Use exact image from example without version
            INetwork? network = null)
        {
            Console.WriteLine($"🔧 Configurando Zookeeper con imagen: {image}");
            
            var builder = new ContainerBuilder()
                .WithImage(image)
                .WithPortBinding(2181, 2181) // ✅ Puerto 2181:2181 como en el ejemplo
                // ✅ Variable de entorno EXACTA del Docker Compose
                .WithEnvironment("ZOOKEEPER_CLIENT_PORT", "2181")
                // ✅ Configuraciones adicionales para mejorar estabilidad 
                .WithEnvironment("ZOOKEEPER_TICK_TIME", "2000") // Tiempo base de tick
                .WithEnvironment("ZOOKEEPER_INIT_LIMIT", "10") // Initial limit
                .WithEnvironment("ZOOKEEPER_SYNC_LIMIT", "5") // Synchronization limit
                // Zookeeper specific health check
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilPortIsAvailable(2181))
                .WithCleanUp(true);
                
            if (network != null)
            {
                Console.WriteLine($"🌐 Zookeeper usando red compartida con alias 'zookeeper'");
                builder = builder.WithNetwork(network)
                    .WithNetworkAliases("zookeeper");
            }
                
            _container = builder.Build();
        }

        public string ContainerName => _container.Name;
        
        public bool IsRunning => _container.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

        public int Port => 2181; // ✅ Puerto fijo como en el ejemplo

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"🚀 Iniciando Zookeeper...");
            await _container.StartAsync(cancellationToken);
            Console.WriteLine($"✅ Zookeeper iniciado en puerto: {Port}");
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
