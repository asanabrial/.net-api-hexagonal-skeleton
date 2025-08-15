using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Contrato para contenedores de testing de Debezium Connect
    /// </summary>
    public interface IDebeziumConnectTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// URL del Debezium Connect
        /// </summary>
        string ConnectUrl { get; }
        
        /// <summary>
        /// Nombre del contenedor
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indica si el contenedor está ejecutándose
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Puerto mapeado del Debezium Connect
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Inicia el contenedor de Debezium Connect
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Detiene el contenedor de Debezium Connect
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si el contenedor está saludable
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Configura un conector de PostgreSQL para CDC
        /// </summary>
        Task ConfigurePostgreSqlConnectorAsync(string connectorName, string postgresqlConnectionString, CancellationToken cancellationToken = default);
    }
}
