using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    public interface ISchemaRegistryTestContainer : IAsyncDisposable
    {
        string SchemaRegistryUrl { get; }
        int Port { get; }
        string ContainerName { get; }
        bool IsRunning { get; }
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
