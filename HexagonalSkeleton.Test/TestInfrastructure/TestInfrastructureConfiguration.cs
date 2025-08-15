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
                StartupTimeout = TimeSpan.FromSeconds(
                    int.Parse(Environment.GetEnvironmentVariable("TEST_CONTAINER_STARTUP_TIMEOUT_SECONDS") ?? "15"))
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
