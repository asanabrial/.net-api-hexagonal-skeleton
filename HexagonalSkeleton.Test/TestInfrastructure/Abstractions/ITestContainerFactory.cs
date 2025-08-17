using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.TestInfrastructure.Abstractions
{
    /// <summary>
    /// Factory for creating test database containers.
    /// This abstraction allows switching between different container implementations.
    /// </summary>
    public interface ITestContainerFactory
    {
        /// <summary>
        /// Creates a PostgreSQL test container
        /// </summary>
        IPostgreSqlTestContainer CreatePostgreSqlContainer(TestContainerConfiguration? config = null);

        /// <summary>
        /// Creates a MongoDB test container
        /// </summary>
        IMongoDbTestContainer CreateMongoDbContainer(TestContainerConfiguration? config = null);
        
        /// <summary>
        /// Creates a Zookeeper test container for Kafka coordination
        /// </summary>
        IZookeeperTestContainer CreateZookeeperContainer(TestContainerConfiguration? config = null);
        
        /// <summary>
        /// Creates a Kafka test container for CDC testing
        /// </summary>
        IKafkaTestContainer CreateKafkaContainer(TestContainerConfiguration? config = null);
        
        /// <summary>
        /// Creates a Schema Registry test container for Avro schemas
        /// </summary>
        ISchemaRegistryTestContainer CreateSchemaRegistryContainer(TestContainerConfiguration? config = null);
        
        /// <summary>
        /// Creates a Debezium Connect test container for CDC
        /// </summary>
        IDebeziumConnectTestContainer CreateDebeziumConnectContainer(TestContainerConfiguration? config = null);
    }

    /// <summary>
}
