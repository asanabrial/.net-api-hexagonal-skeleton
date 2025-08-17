using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Shared;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// FAST Factory for development that only uses PostgreSQL + MongoDB + Kafka
    /// Ideal for daily development with 60-90 second startup vs 3-4 minutes
    /// </summary>
    public class FastTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        /// <summary>
        /// Public exposure of the orchestrator for tests
        /// </summary>
        public new ITestContainerOrchestrator ContainerOrchestrator => base.ContainerOrchestrator;

        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            // Create an orchestrator that only manages the 3 essential containers
            return new FastContainerOrchestrator();
        }
    }
}
