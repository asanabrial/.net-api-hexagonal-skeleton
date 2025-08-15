using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Factory RÁPIDA para desarrollo que solo usa PostgreSQL + MongoDB + Kafka
    /// Ideal para desarrollo diario con startup de 60-90 segundos vs 3-4 minutos
    /// </summary>
    public class FastTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        /// <summary>
        /// Exposición pública del orquestador para tests
        /// </summary>
        public new ITestContainerOrchestrator ContainerOrchestrator => base.ContainerOrchestrator;

        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            // Crear un orquestador que solo maneja los 3 contenedores esenciales
            return new FastContainerOrchestrator();
        }
    }

    /// <summary>
    /// Orquestador que solo maneja los 3 contenedores esenciales
    /// </summary>
    internal class FastContainerOrchestrator : ITestContainerOrchestrator
    {
        public IPostgreSqlTestContainer PostgreSql => FastTestContainerManager.Instance.PostgreSql;
        public IMongoDbTestContainer MongoDb => FastTestContainerManager.Instance.MongoDb;
        public IKafkaTestContainer Kafka => FastTestContainerManager.Instance.Kafka;

        // Debezium Connect no está disponible en modo rápido
        // Devolvemos implementación null-object en lugar de lanzar excepción
        public IDebeziumConnectTestContainer DebeziumConnect => new NullDebeziumConnectContainer();

        // Debezium Connect no está disponible en modo rápido
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
            // No dispose aquí, FastTestContainerManager maneja el ciclo de vida
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implementación null-object para Debezium Connect en modo rápido
    /// </summary>
    internal class NullDebeziumConnectContainer : IDebeziumConnectTestContainer
    {
        public string ConnectUrl => "http://localhost:8083"; // URL por defecto para compatibilidad
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
