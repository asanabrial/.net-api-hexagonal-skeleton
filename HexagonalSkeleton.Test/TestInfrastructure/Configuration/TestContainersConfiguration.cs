using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

/// <summary>
/// Main configuration class for TestContainers loaded from appsettings.Test.json
/// </summary>
public class TestContainersConfiguration
{
    public PostgreSQLConfiguration PostgreSQL { get; set; } = new();
    public MongoDBConfiguration MongoDB { get; set; } = new();
    public KafkaConfiguration Kafka { get; set; } = new();
    public ZookeeperConfiguration Zookeeper { get; set; } = new();
    public SchemaRegistryConfiguration SchemaRegistry { get; set; } = new();
    public DebeziumConnectConfiguration DebeziumConnect { get; set; } = new();
    public CdcConfiguration CDC { get; set; } = new();

    public static TestContainersConfiguration LoadFromConfiguration(IConfiguration configuration)
    {
        var config = new TestContainersConfiguration();
        configuration.GetSection("TestContainers").Bind(config);
        return config;
    }
}
