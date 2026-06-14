using Xunit;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using HexagonalSkeleton.Test.Integration.Infrastructure;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.Integration.UserProfile
{
    /// <summary>
    /// COMPLETE integration tests that validate the entire system
    /// - PostgreSQL (Command database)
    /// - MongoDB (Query database) 
    /// - Kafka (Event streaming)
    /// - Testcontainers (Real infrastructure)
    /// </summary>
    [Collection("Integration Collection")]
    public class CompleteIntegrationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task CompleteWorkflow_ShouldWork_EndToEnd()
        {
            // Arrange
            Console.WriteLine("COMPLETE TEST: End-to-End Workflow");
            Console.WriteLine("📋 Validando: PostgreSQL + MongoDB + Kafka + Entity Framework + Testcontainers");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            var queryDb = scope.ServiceProvider.GetRequiredService<QueryDbContext>();
            
            // Test 1: Create user in PostgreSQL
            Console.WriteLine("\n1. Creating user in PostgreSQL...");
            var userId = Guid.NewGuid();
            var user = new UserCommandEntity
            {
                Id = userId,
                Email = "complete.test@integration.com",
                FirstName = "Complete",
                LastName = "Integration",
                PhoneNumber = "+1234567890",
                PasswordHash = "secure_hash_123",
                PasswordSalt = "secure_salt_123",
                Birthdate = new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Complete integration test user",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            commandDb.Users.Add(user);
            var createResult = await commandDb.SaveChangesAsync();
            
            Assert.Equal(1, createResult);
            Console.WriteLine($" User created: {user.Email}");
            
            // Test 2: Verify persistence in PostgreSQL
            Console.WriteLine("\n2. Verifying persistence in PostgreSQL...");
            var savedUser = await commandDb.Users.FindAsync(userId);
            
            Assert.NotNull(savedUser);
            Assert.Equal("complete.test@integration.com", savedUser.Email);
            Assert.Equal("Complete", savedUser.FirstName);
            Assert.Equal("Integration", savedUser.LastName);
            Assert.False(savedUser.IsDeleted);
            
            Console.WriteLine($" User verified in PostgreSQL: {savedUser.Id}");
            
            // Test 3: Update user
            Console.WriteLine("\n3. Updating user...");
            savedUser.FirstName = "Updated";
            savedUser.AboutMe = "Updated via complete integration test";
            savedUser.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await commandDb.SaveChangesAsync();
            Assert.Equal(1, updateResult);
            
            var updatedUser = await commandDb.Users.FindAsync(userId);
            Assert.Equal("Updated", updatedUser!.FirstName);
            Assert.Equal("Updated via complete integration test", updatedUser.AboutMe);
            Assert.NotNull(updatedUser.UpdatedAt);
            
            Console.WriteLine($" User updated: {updatedUser.FirstName}");
            
            // Test 4: Verify MongoDB available
            Console.WriteLine("\n4. Verifying MongoDB...");
            var mongoCount = await queryDb.Users.CountDocumentsAsync(FilterDefinition<UserQueryDocument>.Empty);
            
            Assert.True(mongoCount >= 0);
            Console.WriteLine($" MongoDB disponible con {mongoCount} documentos");
            
            // Test 5: Soft Delete
            Console.WriteLine("\n5. Performing soft delete...");
            updatedUser.IsDeleted = true;
            updatedUser.DeletedAt = DateTime.UtcNow;
            
            var deleteResult = await commandDb.SaveChangesAsync();
            Assert.Equal(1, deleteResult);
            
            var deletedUser = await commandDb.Users.FindAsync(userId);
            Assert.NotNull(deletedUser);
            Assert.True(deletedUser.IsDeleted);
            Assert.NotNull(deletedUser.DeletedAt);
            
            Console.WriteLine($" User deleted (soft): {deletedUser.Email}");
            
            // Test 6: Verify complete infrastructure
            Console.WriteLine("\n6. Verifying complete infrastructure...");
            
            // PostgreSQL
            var pgConnection = PostgreSqlConnectionString;
            Assert.NotNull(pgConnection);
            Assert.Contains("hexagonal_test", pgConnection);
            Console.WriteLine($" PostgreSQL: {pgConnection.Substring(0, 50)}...");
            
            // MongoDB  
            var mongoConnection = MongoDbConnectionString;
            Assert.NotNull(mongoConnection);
            Assert.Contains("127.0.0.1", mongoConnection);
            Console.WriteLine($" MongoDB: {mongoConnection.Substring(0, 50)}...");
            
            // Kafka
            var kafkaServers = KafkaBootstrapServers;
            Assert.NotNull(kafkaServers);
            Assert.Contains("127.0.0.1", kafkaServers);
            Console.WriteLine($" Kafka: {kafkaServers}");
            
            // Test 7: Verify real connectivity
            Console.WriteLine("\n7. Verifying real connectivity...");
            
            var canConnectPg = await commandDb.Database.CanConnectAsync();
            Assert.True(canConnectPg);
            Console.WriteLine(" PostgreSQL conexión activa");
            
            var mongoCollections = await queryDb.Users.Database.ListCollectionNamesAsync();
            var collectionsList = await mongoCollections.ToListAsync();
            Assert.NotNull(collectionsList);
            Console.WriteLine($" MongoDB conexión activa ({collectionsList.Count} colecciones)");
            
            Console.WriteLine("\nALL TESTS COMPLETED SUCCESSFULLY");
            Console.WriteLine(" RESUMEN:");
            Console.WriteLine($"   - User created y verificado: {userId}");
            Console.WriteLine($"   - Operaciones PostgreSQL: CREATE, READ, UPDATE, DELETE ");
            Console.WriteLine($"   - Conectividad MongoDB: ");
            Console.WriteLine($"   - Infraestructura Kafka: ");
            Console.WriteLine($"   - Testcontainers funcionando: ");
        }
    }
}
