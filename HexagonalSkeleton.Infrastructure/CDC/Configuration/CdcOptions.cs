using System.ComponentModel.DataAnnotations;

namespace HexagonalSkeleton.Infrastructure.CDC.Configuration
{
    /// <summary>
    /// Opciones de configuración para CDC (Change Data Capture)
    /// </summary>
    public class CdcOptions
    {
        public const string SectionName = "CDC";

        /// <summary>
        /// Base de datos objetivo para filtrar eventos CDC en tests
        /// </summary>
        public string? TargetDatabase { get; set; }

        /// <summary>
        /// Si debe procesar solo eventos de la base de datos objetivo
        /// </summary>
        public bool ProcessOnlyTargetDatabase { get; set; } = false;

        /// <summary>
        /// Timeout para esperar sincronización CDC (ms)
        /// </summary>
        public int WaitForSynchronizationTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Máximo número de reintentos de sincronización
        /// </summary>
        public int MaxSyncRetries { get; set; } = 5;

        /// <summary>
        /// Delay entre reintentos de sincronización (ms)
        /// </summary>
        public int SyncRetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// URL del API de Debezium Connect
        /// </summary>
        [Required]
        public string DebeziumConnectUrl { get; set; } = "http://localhost:8083";

        /// <summary>
        /// Configuración de PostgreSQL para CDC
        /// </summary>
        public PostgreSQLCdcOptions PostgreSQL { get; set; } = new();

        /// <summary>
        /// Configuración de Kafka para CDC
        /// </summary>
        public KafkaCdcOptions Kafka { get; set; } = new();

        /// <summary>
        /// Lista de tablas a incluir en CDC
        /// </summary>
        public string TableIncludeList { get; set; } = "public.users";

        /// <summary>
        /// Prefijo de tópicos Kafka
        /// </summary>
        public string TopicPrefix { get; set; } = "hexagonal";
    }

    /// <summary>
    /// Configuración de PostgreSQL para CDC
    /// </summary>
    public class PostgreSQLCdcOptions
    {
        /// <summary>
        /// Host del servidor PostgreSQL
        /// </summary>
        public string Host { get; set; } = "postgresql";

        /// <summary>
        /// Puerto del servidor PostgreSQL
        /// </summary>
        public string Port { get; set; } = "5432";

        /// <summary>
        /// Usuario de PostgreSQL
        /// </summary>
        public string User { get; set; } = "hexagonal_user";

        /// <summary>
        /// Contraseña de PostgreSQL
        /// </summary>
        public string Password { get; set; } = "hexagonal_password";

        /// <summary>
        /// Base de datos de PostgreSQL
        /// </summary>
        public string Database { get; set; } = "HexagonalSkeleton";

        /// <summary>
        /// Nombre del servidor para Debezium
        /// </summary>
        public string ServerName { get; set; } = "hexagonal-postgres";

        /// <summary>
        /// Nombre del slot de replicación
        /// </summary>
        public string SlotName { get; set; } = "hexagonal_slot";

        /// <summary>
        /// Nombre de la publicación
        /// </summary>
        public string PublicationName { get; set; } = "hexagonal_publication";
    }

    /// <summary>
    /// Configuración de Kafka para CDC
    /// </summary>
    public class KafkaCdcOptions
    {
        /// <summary>
        /// Servidores bootstrap de Kafka
        /// </summary>
        public string BootstrapServers { get; set; } = "";

        /// <summary>
        /// ID del cliente producer
        /// </summary>
        public string ProducerClientId { get; set; } = "hexagonal-producer";

        /// <summary>
        /// ID del cliente consumer
        /// </summary>
        public string ConsumerClientId { get; set; } = "hexagonal-consumer";

        /// <summary>
        /// ID del grupo consumer
        /// </summary>
        public string ConsumerGroupId { get; set; } = "hexagonal-cdc-consumer-group";

        /// <summary>
        /// Generar Group ID único para cada ejecución de tests
        /// </summary>
        public bool GenerateUniqueGroupId { get; set; } = false;

        /// <summary>
        /// Topics de Kafka de los que consumir eventos CDC
        /// </summary>
        public string[] Topics { get; set; } = { "hexagonal-postgres.public.users" };
    }
}
