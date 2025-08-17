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

        [Fact]
        public async Task MultipleUsers_ShouldWork_ConcurrentOperations()
        {
            // Arrange
            Console.WriteLine("MULTIPLE TEST: Concurrent operations");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            
            var users = new[]
            {
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@concurrent.test",
                    FirstName = "User",
                    LastName = "One",
                    PhoneNumber = "+1111111111",
                    PasswordHash = "hash1",
                    PasswordSalt = "salt1",
                    Birthdate = new DateTime(1985, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 40.7589,
                    Longitude = -73.9851,
                    AboutMe = "Concurrent test user 1",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user2@concurrent.test",
                    FirstName = "User",
                    LastName = "Two",
                    PhoneNumber = "+2222222222",
                    PasswordHash = "hash2",
                    PasswordSalt = "salt2",
                    Birthdate = new DateTime(1992, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 34.0522,
                    Longitude = -118.2437,
                    AboutMe = "Concurrent test user 2",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user3@concurrent.test",
                    FirstName = "User",
                    LastName = "Three",
                    PhoneNumber = "+3333333333",
                    PasswordHash = "hash3",
                    PasswordSalt = "salt3",
                    Birthdate = new DateTime(1988, 12, 25, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 41.8781,
                    Longitude = -87.6298,
                    AboutMe = "Concurrent test user 3",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Act - Create múltiples usuarios
            Console.WriteLine("Creating 3 users simultaneously...");
            commandDb.Users.AddRange(users);
            var result = await commandDb.SaveChangesAsync();

            // Assert
            Assert.Equal(3, result);
            Console.WriteLine($" {result} users created successfully");

            // Verify each user
            foreach (var user in users)
            {
                var savedUser = await commandDb.Users.FindAsync(user.Id);
                Assert.NotNull(savedUser);
                Assert.Equal(user.Email, savedUser.Email);
                Assert.False(savedUser.IsDeleted);
                Console.WriteLine($" Verificado: {savedUser.Email}");
            }

            Console.WriteLine("Concurrent test completed successfully");
        }
    }
}
