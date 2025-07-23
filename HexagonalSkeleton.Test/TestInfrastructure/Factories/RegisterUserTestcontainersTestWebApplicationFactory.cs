using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure;

namespace HexagonalSkeleton.Test.TestInfrastructure.Factories
{
    /// <summary>
    /// Dedicated test factory for RegisterUserTestcontainersIntegrationTest.
    /// Each test class gets its own isolated container orchestrator to prevent conflicts.
    /// </summary>
    public class RegisterUserTestcontainersTestWebApplicationFactory : AbstractTestWebApplicationFactory
    {
        protected override ITestContainerOrchestrator CreateContainerOrchestrator()
        {
            return TestInfrastructureConfiguration.CreateConfiguredOrchestrator();
        }
    }
}
