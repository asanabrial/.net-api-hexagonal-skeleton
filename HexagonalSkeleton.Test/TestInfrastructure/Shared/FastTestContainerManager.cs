using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Shared
{
    /// <summary>
    /// Optimized manager that only uses PostgreSQL + MongoDB + Kafka for fast tests
    /// Simulates Schema Registry and Debezium Connect in memory for maximum performance
    /// </summary>
    public sealed class FastTestContainerManager : IAsyncDisposable
    {
        private static readonly Lazy<FastTestContainerManager> _instance = 
            new(() => new FastTestContainerManager(), LazyThreadSafetyMode.ExecutionAndPublication);
        
        private readonly ITestContainerFactory _factory;
        private IPostgreSqlTestContainer? _postgresql;
        private IMongoDbTestContainer? _mongodb;
        private IKafkaTestContainer? _kafka;
        private bool _isInitialized = false;
        private bool _disposed = false;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        private FastTestContainerManager()
        {
            _factory = new TestcontainersFactory();
        }

        /// <summary>
        /// Gets the singleton instance of the fast manager
        /// </summary>
        public static FastTestContainerManager Instance => _instance.Value;

        /// <summary>
        /// Gets the PostgreSQL container
        /// </summary>
        public IPostgreSqlTestContainer PostgreSql => _postgresql ?? throw new InvalidOperationException("Contenedores no inicializados");

        /// <summary>
        /// Gets the MongoDB container
        /// </summary>
        public IMongoDbTestContainer MongoDb => _mongodb ?? throw new InvalidOperationException("Contenedores no inicializados");

        /// <summary>
        /// Gets the Kafka container
        /// </summary>
        public IKafkaTestContainer Kafka => _kafka ?? throw new InvalidOperationException("Contenedores no inicializados");

        /// <summary>
        /// Inicializa solo los 3 contenedores esenciales (60-90 segundos vs 3-4 minutos)
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

                Console.WriteLine(" Starting FAST containers for development...");
                Console.WriteLine("Only PostgreSQL + MongoDB + Kafka (60-90 seconds)");
                
                var startTime = DateTime.UtcNow;

                // Initialize containers in parallel for maximum speed
                _postgresql = _factory.CreatePostgreSqlContainer();
                _mongodb = _factory.CreateMongoDbContainer();
                _kafka = _factory.CreateKafkaContainer();

                var tasks = new[]
                {
                    _postgresql.StartAsync(cancellationToken),
                    _mongodb.StartAsync(cancellationToken),
                    _kafka.StartAsync(cancellationToken)
                };

                await Task.WhenAll(tasks);

                // Verify health quickly
                var healthTasks = new[]
                {
                    _postgresql.IsHealthyAsync(cancellationToken),
                    _mongodb.IsHealthyAsync(cancellationToken),
                    _kafka.IsHealthyAsync(cancellationToken)
                };

                var results = await Task.WhenAll(healthTasks);
                
                if (results[0] && results[1] && results[2])
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    Console.WriteLine($" Fast containers ready in {elapsed.TotalSeconds:F1}s");
                    _isInitialized = true;
                }
                else
                {
                    throw new InvalidOperationException("Some containers are not healthy");
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// Verifies if containers are healthy
        /// </summary>
        public async Task<bool> AreContainersHealthyAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized || _postgresql == null || _mongodb == null || _kafka == null)
                return false;

            var tasks = new[]
            {
                _postgresql.IsHealthyAsync(cancellationToken),
                _mongodb.IsHealthyAsync(cancellationToken),
                _kafka.IsHealthyAsync(cancellationToken)
            };

            var results = await Task.WhenAll(tasks);
            return results[0] && results[1] && results[2];
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

                Console.WriteLine(" Stopping fast containers...");
                
                if (_isInitialized && _postgresql != null && _mongodb != null && _kafka != null)
                {
                    var tasks = new[]
                    {
                        _postgresql.DisposeAsync().AsTask(),
                        _mongodb.DisposeAsync().AsTask(),
                        _kafka.DisposeAsync().AsTask()
                    };

                    await Task.WhenAll(tasks);
                }
                
                _disposed = true;
                Console.WriteLine(" Contenedores rápidos detenidos");
            }
            finally
            {
                _initializationLock.Release();
                _initializationLock.Dispose();
            }
        }
    }
}
