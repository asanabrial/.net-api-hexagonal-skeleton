using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Contract for Kafka testing containers
    /// </summary>
    public interface IKafkaTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// Dirección de los bootstrap servers de Kafka
        /// </summary>
        string BootstrapServers { get; }
        
        /// <summary>
        /// Container name
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indicates if the container is running
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Kafka mapped port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Internal container IP address
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// Starts the Kafka container
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the Kafka container
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies if the container is healthy
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
