using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using DotNet.Testcontainers.Networks;
using System;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation of the container factory
    /// </summary>
    public class TestcontainersFactory : ITestContainerFactory
    {
        private readonly INetwork? _sharedNetwork;

        public TestcontainersFactory()
        {
            _sharedNetwork = null;
        }

        public TestcontainersFactory(INetwork sharedNetwork)
        {
            _sharedNetwork = sharedNetwork;
        }
        public IPostgreSqlTestContainer CreatePostgreSqlContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersPostgreSqlContainer(
                image: settings.Image ?? "postgres:15-alpine",
                database: settings.Database ?? "hexagonal_test",
                username: settings.Username ?? "test_user",
                password: settings.Password ?? "test_password",
                network: _sharedNetwork
            );
        }

        public IMongoDbTestContainer CreateMongoDbContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersMongoDbContainer(
                image: settings.Image ?? "mongo:7",
                database: settings.Database ?? "hexagonal_test",
                username: settings.Username ?? "test_user",
                password: settings.Password ?? "test_password",
                network: _sharedNetwork
            );
        }

        public IZookeeperTestContainer CreateZookeeperContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersZookeeperContainer(
                network: _sharedNetwork
            );
        }

        public IKafkaTestContainer CreateKafkaContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersKafkaContainer(
                network: _sharedNetwork
            );
        }

        public ISchemaRegistryTestContainer CreateSchemaRegistryContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersSchemaRegistryContainer(
                network: _sharedNetwork
            );
        }

        public IDebeziumConnectTestContainer CreateDebeziumConnectContainer(TestContainerConfiguration? config = null)
        {
            var settings = config ?? new TestContainerConfiguration();
            
            return new TestcontainersDebeziumConnectContainer(
                kafkaBootstrapServers: settings.KafkaBootstrapServers ?? "kafka:9092",
                schemaRegistryUrl: settings.SchemaRegistryUrl ?? "http://schema-registry:8081",
                network: _sharedNetwork
            );
        }
    }
}
