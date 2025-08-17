namespace HexagonalSkeleton.Infrastructure.CDC.Configuration
{
    /// <summary>
    /// PostgreSQL configuration for CDC
    /// </summary>
    public class PostgreSQLCdcOptions
    {
        /// <summary>
        /// PostgreSQL server host
        /// </summary>
        public string Host { get; set; } = "postgresql";

        /// <summary>
        /// PostgreSQL server port
        /// </summary>
        public string Port { get; set; } = string.Empty;

        /// <summary>
        /// PostgreSQL user
        /// </summary>
        public string User { get; set; } = "hexagonal_user";

        /// <summary>
        /// PostgreSQL password
        /// </summary>
        public string Password { get; set; } = "hexagonal_password";

        /// <summary>
        /// PostgreSQL database
        /// </summary>
        public string Database { get; set; } = "HexagonalSkeleton";

        /// <summary>
        /// Server name for Debezium
        /// </summary>
        public string ServerName { get; set; } = "hexagonal-postgres";

        /// <summary>
        /// Replication slot name
        /// </summary>
        public string SlotName { get; set; } = "hexagonal_slot";

        /// <summary>
        /// Publication name
        /// </summary>
        public string PublicationName { get; set; } = "hexagonal_publication";
    }
}
