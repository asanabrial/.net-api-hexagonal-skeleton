using HexagonalSkeleton.Test.TestInfrastructure.Factories;

namespace HexagonalSkeleton.Test.TestInfrastructure.Base;

/// <summary>
/// Base class for integration tests that provides AUTOMATIC database cleanup.
/// Ensures test isolation by cleaning databases before each test execution.
/// Use this for tests that need complete isolation and don't create data within the same test method.
/// For tests that create and use data in the same method, use BaseIntegrationTest instead.
/// </summary>
public abstract class BaseIntegrationTestWithCleanup : BaseIntegrationTest, IAsyncLifetime
{
    protected BaseIntegrationTestWithCleanup(ConfiguredTestWebApplicationFactory factory) : base(factory)
    {
    }

    /// <summary>
    /// XUnit IAsyncLifetime implementation - called before each test
    /// </summary>
    public async Task InitializeAsync()
    {
        await CleanDatabasesAsync();
    }

    /// <summary>
    /// XUnit IAsyncLifetime implementation - called after each test
    /// </summary>
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
