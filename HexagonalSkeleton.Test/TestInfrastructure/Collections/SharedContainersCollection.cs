using Xunit;

namespace HexagonalSkeleton.Test.TestInfrastructure.Collections
{
    /// <summary>
    /// Test collection that shares containers for better performance
    /// Initializes containers once for the entire suite and cleans them up at the end
    /// </summary>
    [CollectionDefinition("SharedContainers")]
    public class SharedContainersCollection : ICollectionFixture<SharedContainersFixture>
    {
        // This class is empty and only serves as a marker for xUnit
        // The logic is in SharedContainersFixture
    }
}
