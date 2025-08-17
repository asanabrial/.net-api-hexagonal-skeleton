using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Interface for Zookeeper test container
    /// </summary>
    public interface IZookeeperTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// Gets the port number for Zookeeper
        /// </summary>
        int Port { get; }
        
        /// <summary>
        /// Container name
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indicates if the container is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the container
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the container
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks the health status of Zookeeper
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
