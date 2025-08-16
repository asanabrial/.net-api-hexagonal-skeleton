namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration;

public class TestContainersOptions
{
    public const string SectionName = "TestContainers";
    
    public string Database { get; set; } = "hexagonal_test";
    public string Username { get; set; } = "test_user";
    public string Password { get; set; } = "test_password";
    public bool CleanUp { get; set; } = true;
    public int StartupTimeoutSeconds { get; set; } = 15;
    public bool UseSharedContainers { get; set; } = true;
    public bool EnableContainerLogging { get; set; } = false;
}
