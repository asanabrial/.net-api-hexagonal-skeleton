using Xunit;
using HexagonalSkeleton.Domain.Specifications.Users;
using HexagonalSkeleton.Test;
using HexagonalSkeleton.Test.TestInfrastructure.Helpers;
using DomainUser = HexagonalSkeleton.Domain.User;

namespace HexagonalSkeleton.Test.Unit.Domain.Specifications
{
    /// <summary>
    /// Tests for User specifications demonstrating the Specification pattern
    /// Shows how specifications can be used for complex filtering logic
    /// </summary>
    public class UserSpecificationTest
    {
        [Fact]
        public void ActiveUserSpecification_WithActiveUser_ShouldReturnTrue()
        {
            // Arrange
            var user = TestHelper.CreateTestUser(); // Active user by default
            var specification = new ActiveUserSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ActiveUserSpecification_WithDeletedUser_ShouldReturnFalse()
        {
            // Arrange
            var user = TestHelper.CreateTestUser();
            user.Delete(); // Soft delete the user
            var specification = new ActiveUserSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UserEmailSpecification_WithMatchingEmail_ShouldReturnTrue()
        {
            // Arrange
            var user = TestHelper.CreateTestUser();
            var specification = new UserEmailSpecification(user.Email.Value);

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserEmailSpecification_WithDifferentEmail_ShouldReturnFalse()
        {
            // Arrange
            var user = TestHelper.CreateTestUser();
            var specification = new UserEmailSpecification("different@example.com");

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UserTextSearchSpecification_WithMatchingName_ShouldReturnTrue()
        {
            // Arrange
            var user = TestHelper.CreateTestUser(); // Default name is "John"
            var specification = new UserTextSearchSpecification("john");

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserTextSearchSpecification_WithMatchingEmail_ShouldReturnTrue()
        {
            // Arrange
            var user = TestHelper.CreateTestUser();
            var specification = new UserTextSearchSpecification("test");

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AdultUserSpecification_WithAdultUser_ShouldReturnTrue()
        {
            // Arrange
            var user = DomainUser.Create(
                "adult@example.com", "salt", "hash", "Adult", "User",
                DateTime.Today.AddYears(-25), // 25 years old
                "+1234567890", 40.0, -74.0);
            var specification = new AdultUserSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AdultUserSpecification_WithMinorUser_ShouldReturnFalse()
        {
            // Arrange
            var user = DomainUser.Create(
                "minor@example.com", "salt", "hash", "Minor", "User",
                DateTime.Today.AddYears(-16), // 16 years old
                "+1234567890", 40.0, -74.0);
            var specification = new AdultUserSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UserSpecificationBuilder_CombinedSpecifications_ShouldWorkCorrectly()
        {
            // Arrange
            var activeAdultUser = DomainUser.Create(
                "active.adult@example.com", "salt", "hash", "Active", "Adult",
                DateTime.Today.AddYears(-25), // 25 years old
                "+1234567890", 40.0, -74.0, "Complete about me");

            var specification = UserSpecificationBuilder.Create()
                .OnlyActive()
                .OnlyAdults()
                .WithSearchTerm("active")
                .Build();

            // Act
            var result = specification.IsSatisfiedBy(activeAdultUser);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UserSpecificationBuilder_WithAndLogic_ShouldFilterCorrectly()
        {
            // Arrange
            var user1 = DomainUser.Create(
                "adult1@example.com", "salt", "hash", "Adult", "User",
                DateTime.Today.AddYears(-25), "+1111111111", 40.0, -74.0, "About me");

            var user2 = DomainUser.Create(
                "minor@example.com", "salt", "hash", "Minor", "User",
                DateTime.Today.AddYears(-16), "+2222222222", 40.0, -74.0, "About me");

            var specification = new ActiveUserSpecification()
                .And(new AdultUserSpecification());

            // Act
            var result1 = specification.IsSatisfiedBy(user1);
            var result2 = specification.IsSatisfiedBy(user2);

            // Assert
            Assert.True(result1);  // Adult user passes
            Assert.False(result2); // Minor user fails
        }

        [Fact]
        public void UserSpecificationBuilder_WithOrLogic_ShouldFilterCorrectly()
        {
            // Arrange
            var user1 = DomainUser.Create(
                "search@example.com", "salt", "hash", "John", "Doe",
                DateTime.Today.AddYears(-16), "+1111111111", 40.0, -74.0);

            var user2 = DomainUser.Create(
                "adult@example.com", "salt", "hash", "Jane", "Smith",
                DateTime.Today.AddYears(-25), "+2222222222", 40.0, -74.0);

            var specification = new UserTextSearchSpecification("john")
                .Or(new AdultUserSpecification());

            // Act
            var result1 = specification.IsSatisfiedBy(user1); // Matches name "John"
            var result2 = specification.IsSatisfiedBy(user2); // Matches adult criteria

            // Assert
            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public void CompleteProfileSpecification_WithCompleteProfile_ShouldReturnTrue()
        {
            // Arrange
            var user = DomainUser.Create(
                "complete@example.com", "salt", "hash", "Complete", "User",
                DateTime.Today.AddYears(-25), "+1234567890", 40.0, -74.0, "Complete about me section");
            
            var specification = new CompleteProfileSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompleteProfileSpecification_WithIncompleteProfile_ShouldReturnFalse()
        {
            // Arrange
            var user = DomainUser.Create(
                "incomplete@example.com", "salt", "hash", "Incomplete", "User",
                DateTime.Today.AddYears(-25), "+1234567890", 40.0, -74.0, ""); // Empty about me
            
            var specification = new CompleteProfileSpecification();

            // Act
            var result = specification.IsSatisfiedBy(user);

            // Assert
            Assert.False(result);
        }
    }
}
