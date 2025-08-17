using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Configuration;
using System;

namespace HexagonalSkeleton.Test.TestInfrastructure.Implementations
{
    /// <summary>
    /// Testcontainers implementation of the container factory
    /// Uses configuration classes instead of hardcoded values
    /// </summary>
    public class TestcontainersFactory : ITestContainerFactory
    {
        private readonly INetwork? _sharedNetwork;
        private TestContainersOptions _options = new();
        private DockerConfiguration _dockerConfig = new();
        private KafkaConfiguration _kafkaConfig = new();
        private ZookeeperConfiguration _zookeeperConfig = new();
        private SchemaRegistryConfiguration _schemaRegistryConfig = new();
        private DebeziumConnectConfiguration _debeziumConnectConfig = new();

        public TestcontainersFactory()
        {
            _sharedNetwork = null;
            LoadConfiguration();
        }

        public TestcontainersFactory(INetwork sharedNetwork)
        {
            _sharedNetwork = sharedNetwork;
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // Load configuration from appsettings.Test.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            
            _options = new TestContainersOptions();
            configuration.GetSection(TestContainersOptions.SectionName).Bind(_options);
            
            _dockerConfig = new DockerConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:Docker").Bind(_dockerConfig);
            
            _kafkaConfig = new KafkaConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:Kafka").Bind(_kafkaConfig);
            
            _zookeeperConfig = new ZookeeperConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:Zookeeper").Bind(_zookeeperConfig);
            
            _schemaRegistryConfig = new SchemaRegistryConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:SchemaRegistry").Bind(_schemaRegistryConfig);
            
            _debeziumConnectConfig = new DebeziumConnectConfiguration();
            configuration.GetSection($"{TestContainersOptions.SectionName}:DebeziumConnect").Bind(_debeziumConnectConfig);
        }
        public IPostgreSqlTestContainer CreatePostgreSqlContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersPostgreSqlContainer(
                _options,
                _dockerConfig,
                network: _sharedNetwork
            );
        }

        public IMongoDbTestContainer CreateMongoDbContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersMongoDbContainer(
                _options,
                _dockerConfig,
                network: _sharedNetwork
            );
        }

        public IZookeeperTestContainer CreateZookeeperContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersZookeeperContainer(
                _dockerConfig,
                _zookeeperConfig,
                network: _sharedNetwork
            );
        }

        public IKafkaTestContainer CreateKafkaContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersKafkaContainer(
                _dockerConfig,
                _kafkaConfig,
                network: _sharedNetwork
            );
        }

        public ISchemaRegistryTestContainer CreateSchemaRegistryContainer(TestContainerConfiguration? config = null)
        {
            return new TestcontainersSchemaRegistryContainer(
                _dockerConfig,
                _schemaRegistryConfig,
                network: _sharedNetwork
            );
        }

        public IDebeziumConnectTestContainer CreateDebeziumConnectContainer(TestContainerConfiguration? config = null)
        {
            var kafkaBootstrapServers = $"{_dockerConfig.NetworkAliases.Kafka}:{_kafkaConfig.InternalNetworkPort}";
            
            return new TestcontainersDebeziumConnectContainer(
                _dockerConfig,
                _debeziumConnectConfig,
                kafkaBootstrapServers,
                network: _sharedNetwork
            );
        }
    }
}
