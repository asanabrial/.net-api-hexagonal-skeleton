using Xunit;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Collection for FAST tests that only use PostgreSQL + MongoDB + Kafka
    /// Ideal for daily development (60-90 seconds vs 3-4 minutes)
    /// </summary>
    [CollectionDefinition("FastContainers")]
    public class FastContainersCollection : ICollectionFixture<FastContainersFixture>
    {
        // This class is empty and only serves as a marker for xUnit
    }
}
