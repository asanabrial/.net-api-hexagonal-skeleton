using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Factory for creating test database containers.
    /// This abstraction allows switching between different container implementations.
    /// </summary>
    public interface ITestContainerFactory
    {
        /// <summary>
        /// Creates a PostgreSQL test container
        /// </summary>
        IPostgreSqlTestContainer CreatePostgreSqlContainer(TestContainerConfiguration? config = null);

        /// <summary>
        /// Creates a MongoDB test container
        /// </summary>
        IMongoDbTestContainer CreateMongoDbContainer(TestContainerConfiguration? config = null);

        /// <summary>
        /// Creates a RabbitMQ test container
        /// </summary>
        IRabbitMqTestContainer CreateRabbitMqContainer(TestContainerConfiguration? config = null);
    }

    /// <summary>
    /// Configuration for test containers
    /// </summary>
    public record TestContainerConfiguration
    {
        public string? Image { get; init; }
        public string? Database { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
        public bool CleanUp { get; init; } = true;
        public TimeSpan StartupTimeout { get; init; } = TimeSpan.FromMinutes(2);
    }

    /// <summary>
    /// Manages the lifecycle of all test containers for a test session
    /// </summary>
    public interface ITestContainerOrchestrator : IAsyncDisposable
    {
        /// <summary>
        /// Gets the PostgreSQL container
        /// </summary>
        IPostgreSqlTestContainer PostgreSql { get; }

        /// <summary>
        /// Gets the MongoDB container
        /// </summary>
        IMongoDbTestContainer MongoDb { get; }

        /// <summary>
        /// Gets the RabbitMQ container
        /// </summary>
        IRabbitMqTestContainer RabbitMq { get; }

        /// <summary>
        /// Starts all containers
        /// </summary>
        Task StartAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops all containers
        /// </summary>
        Task StopAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if all containers are healthy
        /// </summary>
        Task<bool> AreAllHealthyAsync(CancellationToken cancellationToken = default);
    }
}
