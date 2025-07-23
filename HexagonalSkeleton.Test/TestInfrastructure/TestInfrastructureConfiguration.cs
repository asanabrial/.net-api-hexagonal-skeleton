using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using System;

namespace HexagonalSkeleton.Test.TestInfrastructure
{
    /// <summary>
    /// Central configuration for test infrastructure using Testcontainers.
    /// </summary>
    public static class TestInfrastructureConfiguration
    {
        /// <summary>
        /// Gets the Testcontainers factory
        /// </summary>
        public static ITestContainerFactory GetContainerFactory()
        {
            return new TestcontainersFactory();
        }

        /// <summary>
        /// Gets the default container configuration
        /// </summary>
        public static TestContainerConfiguration GetDefaultConfiguration()
        {
            return new TestContainerConfiguration
            {
                Database = Environment.GetEnvironmentVariable("TEST_DATABASE_NAME") ?? "hexagonal_test",
                Username = Environment.GetEnvironmentVariable("TEST_DATABASE_USER") ?? "test_user",
                Password = Environment.GetEnvironmentVariable("TEST_DATABASE_PASSWORD") ?? "test_password",
                CleanUp = bool.Parse(Environment.GetEnvironmentVariable("TEST_CONTAINER_CLEANUP") ?? "true"),
                StartupTimeout = TimeSpan.FromMinutes(
                    int.Parse(Environment.GetEnvironmentVariable("TEST_CONTAINER_TIMEOUT_MINUTES") ?? "2"))
            };
        }

        /// <summary>
        /// Gets PostgreSQL-specific configuration
        /// </summary>
        public static TestContainerConfiguration GetPostgreSqlConfiguration()
        {
            var baseConfig = GetDefaultConfiguration();
            return baseConfig with
            {
                Image = Environment.GetEnvironmentVariable("TEST_POSTGRES_IMAGE") ?? "postgres:15-alpine"
            };
        }

        /// <summary>
        /// Gets MongoDB-specific configuration
        /// </summary>
        public static TestContainerConfiguration GetMongoDbConfiguration()
        {
            var baseConfig = GetDefaultConfiguration();
            return baseConfig with
            {
                Image = Environment.GetEnvironmentVariable("TEST_MONGODB_IMAGE") ?? "mongo:7"
            };
        }

        /// <summary>
        /// Gets RabbitMQ-specific configuration
        /// </summary>
        public static TestContainerConfiguration GetRabbitMqConfiguration()
        {
            var baseConfig = GetDefaultConfiguration();
            return baseConfig with
            {
                Image = Environment.GetEnvironmentVariable("TEST_RABBITMQ_IMAGE") ?? "rabbitmq:3-management-alpine"
            };
        }

        /// <summary>
        /// Creates a configured test container orchestrator
        /// </summary>
        public static ITestContainerOrchestrator CreateConfiguredOrchestrator()
        {
            var factory = GetContainerFactory();
            return new TestContainerOrchestrator(factory);
        }

        /// <summary>
        /// Indicates if containers should be shared across test classes (for performance)
        /// </summary>
        public static bool UseSharedContainers => 
            bool.Parse(Environment.GetEnvironmentVariable("TEST_USE_SHARED_CONTAINERS") ?? "true");

        /// <summary>
        /// Indicates if detailed container logging should be enabled
        /// </summary>
        public static bool EnableContainerLogging => 
            bool.Parse(Environment.GetEnvironmentVariable("TEST_ENABLE_CONTAINER_LOGGING") ?? "false");
    }
}
