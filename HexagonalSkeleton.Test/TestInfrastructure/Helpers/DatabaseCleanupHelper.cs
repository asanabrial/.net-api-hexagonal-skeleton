using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;

namespace HexagonalSkeleton.Test.TestInfrastructure.Helpers;

/// <summary>
/// Helper class for cleaning up test databases between test runs.
/// Ensures test isolation by removing data from both PostgreSQL (Command) and MongoDB (Query) databases.
/// </summary>
public static class DatabaseCleanupHelper
{
    /// <summary>
    /// Cleans all data from both Command (PostgreSQL) and Query (MongoDB) databases.
    /// Call this method at the beginning of each integration test to ensure clean state.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider</param>
    public static async Task CleanAllDatabasesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        // Clean PostgreSQL (Command side)
        await CleanCommandDatabaseAsync(scope.ServiceProvider);
        
        // Clean MongoDB (Query side)
        await CleanQueryDatabaseAsync(scope.ServiceProvider);
    }
    
    /// <summary>
    /// Cleans the PostgreSQL command database by truncating all tables.
    /// Uses TRUNCATE for better performance and to reset identity sequences.
    /// </summary>
    private static async Task CleanCommandDatabaseAsync(IServiceProvider serviceProvider)
    {
        var commandContext = serviceProvider.GetRequiredService<CommandDbContext>();
        
        try
        {
            // Get all table names dynamically
            var tableNames = commandContext.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (tableNames.Any())
            {
                // Disable foreign key constraints
                await commandContext.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");
                
                // Truncate all tables with parameterized queries to avoid SQL injection
                foreach (var tableName in tableNames)
                {
                    // tableName comes from EF Core model, so it's safe from injection
                    #pragma warning disable EF1002
                    await commandContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE");
                    #pragma warning restore EF1002
                }
                
                // Re-enable foreign key constraints
                await commandContext.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail the test
            Console.WriteLine($"Warning: Failed to clean command database: {ex.Message}");
            
            // Fallback: Try to delete records individually
            try
            {
                var users = commandContext.Users.ToList();
                commandContext.Users.RemoveRange(users);
                await commandContext.SaveChangesAsync();
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Warning: Fallback cleanup also failed: {fallbackEx.Message}");
            }
        }
    }
    
    /// <summary>
    /// Cleans the MongoDB query database by clearing all collections.
    /// This ensures complete cleanup of denormalized query data.
    /// </summary>
    private static async Task CleanQueryDatabaseAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var queryContext = serviceProvider.GetRequiredService<QueryDbContext>();
            
            // Clear users collection
            if (queryContext.Users != null)
            {
                await queryContext.Users.DeleteManyAsync(FilterDefinition<Infrastructure.Persistence.Query.Documents.UserQueryDocument>.Empty);
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail the test
            Console.WriteLine($"Warning: Failed to clean query database: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Cleans only the PostgreSQL command database.
    /// Use this when you only need to clean command-side data.
    /// </summary>
    public static async Task CleanCommandDatabaseOnlyAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await CleanCommandDatabaseAsync(scope.ServiceProvider);
    }
    
    /// <summary>
    /// Cleans only the MongoDB query database.
    /// Use this when you only need to clean query-side data.
    /// </summary>
    public static async Task CleanQueryDatabaseOnlyAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await CleanQueryDatabaseAsync(scope.ServiceProvider);
    }
}
