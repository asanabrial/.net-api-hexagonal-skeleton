namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions;

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
    /// Gets the Kafka container for CDC testing
    /// </summary>
    IKafkaTestContainer Kafka { get; }
    
    /// <summary>
    /// Gets the Debezium Connect container for CDC
    /// </summary>
    IDebeziumConnectTestContainer DebeziumConnect { get; }

    /// <summary>
    /// Indicates if Debezium Connect is available and ready for use
    /// </summary>
    bool IsDebeziumConnectAvailable { get; }

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
