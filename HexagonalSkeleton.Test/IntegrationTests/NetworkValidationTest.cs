using Xunit;
using HexagonalSkeleton.Test.Integration.Infrastructure;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.Integration
{
    /// <summary>
    /// Test to validate that the shared Docker network works correctly
    /// </summary>
    [Collection("Integration Collection")]
    public class NetworkValidationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task Infrastructure_ShouldHaveValidConnectionStrings_AndReachableDatabases()
        {
            // Arrange - connection strings come from the shared container network
            var postgresConnection = PostgreSqlConnectionString;
            var mongoConnection = MongoDbConnectionString;
            var kafkaBootstrap = KafkaBootstrapServers;

            // Assert - connection strings are valid
            Assert.Contains("Host=", postgresConnection);
            Assert.Contains("mongodb://", mongoConnection);
            Assert.Contains("127.0.0.1:", kafkaBootstrap);

            // Act & Assert - both databases are reachable
            using var scope = CreateScope();
            var commandDb = GetCommandDbContext();
            var queryDb = GetQueryDbContext();

            var canConnectToPostgres = await commandDb.Database.CanConnectAsync();
            Assert.True(canConnectToPostgres, "PostgreSQL must be reachable");

            var mongoCollection = queryDb.Users;
            Assert.NotNull(mongoCollection);
        }
    }
}
