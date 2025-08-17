namespace HexagonalSkeleton.Infrastructure.CDC.Configuration
{
    /// <summary>
    /// Kafka configuration for CDC
    /// </summary>
    public class KafkaCdcOptions
    {
        /// <summary>
        /// Kafka bootstrap servers
        /// </summary>
        public string BootstrapServers { get; set; } = "";

        /// <summary>
        /// Producer client ID
        /// </summary>
        public string ProducerClientId { get; set; } = "hexagonal-producer";

        /// <summary>
        /// Consumer client ID
        /// </summary>
        public string ConsumerClientId { get; set; } = "hexagonal-consumer";

        /// <summary>
        /// Consumer group ID
        /// </summary>
        public string ConsumerGroupId { get; set; } = "hexagonal-cdc-consumer-group";

        /// <summary>
        /// Generate unique Group ID for each test execution
        /// </summary>
        public bool GenerateUniqueGroupId { get; set; } = false;

        /// <summary>
        /// Kafka topics to consume CDC events from
        /// </summary>
        public string[] Topics { get; set; } = { "hexagonal-postgres.public.users" };
    }
}
