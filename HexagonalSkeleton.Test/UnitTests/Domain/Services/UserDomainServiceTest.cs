using Xunit;
using HexagonalSkeleton.Test.TestHelpers;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Domain.Exceptions;

namespace HexagonalSkeleton.Test.Unit.User.Domain.Services;

public class UserDomainServiceTest
{
    #region CreateUser Tests - SRP Violation: Combines validation + creation

    [Theory]
    [InlineData("test@example.com", "John", "Doe")]
    [InlineData("maria@example.com", "María", "González")]
    public void CreateUser_ValidData_ShouldCreateUserSuccessfully(string email, string firstName, string lastName)
    {
        // Arrange
        var passwordSalt = "salt";
        var passwordHash = "hash";
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var phoneNumber = "+1234567890";
        var latitude = 40.7128;
        var longitude = -74.0060;
        var aboutMe = "Test about me";

        // Act
        var user = UserDomainService.CreateUser(
            email, passwordSalt, passwordHash, firstName, lastName,
            birthdate, phoneNumber, latitude, longitude, aboutMe);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(email.ToLowerInvariant(), user.Email.Value);
        Assert.Equal(firstName.Trim(), user.FullName.FirstName);
        Assert.Equal(lastName.Trim(), user.FullName.LastName);
        Assert.Equal(phoneNumber, user.PhoneNumber.Value);
    }

    [Theory]
    [InlineData("", "John", "Doe")]
    [InlineData("test@example.com", "", "Doe")]
    [InlineData("test@example.com", "John", "")]
    public void CreateUser_InvalidData_ShouldThrowArgumentException(string email, string firstName, string lastName)
    {
        // Arrange
        var passwordSalt = "salt";
        var passwordHash = "hash";
        var birthdate = DateTime.UtcNow.AddYears(-25);
        var phoneNumber = "+1234567890";
        var latitude = 40.7128;
        var longitude = -74.0060;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserDomainService.CreateUser(
            email, passwordSalt, passwordHash, firstName, lastName,
            birthdate, phoneNumber, latitude, longitude));
    }

    [Fact]
    public void CreateUser_EmptyFirstName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            UserDomainService.CreateUser(
                "test@example.com", "salt", "hash", "", "Doe",
                DateTime.UtcNow.AddYears(-25), "+1234567890", 40.7128, -74.0060));

        Assert.Contains("First name is required", exception.Message);
    }

    [Fact]
    public void CreateUser_EmptyLastName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            UserDomainService.CreateUser(
                "test@example.com", "salt", "hash", "John", "",
                DateTime.UtcNow.AddYears(-25), "+1234567890", 40.7128, -74.0060));

        Assert.Contains("Last name is required", exception.Message);
    }

    [Fact]
    public void CreateUser_InvalidBusinessEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            UserDomainService.CreateUser(
                "test@tempmail.com", "salt", "hash", "John", "Doe",
                DateTime.UtcNow.AddYears(-25), "+1234567890", 40.7128, -74.0060));

        Assert.Contains("Email does not meet business requirements", exception.Message);
    }

    #endregion

    #region CanUserUpdateProfile Tests - SRP Violation: Combines authorization + state validation

    [Fact]
    public void CanUserUpdateProfile_SameUser_ShouldReturnTrue()
    {
        // Arrange
        var user = UserTestDataBuilder.CreateTestUser();

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user, user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanUserUpdateProfile_DifferentUsers_ShouldReturnFalse()
    {
        // Arrange
        var user1 = UserTestDataBuilder.CreateTestUser(id: Guid.NewGuid());
        var user2 = UserTestDataBuilder.CreateTestUser(id: Guid.NewGuid());

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user1, user2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanUserUpdateProfile_DeletedUser_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestDataBuilder.CreateTestUser();
        user.Delete();

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user, user);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsPasswordStrong Tests - Single Responsibility: Password validation only

    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("Password123", false)] // No special char
    [InlineData("password123!", false)] // No uppercase
    [InlineData("PASSWORD123!", false)] // No lowercase
    [InlineData("Password!", false)] // No digit
    [InlineData("Pass1!", false)] // Too short
    [InlineData("", false)] // Empty
    [InlineData(" ", false)] // Whitespace
    public void IsPasswordStrong_VariousPasswords_ShouldReturnExpectedResult(string password, bool expected)
    {
        // Act
        var result = UserDomainService.IsPasswordStrong(password);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ValidatePasswordStrength Tests - Single Responsibility: Password validation with exception

    [Fact]
    public void ValidatePasswordStrength_StrongPassword_ShouldNotThrowException()
    {
        // Act & Assert - Should not throw
        UserDomainService.ValidatePasswordStrength("Password123!");
    }

    [Fact]
    public void ValidatePasswordStrength_WeakPassword_ShouldThrowWeakPasswordException()
    {
        // Act & Assert
        Assert.Throws<WeakPasswordException>(() =>
            UserDomainService.ValidatePasswordStrength("weak"));
    }

    #endregion

    #region IsValidBusinessEmail Tests - Single Responsibility: Email validation only

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user@gmail.com", true)]
    [InlineData("user@tempmail.com", false)] // Blocked domain
    [InlineData("user@10minutemail.com", false)] // Blocked domain
    [InlineData("invalid-email", false)] // Invalid format
    public void IsValidBusinessEmail_VariousEmails_ShouldReturnExpectedResult(string email, bool expected)
    {
        // Act
        var result = UserDomainService.IsValidBusinessEmail(email);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ValidateUserUniqueness Tests - Single Responsibility: Uniqueness validation only

    [Fact]
    public void ValidateUserUniqueness_UniqueData_ShouldNotThrowException()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";

        // Act & Assert - Should not throw
        UserDomainService.ValidateUserUniqueness(false, false, email, phoneNumber);
    }

    [Fact]
    public void ValidateUserUniqueness_EmailExists_ShouldThrowUserDataNotUniqueException()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";

        // Act & Assert
        var exception = Assert.Throws<UserDataNotUniqueException>(() =>
            UserDomainService.ValidateUserUniqueness(true, false, email, phoneNumber));

        Assert.Equal(email, exception.Email);
        Assert.Equal(phoneNumber, exception.PhoneNumber);
    }

    [Fact]
    public void ValidateUserUniqueness_PhoneExists_ShouldThrowUserDataNotUniqueException()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";

        // Act & Assert
        var exception = Assert.Throws<UserDataNotUniqueException>(() =>
            UserDomainService.ValidateUserUniqueness(false, true, email, phoneNumber));

        Assert.Equal(email, exception.Email);
        Assert.Equal(phoneNumber, exception.PhoneNumber);
    }

    #endregion

    #region CalculateCompatibilityScore Tests - SRP Violation: Combines age, geography, and profile logic

    [Fact]
    public void CalculateCompatibilityScore_CannotInteract_ShouldReturnZero()
    {
        // Arrange
        var adultUser = UserTestDataBuilder.CreateTestUser();
        var minorUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: Guid.NewGuid(), email: "minor@example.com", firstName: "Teen", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-16), phoneNumber: "+9876543210",
            latitude: 40.7130, longitude: -74.0062, aboutMe: "Minor user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.CalculateCompatibilityScore(adultUser, minorUser);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateCompatibilityScore_PerfectMatch_ShouldReturnHighScore()
    {
        // Arrange
        var user1 = UserTestDataBuilder.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: Guid.NewGuid(), email: "match@example.com", firstName: "Perfect", lastName: "Match",
            birthdate: user1.Birthdate!.Value.AddYears(2), // Within 5 years
            phoneNumber: "+9876543210",
            latitude: 40.7130, longitude: -74.0062, // Very close coordinates (within 10km)
            aboutMe: "Complete profile", // Both have AboutMe
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.CalculateCompatibilityScore(user1, user2);

        // Assert
        Assert.True(result >= 75); // Age (20) + Distance (30) + Profile (25) = 75+
    }

    [Fact]
    public void CalculateCompatibilityScore_NoCommonalities_ShouldReturnLowScore()
    {
        // Arrange
        var youngUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: Guid.NewGuid(), email: "young@example.com", firstName: "Young", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-20), phoneNumber: "+1234567890",
            latitude: 40.7128, longitude: -74.0060, aboutMe: "", // No AboutMe
            passwordSalt: "salt", passwordHash: "hash", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);
        
        var oldUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: Guid.NewGuid(), email: "old@example.com", firstName: "Old", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-60), phoneNumber: "+9876543210", // Large age gap
            latitude: 50.0, longitude: -80.0, aboutMe: "", // Far away, no AboutMe
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.CalculateCompatibilityScore(youngUser, oldUser);

        // Assert
        Assert.Equal(0, result); // No points for age, distance, or profile
    }

    [Fact]
    public void CalculateCompatibilityScore_ScoreCappedAt100_ShouldNotExceedMaximum()
    {
        // This test ensures the score doesn't exceed 100 even with perfect matches
        // Act with same user (perfect match in all criteria)
        var user = UserTestDataBuilder.CreateTestUser();
        
        // We can't test with the same user due to ID check in CanUsersInteract
        // So we create a duplicate with different ID
        var duplicateUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: Guid.NewGuid(), email: user.Email.Value, firstName: user.FullName.FirstName, 
            lastName: user.FullName.LastName, birthdate: user.Birthdate!.Value,
            phoneNumber: "+9876543210", latitude: user.Location.Latitude, 
            longitude: user.Location.Longitude, aboutMe: user.AboutMe,
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        var result = UserDomainService.CalculateCompatibilityScore(user, duplicateUser);

        // Assert
        Assert.True(result <= 100);
    }

    #endregion
}
