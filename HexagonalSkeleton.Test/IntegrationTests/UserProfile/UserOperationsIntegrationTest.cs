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
    }
}
