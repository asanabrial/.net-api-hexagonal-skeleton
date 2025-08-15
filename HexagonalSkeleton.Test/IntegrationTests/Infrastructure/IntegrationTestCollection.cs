using Xunit;

namespace HexagonalSkeleton.Test.Integration.Infrastructure
{
    /// <summary>
    /// Collection for integration tests that use containers
    /// Limits parallel execution of tests that use Testcontainers
    /// </summary>
    [CollectionDefinition("Integration Collection", DisableParallelization = true)]
    public class IntegrationTestCollection
    {
        // This class only serves to define the collection
        // Tests that use [Collection("Integration Collection")] 
        // will be executed sequentially among themselves
    }
}
