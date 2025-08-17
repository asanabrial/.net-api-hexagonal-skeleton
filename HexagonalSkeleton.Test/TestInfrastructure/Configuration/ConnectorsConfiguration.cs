namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    /// <summary>
    /// Configuration for Debezium connectors in tests
    /// Contains connector names and types that were hardcoded
    /// </summary>
    public class ConnectorsConfiguration
    {
        public string PostgreSQLConnectorName { get; set; } = string.Empty;
        public string ConnectorType { get; set; } = string.Empty;
    }
}
