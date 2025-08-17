namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    public class PortsConfiguration
    {
        public int Kafka { get; set; }
        public int Zookeeper { get; set; }
        public int SchemaRegistry { get; set; }
        public int DebeziumConnect { get; set; }
    }
}
