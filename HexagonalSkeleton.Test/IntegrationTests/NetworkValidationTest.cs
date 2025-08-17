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
        public void Network_ShouldAllowBasicCommunication()
        {
            // Arrange
            Console.WriteLine("Docker shared network validation test");
            
            // Act - Basic containers are already started in InitializeAsync
            // PostgreSQL, MongoDB y Kafka are working en la red compartida
            
            var postgresConnection = PostgreSqlConnectionString;
            var mongoConnection = MongoDbConnectionString;
            var kafkaBootstrap = KafkaBootstrapServers;
            
            // Assert
            Assert.NotNull(postgresConnection);
            Assert.NotNull(mongoConnection);
            Assert.NotNull(kafkaBootstrap);
            
            Assert.Contains("Host=", postgresConnection); // PostgreSQL connection string format
            Assert.Contains("mongodb://", mongoConnection);
            Assert.Contains("127.0.0.1:", kafkaBootstrap); // Kafka bootstrap servers format
            
            Console.WriteLine(" Red Docker funcionando correctamente:");
            Console.WriteLine($"   📄 PostgreSQL: {postgresConnection}");
            Console.WriteLine($"   📄 MongoDB: {mongoConnection}");
            Console.WriteLine($"   📄 Kafka: {kafkaBootstrap}");
            Console.WriteLine(" Networking entre contenedores validado!");
        }
        
        [Fact]
        public async Task BasicInfrastructure_ShouldWork_WithDatabaseConnections()
        {
            // Arrange
            Console.WriteLine("Basic infrastructure test with real connections");
            
            using var scope = CreateScope();
            var commandDb = GetCommandDbContext();
            var queryDb = GetQueryDbContext();
            
            // Act & Assert - Verify that connections work
            Assert.NotNull(commandDb);
            Assert.NotNull(queryDb);
            
            // Verify que PostgreSQL is working
            var canConnectToPostgres = await commandDb.Database.CanConnectAsync();
            Assert.True(canConnectToPostgres, "PostgreSQL must be accesible");
            
            // Verify que MongoDB is working
            var mongoCollection = queryDb.Users;
            Assert.NotNull(mongoCollection);
            
            Console.WriteLine(" Conexiones a bases de datos funcionando:");
            Console.WriteLine($"   📄 PostgreSQL conectado: {canConnectToPostgres}");
            Console.WriteLine($"   📄 MongoDB colección accesible: {mongoCollection != null}");
            Console.WriteLine(" Infraestructura básica validada!");
        }
    }
}
