namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class ZookeeperConfiguration
{
    public int Port { get; set; }
    public int InternalPort { get; set; }
    public string Image { get; set; } = string.Empty;
    public bool CleanupAfterTest { get; set; }
    public int WaitTimeoutSeconds { get; set; }
    public string LogLevel { get; set; } = string.Empty;
}
