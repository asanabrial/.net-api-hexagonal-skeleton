using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Shared;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Orchestrator that only manages the 3 essential containers
    /// </summary>
    internal class FastContainerOrchestrator : ITestContainerOrchestrator
    {
        public IPostgreSqlTestContainer PostgreSql => FastTestContainerManager.Instance.PostgreSql;
        public IMongoDbTestContainer MongoDb => FastTestContainerManager.Instance.MongoDb;
        public IKafkaTestContainer Kafka => FastTestContainerManager.Instance.Kafka;

        // Debezium Connect is not available in fast mode
        // We return null-object implementation instead of throwing exception
        public IDebeziumConnectTestContainer DebeziumConnect => new NullDebeziumConnectContainer();

        // Debezium Connect is not available in fast mode
        public bool IsDebeziumConnectAvailable => false;

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
            await FastTestContainerManager.Instance.InitializeAsync(cancellationToken);
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            await FastTestContainerManager.Instance.DisposeAsync();
        }

        public async Task<bool> AreAllHealthyAsync(CancellationToken cancellationToken = default)
        {
            return await FastTestContainerManager.Instance.AreContainersHealthyAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            // No dispose here, FastTestContainerManager manages the lifecycle
            await Task.CompletedTask;
        }
    }
}
