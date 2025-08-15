using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Interfaz para contenedor de test de Zookeeper
    /// </summary>
    public interface IZookeeperTestContainer : IAsyncDisposable
    {
        /// <summary>
        /// Puerto público mapeado de Zookeeper
        /// </summary>
        int Port { get; }
        
        /// <summary>
        /// Nombre del contenedor
        /// </summary>
        string ContainerName { get; }
        
        /// <summary>
        /// Indica si el contenedor está corriendo
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Inicia el contenedor
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Detiene el contenedor
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica el estado de salud del Zookeeper
        /// </summary>
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
