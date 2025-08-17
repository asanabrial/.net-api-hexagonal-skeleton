using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Contract for Debezium Connect testing containers
    /// </summary>
    public interface IDebeziumConnectTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// URL del Debezium Connect
        /// </summary>
        string ConnectUrl { get; }
        
        /// <summary>
        /// Container name
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indicates if the container is running
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Gets the port number for Debezium Connect
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Starts the Debezium Connect container
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the Debezium Connect container
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies if the container is healthy
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Configura un conector de PostgreSQL para CDC
        /// </summary>
        Task ConfigurePostgreSqlConnectorAsync(string connectorName, string postgresqlConnectionString, CancellationToken cancellationToken = default);
    }
}
