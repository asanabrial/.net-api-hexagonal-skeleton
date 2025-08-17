using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.Infrastructure.CDC.Configuration
{
    /// <summary>
    /// CDC (Change Data Capture) configuration options
    /// </summary>
    public class CdcOptions
    {
        public const string SectionName = "CDC";

        /// <summary>
        /// Target database to filter CDC events in tests
        /// </summary>
        public string? TargetDatabase { get; set; }

        /// <summary>
        /// Whether to process only events from the target database
        /// </summary>
        public bool ProcessOnlyTargetDatabase { get; set; } = false;

        /// <summary>
        /// Timeout to wait for CDC synchronization (ms)
        /// </summary>
        public int WaitForSynchronizationTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Maximum number of synchronization retries
        /// </summary>
        public int MaxSyncRetries { get; set; } = 5;

        /// <summary>
        /// Delay between synchronization retries (ms)
        /// </summary>
        public int SyncRetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Debezium Connect API URL
        /// </summary>
        [Required]
        public string DebeziumConnectUrl { get; set; } = string.Empty;

        /// <summary>
        /// PostgreSQL configuration for CDC
        /// </summary>
        public PostgreSQLCdcOptions PostgreSQL { get; set; } = new();

        /// <summary>
        /// Kafka configuration for CDC
        /// </summary>
        public KafkaCdcOptions Kafka { get; set; } = new();

        /// <summary>
        /// List of tables to include in CDC
        /// </summary>
        public string TableIncludeList { get; set; } = "public.users";

        /// <summary>
        /// Kafka topic prefix
        /// </summary>
        public string TopicPrefix { get; set; } = "hexagonal";
    }
}
