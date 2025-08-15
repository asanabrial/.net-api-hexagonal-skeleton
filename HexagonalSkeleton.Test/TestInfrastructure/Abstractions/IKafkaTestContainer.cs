using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Contrato para contenedores de testing de Kafka
    /// </summary>
    public interface IKafkaTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// Dirección de los bootstrap servers de Kafka
        /// </summary>
        string BootstrapServers { get; }
        
        /// <summary>
        /// Nombre del contenedor
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indica si el contenedor está ejecutándose
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Puerto mapeado de Kafka
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Dirección IP interna del contenedor
        /// </summary>
        string IpAddress { get; }

        /// <summary>
        /// Inicia el contenedor de Kafka
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Detiene el contenedor de Kafka
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si el contenedor está saludable
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
