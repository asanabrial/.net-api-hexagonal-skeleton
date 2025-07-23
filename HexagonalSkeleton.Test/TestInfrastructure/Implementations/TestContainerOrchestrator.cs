using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Orchestrates the lifecycle of all test containers
    /// </summary>
    public class TestContainerOrchestrator : ITestContainerOrchestrator
    {
        private readonly ITestContainerFactory _factory;
        private bool _disposed = false;

        public TestContainerOrchestrator(ITestContainerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            
            PostgreSql = _factory.CreatePostgreSqlContainer();
            MongoDb = _factory.CreateMongoDbContainer();
            RabbitMq = _factory.CreateRabbitMqContainer();
        }

        public IPostgreSqlTestContainer PostgreSql { get; }
        
        public IMongoDbTestContainer MongoDb { get; }
        
        public IRabbitMqTestContainer RabbitMq { get; }

        public async Task StartAllAsync(CancellationToken cancellationToken = default)
        {
            // Start containers in parallel for better performance
            var tasks = new[]
            {
                PostgreSql.StartAsync(cancellationToken),
                MongoDb.StartAsync(cancellationToken),
                RabbitMq.StartAsync(cancellationToken)
            };

            await Task.WhenAll(tasks);
        }

        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            // Stop containers in parallel
            var tasks = new[]
            {
                PostgreSql.StopAsync(cancellationToken),
                MongoDb.StopAsync(cancellationToken),
                RabbitMq.StopAsync(cancellationToken)
            };

            await Task.WhenAll(tasks);
        }

        public async Task<bool> AreAllHealthyAsync(CancellationToken cancellationToken = default)
        {
            // Check health of all containers in parallel
            var tasks = new[]
            {
                PostgreSql.IsHealthyAsync(cancellationToken),
                MongoDb.IsHealthyAsync(cancellationToken),
                RabbitMq.IsHealthyAsync(cancellationToken)
            };

            var results = await Task.WhenAll(tasks);
            
            // All containers must be healthy
            return results[0] && results[1] && results[2];
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                var tasks = new[]
                {
                    PostgreSql.DisposeAsync(),
                    MongoDb.DisposeAsync(),
                    RabbitMq.DisposeAsync()
                };

                await Task.WhenAll(tasks.Select(t => t.AsTask()));
                _disposed = true;
            }
        }
    }
}
