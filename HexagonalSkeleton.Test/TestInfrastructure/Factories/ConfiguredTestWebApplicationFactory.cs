using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure;
using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Configured test web application factory that uses testcontainers.
    /// Includes PostgreSQL + MongoDB + Kafka + Debezium Connect for CDC integration tests.
    /// </summary>
    public class ConfiguredTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            return TestInfrastructureConfiguration.CreateConfiguredOrchestrator();
        }
    }
}
