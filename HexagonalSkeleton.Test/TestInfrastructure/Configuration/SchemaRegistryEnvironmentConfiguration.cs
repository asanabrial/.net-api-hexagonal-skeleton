namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    public class SchemaRegistryEnvironmentConfiguration
    {
        public string KafkastoreBootstrapServers { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Listeners { get; set; } = string.Empty;
        public string KafkastoreTopicReplicationFactor { get; set; } = string.Empty;
    }
}
