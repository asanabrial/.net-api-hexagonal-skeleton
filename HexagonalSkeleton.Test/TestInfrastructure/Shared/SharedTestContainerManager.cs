using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Shared
{
    /// <summary>
    /// Centralized manager for shared test containers
    /// Ensures all tests use the same container instances for better performance
    /// Implements thread-safe Singleton pattern with lazy initialization
    /// </summary>
    public sealed class SharedTestContainerManager : IAsyncDisposable
    {
        private static readonly Lazy<SharedTestContainerManager> _instance = 
            new(() => new SharedTestContainerManager(), LazyThreadSafetyMode.ExecutionAndPublication);
        
        private readonly ITestContainerOrchestrator _orchestrator;
        private bool _isInitialized = false;
        private bool _disposed = false;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private SharedTestContainerManager()
        {
            var factory = new TestcontainersFactory();
            _orchestrator = new TestContainerOrchestrator(factory);
        }

        /// <summary>
        /// Gets the singleton instance of the container manager
        /// </summary>
        public static SharedTestContainerManager Instance => _instance.Value;

        /// <summary>
        /// Gets the container orchestrator
        /// </summary>
        public ITestContainerOrchestrator Orchestrator => _orchestrator;

        /// <summary>
        /// Initializes all containers if they haven't been initialized
        /// This method is thread-safe and only executes once
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized || _disposed)
                return;

            await _initializationLock.WaitAsync(cancellationToken);
            try
            {
                if (_isInitialized || _disposed)
                    return;

                Console.WriteLine(" Iniciando contenedores compartidos para toda la suite de tests...");
                Console.WriteLine("This may take 2-3 minutes the first time...");
                
                var startTime = DateTime.UtcNow;
                await _orchestrator.StartAllAsync(cancellationToken);
                
                // Verify health with optimized timeout
                var maxRetries = 20;  // More retries with shorter intervals
                var retryDelay = 1000; // Reduced to 1 second
                var retryCount = 0;
                
                while (retryCount < maxRetries)
                {
                    if (await _orchestrator.AreAllHealthyAsync(cancellationToken))
                    {
                        var elapsed = DateTime.UtcNow - startTime;
                        Console.WriteLine($" All CDC containers are healthy and ready in {elapsed.TotalSeconds:F1}s");
                        _isInitialized = true;
                        return;
                    }
                    
                    retryCount++;
                    Console.WriteLine($" Checking container health... ({retryCount}/{maxRetries})");
                    await Task.Delay(retryDelay, cancellationToken);
                }
                
                throw new TimeoutException($"Containers did not reach healthy state after {maxRetries * retryDelay / 1000}s");
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// Configures the Debezium connector for PostgreSQL
        /// </summary>
        public async Task ConfigureDebeziumConnectorAsync(string connectorName = "shared-postgres-connector", CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Containers must be initialized before configuring Debezium");

            try
            {
                await _orchestrator.DebeziumConnect.ConfigurePostgreSqlConnectorAsync(
                    connectorName, 
                    _orchestrator.PostgreSql.ConnectionString,
                    cancellationToken);
                
                Console.WriteLine($" Debezium connector '{connectorName}' configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Warning: Error configuring Debezium connector: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Checks if containers are initialized and healthy
        /// </summary>
        public async Task<bool> AreContainersHealthyAsync(CancellationToken cancellationToken = default)
        {
            return _isInitialized && await _orchestrator.AreAllHealthyAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            await _initializationLock.WaitAsync();
            try
            {
                if (_disposed)
                    return;

                Console.WriteLine(" Deteniendo contenedores compartidos...");
                
                if (_isInitialized)
                {
                    await _orchestrator.DisposeAsync();
                }
                
                _disposed = true;
                Console.WriteLine(" Contenedores compartidos detenidos");
            }
            finally
            {
                _initializationLock.Release();
                _initializationLock.Dispose();
            }
        }
    }
}
