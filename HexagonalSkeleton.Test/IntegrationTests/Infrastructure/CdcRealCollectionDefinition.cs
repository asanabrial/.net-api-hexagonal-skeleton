using Xunit;

namespace HexagonalSkeleton.Test.Integration.Infrastructure
{
    /// <summary>
    /// Collection definition for CDC tests with basic infrastructure
    /// CDC tests that work with PostgreSQL, MongoDB and Kafka only
    /// Without Schema Registry or Debezium Connect for maximum speed and stability
    /// </summary>
    [CollectionDefinition("CDC Real Collection", DisableParallelization = true)]
    public class CdcRealCollectionDefinition
    {
        // This class is only used to define the collection
        // CDC tests use basic infrastructure: PostgreSQL + MongoDB + Kafka
    }
}
