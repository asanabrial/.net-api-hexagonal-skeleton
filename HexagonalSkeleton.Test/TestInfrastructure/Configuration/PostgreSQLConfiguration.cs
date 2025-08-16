namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class PostgreSQLConfiguration
{
    public int Port { get; set; }
    public int InternalPort { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool CleanupAfterTest { get; set; }
    public int WaitTimeoutSeconds { get; set; }
    public string InitScript { get; set; } = string.Empty;
}
