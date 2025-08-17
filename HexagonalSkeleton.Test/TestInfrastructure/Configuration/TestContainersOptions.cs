namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class TestContainersOptions
{
    public const string SectionName = "TestContainers";
    
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool CleanUp { get; set; }
    public int StartupTimeoutSeconds { get; set; }
    public bool UseSharedContainers { get; set; }
    public bool EnableContainerLogging { get; set; }
}
