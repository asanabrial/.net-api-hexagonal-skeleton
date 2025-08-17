using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Null-object implementation for Debezium Connect in fast mode
    /// </summary>
    internal class NullDebeziumConnectContainer : IDebeziumConnectTestContainer
    {
        public string ConnectUrl => "http://localhost:8083"; // Default URL for compatibility
        public string ContainerName => "null-debezium-connect";
        public bool IsRunning => false;
        public int Port => 8083;

        public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task ConfigurePostgreSqlConnectorAsync(string connectorName, string postgresqlConnectionString, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
