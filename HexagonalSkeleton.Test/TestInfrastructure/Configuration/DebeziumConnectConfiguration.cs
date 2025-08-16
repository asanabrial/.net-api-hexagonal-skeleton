namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class DebeziumConnectConfiguration
{
    public int Port { get; set; }
    public int InternalPort { get; set; }
    public string Image { get; set; } = string.Empty;
    public string ConnectorName { get; set; } = string.Empty;
    public bool CleanupAfterTest { get; set; }
    public int WaitTimeoutSeconds { get; set; }
    public string DatabaseFilter { get; set; } = string.Empty;
    public string PluginPath { get; set; } = string.Empty;
    public int ConnectorRetryAttempts { get; set; }
    public int ConnectorRetryDelayMs { get; set; }
}
