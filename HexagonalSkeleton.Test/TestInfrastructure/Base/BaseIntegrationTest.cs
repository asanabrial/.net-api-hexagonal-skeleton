using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.Test.TestInfrastructure.Factories;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;

namespace HexagonalSkeleton.Test.TestInfrastructure.Base;

/// <summary>
/// Base class for integration tests that provides database cleanup utilities.
/// Does NOT perform automatic cleanup - tests must call cleanup methods explicitly if needed.
/// Use BaseIntegrationTestWithCleanup for automatic cleanup between tests.
/// </summary>
public abstract class BaseIntegrationTest<TFactory> where TFactory : AbstractTestWebApplicationFactory
{
    protected readonly TFactory _factory;
    protected readonly HttpClient _client;

    protected BaseIntegrationTest(TFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Manual database cleanup method.
    /// Call this method explicitly when you need to clean databases.
    /// </summary>
    private async Task CleanupDatabases()
    {
        try
        {
            // Clean both Command (PostgreSQL) and Query (MongoDB) databases
            await DatabaseCleanupHelper.CleanAllDatabasesAsync(_factory.Services);
        }
        catch (Exception ex)
        {
            // Log but don't fail the test initialization
            Console.WriteLine($"Warning: Database cleanup failed during test initialization: {ex.Message}");
        }
    }

    /// <summary>
    /// Manual cleanup method that can be called within tests if needed.
    /// Useful for tests that need multiple clean states within a single test.
    /// </summary>
    protected async Task CleanDatabasesAsync()
    {
        await DatabaseCleanupHelper.CleanAllDatabasesAsync(_factory.Services);
    }

    /// <summary>
    /// Clean only the command database (PostgreSQL).
    /// Useful when you only need to reset command-side data.
    /// </summary>
    protected async Task CleanCommandDatabaseAsync()
    {
        await DatabaseCleanupHelper.CleanCommandDatabaseOnlyAsync(_factory.Services);
    }

    /// <summary>
    /// Clean only the query database (MongoDB).
    /// Useful when you only need to reset query-side data.
    /// </summary>
    protected async Task CleanQueryDatabaseAsync()
    {
        await DatabaseCleanupHelper.CleanQueryDatabaseOnlyAsync(_factory.Services);
    }
}

/// <summary>
/// Non-generic version for backward compatibility with existing tests that use ConfiguredTestWebApplicationFactory.
/// </summary>
public abstract class BaseIntegrationTest : BaseIntegrationTest<ConfiguredTestWebApplicationFactory>
{
    protected BaseIntegrationTest(ConfiguredTestWebApplicationFactory factory) : base(factory)
    {
    }
}
