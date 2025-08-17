namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    public class DockerImagesConfiguration
    {
        public string PostgreSQL { get; set; } = string.Empty;
        public string MongoDB { get; set; } = string.Empty;
        public string Kafka { get; set; } = string.Empty;
        public string Zookeeper { get; set; } = string.Empty;
        public string SchemaRegistry { get; set; } = string.Empty;
        public string DebeziumConnect { get; set; } = string.Empty;
    }
}
