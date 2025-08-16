using HexagonalSkeleton.Test.TestInfrastructure.Abstractions;
using HexagonalSkeleton.Test.TestInfrastructure.Implementations;
using HexagonalSkeleton.Test.TestInfrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.Test.TestInfrastructure;

public static class TestInfrastructureConfiguration
{
    public static ITestContainerFactory GetContainerFactory()
    {
        return new TestcontainersFactory();
    }

    public static TestContainerConfiguration GetDefaultConfiguration(IConfiguration configuration)
    {
        var options = configuration.GetSection(TestContainersOptions.SectionName).Get<TestContainersOptions>() 
                     ?? new TestContainersOptions();
        
        return new TestContainerConfiguration
        {
            Database = options.Database,
            Username = options.Username,
            Password = options.Password,
            CleanUp = options.CleanUp,
            StartupTimeout = TimeSpan.FromSeconds(options.StartupTimeoutSeconds)
        };
    }

    public static ITestContainerOrchestrator CreateConfiguredOrchestrator()
    {
        var factory = GetContainerFactory();
        return new TestContainerOrchestrator(factory);
    }

    public static bool UseSharedContainers(IConfiguration configuration) => 
        configuration.GetSection(TestContainersOptions.SectionName)
                    .GetValue<bool>(nameof(TestContainersOptions.UseSharedContainers));

    public static bool EnableContainerLogging(IConfiguration configuration) => 
        configuration.GetSection(TestContainersOptions.SectionName)
                    .GetValue<bool>(nameof(TestContainersOptions.EnableContainerLogging));
}
