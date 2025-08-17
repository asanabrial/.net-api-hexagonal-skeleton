using Xunit;
using HexagonalSkeleton.Infrastructure.Persistence.Command.Entities;
using Microsoft.Extensions.DependencyInjection;
using HexagonalSkeleton.Infrastructure.Persistence.Command;
using HexagonalSkeleton.Test.Integration.Infrastructure;
using System;
using System.Threading.Tasks;

namespace HexagonalSkeleton.Test.Integration.UserProfile
{
    /// <summary>
    /// REAL integration tests for user operations
    /// Using real containers without mocks
    /// </summary>
    [Collection("Integration Collection")]
    public class UserOperationsIntegrationTest : BaseIntegrationTest
    {
        [Fact]
        public async Task CreateUser_ShouldPersist_InPostgreSQL()
        {
            // Arrange
            Console.WriteLine("REAL Test: Create user in PostgreSQL");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            
            var userId = Guid.NewGuid();
            var user = new UserCommandEntity
            {
                Id = userId,
                Email = "real.user@integration.com",
                FirstName = "Real",
                LastName = "User",
                PhoneNumber = "+1234567890",
                PasswordHash = "hashed_password_123",
                PasswordSalt = "salt_123",
                Birthdate = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Latitude = 40.7128,
                Longitude = -74.0060,
                AboutMe = "Real integration test user",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            commandDb.Users.Add(user);
            var result = await commandDb.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            
            // Verify that it persisted correctly
            var savedUser = await commandDb.Users.FindAsync(userId);
            Assert.NotNull(savedUser);
            Assert.Equal("real.user@integration.com", savedUser.Email);
            Assert.Equal("Real", savedUser.FirstName);
            Assert.Equal("User", savedUser.LastName);
            Assert.Equal("+1234567890", savedUser.PhoneNumber);
            Assert.False(savedUser.IsDeleted);
            
            Console.WriteLine($" User created successfully: {savedUser.Id}");
        }

        [Fact]
        public async Task UpdateUser_ShouldModify_ExistingRecord()
        {
            // Arrange
            Console.WriteLine("REAL Test: Update existing user");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            
            // Create initial user
            var userId = Guid.NewGuid();
            var user = new UserCommandEntity
            {
                Id = userId,
                Email = "update.test@integration.com",
                FirstName = "Original",
                LastName = "Name",
                PhoneNumber = "+9876543210",
                PasswordHash = "original_hash",
                PasswordSalt = "original_salt",
                Birthdate = new DateTime(1985, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                Latitude = 41.8781,
                Longitude = -87.6298,
                AboutMe = "Original description",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            commandDb.Users.Add(user);
            await commandDb.SaveChangesAsync();

                        // Act - Update user
            user.FirstName = "Updated";
            user.LastName = "NewName";
            user.AboutMe = "Updated description";
            user.UpdatedAt = DateTime.UtcNow;
            
            var updateResult = await commandDb.SaveChangesAsync();

            // Assert
            Assert.Equal(1, updateResult);
            
            // Verify update
            var updatedUser = await commandDb.Users.FindAsync(userId);
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
            Assert.Equal("NewName", updatedUser.LastName);
            Assert.Equal("Updated description", updatedUser.AboutMe);
            Assert.NotNull(updatedUser.UpdatedAt);
            
            Console.WriteLine($" User updated: {updatedUser.FirstName} {updatedUser.LastName}");
        }

        [Fact]
        public async Task DeleteUser_ShouldMarkAsDeleted_SoftDelete()
        {
            // Arrange
            Console.WriteLine("REAL Test: Soft delete user");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            
            // Create user
            var userId = Guid.NewGuid();
            var user = new UserCommandEntity
            {
                Id = userId,
                Email = "delete.test@integration.com",
                FirstName = "ToDelete",
                LastName = "User",
                PhoneNumber = "+5555555555",
                PasswordHash = "delete_hash",
                PasswordSalt = "delete_salt",
                Birthdate = new DateTime(1992, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                Latitude = 34.0522,
                Longitude = -118.2437,
                AboutMe = "User to be deleted",
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            
            commandDb.Users.Add(user);
            await commandDb.SaveChangesAsync();

            // Act - Delete (soft delete)
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            
            var deleteResult = await commandDb.SaveChangesAsync();

            // Assert
            Assert.Equal(1, deleteResult);
            
            // Verify that it still exists but marked as deleted
            var deletedUser = await commandDb.Users.FindAsync(userId);
            Assert.NotNull(deletedUser);
            Assert.True(deletedUser.IsDeleted);
            Assert.NotNull(deletedUser.DeletedAt);
            Assert.Equal("delete.test@integration.com", deletedUser.Email);
            
            Console.WriteLine($" User deleted (soft delete): {deletedUser.Email}");
        }

        [Fact]
        public async Task CreateMultipleUsers_ShouldPersist_AllRecords()
        {
            // Arrange
            Console.WriteLine("REAL Test: Create multiple users");
            
            using var scope = CreateScope();
            var commandDb = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
            
            var users = new[]
            {
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user1@batch.com",
                    FirstName = "User",
                    LastName = "One",
                    PhoneNumber = "+1111111111",
                    PasswordHash = "hash1",
                    PasswordSalt = "salt1",
                    Birthdate = new DateTime(1988, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 25.7617,
                    Longitude = -80.1918,
                    AboutMe = "First batch user",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user2@batch.com",
                    FirstName = "User",
                    LastName = "Two",
                    PhoneNumber = "+2222222222",
                    PasswordHash = "hash2",
                    PasswordSalt = "salt2",
                    Birthdate = new DateTime(1995, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 39.9526,
                    Longitude = -75.1652,
                    AboutMe = "Second batch user",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                },
                new UserCommandEntity
                {
                    Id = Guid.NewGuid(),
                    Email = "user3@batch.com",
                    FirstName = "User",
                    LastName = "Three",
                    PhoneNumber = "+3333333333",
                    PasswordHash = "hash3",
                    PasswordSalt = "salt3",
                    Birthdate = new DateTime(1982, 12, 25, 0, 0, 0, DateTimeKind.Utc),
                    Latitude = 47.6062,
                    Longitude = -122.3321,
                    AboutMe = "Third batch user",
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Act
            commandDb.Users.AddRange(users);
            var result = await commandDb.SaveChangesAsync();

            // Assert
            Assert.Equal(3, result);
            
            // Verify that all were created
            foreach (var user in users)
            {
                var savedUser = await commandDb.Users.FindAsync(user.Id);
                Assert.NotNull(savedUser);
                Assert.Equal(user.Email, savedUser.Email);
                Assert.False(savedUser.IsDeleted);
            }
            
            Console.WriteLine($" {users.Length} users created en lote successfully");
        }
    }
}
