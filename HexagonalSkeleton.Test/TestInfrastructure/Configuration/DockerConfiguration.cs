namespace HexagonalSkeleton.Test.TestInfrastructure.Configuration
{
    /// <summary>
    /// Docker configuration for TestContainers
    /// Contains all hardcoded values that were moved from code to settings
    /// </summary>
    public class DockerConfiguration
    {
        public DockerImagesConfiguration Images { get; set; } = new();
        public NetworkAliasesConfiguration NetworkAliases { get; set; } = new();
        public PortsConfiguration Ports { get; set; } = new();
    }
}
