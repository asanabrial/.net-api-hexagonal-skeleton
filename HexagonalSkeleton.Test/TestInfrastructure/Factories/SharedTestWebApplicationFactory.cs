using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Test Web Application Factory that uses shared containers for better performance
    /// All tests in the suite will share the same instance of PostgreSQL, MongoDB, Kafka, etc.
    /// </summary>
    public class SharedTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        /// <summary>
        /// Exposes the container orchestrator for test verifications
        /// </summary>
        public new ITestContainerOrchestrator ContainerOrchestrator => base.ContainerOrchestrator;

        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            // Use the shared manager to get reusable containers
            return SharedTestContainerManager.Instance.Orchestrator;
        }

        public override async Task InitializeAsync()
        {
            // Initialize shared containers (only runs once)
            await SharedTestContainerManager.Instance.InitializeAsync();
            
            // Execute base initialization
            await base.InitializeAsync();
            
            // Configure Debezium once for all tests
            try
            {
                await SharedTestContainerManager.Instance.ConfigureDebeziumConnectorAsync("shared-test-connector");
            }
            catch
            {
                // If Debezium Connect fails, continue with TestCdcEventPublisher as fallback
            }
        }

        public override async Task DisposeAsync()
        {
            // Don't dispose shared containers here
            // Only run base cleanup without containers
            await base.DisposeAsync();
        }
    }
}
