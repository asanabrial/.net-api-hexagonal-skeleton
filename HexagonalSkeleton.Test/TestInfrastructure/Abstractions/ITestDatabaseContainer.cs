using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Abstraction for database containers used in testing.
    /// This allows switching between different container implementations (Testcontainers, Docker.NET, etc.)
    /// </summary>
    public interface ITestDatabaseContainer : IAsyncDisposable
    {
        /// <summary>
        /// Gets the connection string for the database container
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the container name or identifier
        /// </summary>
        string ContainerName { get; }

        /// <summary>
        /// Indicates if the container is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the database container
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the database container
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the health status of the container
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Specific abstraction for PostgreSQL containers
    /// </summary>
    public interface IPostgreSqlTestContainer : ITestDatabaseContainer
    {
        string DatabaseName { get; }
        string Username { get; }
        int Port { get; }
    }

    /// <summary>
    /// Specific abstraction for MongoDB containers
    /// </summary>
    public interface IMongoDbTestContainer : ITestDatabaseContainer
    {
        string DatabaseName { get; }
        string Username { get; }
        int Port { get; }
    }

    /// <summary>
    /// Specific abstraction for RabbitMQ containers
    /// </summary>
    public interface IRabbitMqTestContainer : ITestDatabaseContainer
    {
        string Username { get; }
        int Port { get; }
        int ManagementPort { get; }
    }
}
