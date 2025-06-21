using Xunit;
using Moq;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Domain.Ports;
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
        var user = TestHelper.CreateTestUser();

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user, user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanUserUpdateProfile_DifferentUsers_ShouldReturnFalse()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser(id: 1);
        var user2 = TestHelper.CreateTestUser(id: 2);

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user1, user2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanUserUpdateProfile_DeletedUser_ShouldReturnFalse()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        user.Delete();

        // Act
        var result = UserDomainService.CanUserUpdateProfile(user, user);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CanUsersInteract Tests - SRP Violation: Combines deletion check + age validation

    [Fact]
    public void CanUsersInteract_BothAdultAndActive_ShouldReturnTrue()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "other@example.com", firstName: "Jane", lastName: "Smith",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 41.0, longitude: -75.0, aboutMe: "Other user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.CanUsersInteract(user1, user2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanUsersInteract_OneUserDeleted_ShouldReturnFalse()
    {
        // Arrange
        var activeUser = TestHelper.CreateTestUser();
        var deletedUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "deleted@example.com", firstName: "Jane", lastName: "Smith",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 41.0, longitude: -75.0, aboutMe: "Deleted user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, 
            deletedAt: DateTime.UtcNow, isDeleted: true);

        // Act
        var result = UserDomainService.CanUsersInteract(activeUser, deletedUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanUsersInteract_OneUserMinor_ShouldReturnFalse()
    {
        // Arrange
        var adultUser = TestHelper.CreateTestUser();
        var minorUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "minor@example.com", firstName: "Teen", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-16), phoneNumber: "+9876543210",
            latitude: 41.0, longitude: -75.0, aboutMe: "Minor user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.CanUsersInteract(adultUser, minorUser);

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

    #region IsProfileComplete Tests - Single Responsibility: Profile completeness check only

    [Fact]
    public void IsProfileComplete_CompleteProfile_ShouldReturnTrue()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act
        var result = UserDomainService.IsProfileComplete(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsProfileComplete_IncompleteProfile_ShouldReturnFalse()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();
        // The test helper creates users with AboutMe set, so we need to create one manually without it
        var incompleteUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 1,
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            birthdate: DateTime.UtcNow.AddYears(-25),
            phoneNumber: "+1234567890",
            latitude: 40.7128,
            longitude: -74.0060,
            aboutMe: "", // Empty AboutMe
            passwordSalt: "salt",
            passwordHash: "hash",
            lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null,
            isDeleted: false);

        // Act
        var result = UserDomainService.IsProfileComplete(incompleteUser);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region FindNearbyUsers Tests - SRP Violation: Combines filtering + geographical logic

    [Fact]
    public void FindNearbyUsers_WithUsersInRange_ShouldReturnNearbyUsers()
    {
        // Arrange
        var centerUser = TestHelper.CreateTestUser();
        var nearbyUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "nearby@example.com", firstName: "Near", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 40.7130, longitude: -74.0062, aboutMe: "Nearby user", // Very close coordinates
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);
        
        var farUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 3, email: "far@example.com", firstName: "Far", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-25), phoneNumber: "+1111111111",
            latitude: 50.0, longitude: -80.0, aboutMe: "Far user", // Far coordinates
            passwordSalt: "salt3", passwordHash: "hash3", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        var allUsers = new[] { centerUser, nearbyUser, farUser };

        // Act
        var result = UserDomainService.FindNearbyUsers(centerUser, allUsers, 1.0); // 1km radius

        // Assert
        Assert.Single(result);
        Assert.Equal(nearbyUser.Id, result.First().Id);
    }

    [Fact]
    public void FindNearbyUsers_DeletedCenterUser_ShouldReturnEmpty()
    {
        // Arrange
        var deletedUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 1, email: "deleted@example.com", firstName: "Deleted", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-25), phoneNumber: "+1234567890",
            latitude: 40.7128, longitude: -74.0060, aboutMe: "Deleted user",
            passwordSalt: "salt", passwordHash: "hash", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, 
            deletedAt: DateTime.UtcNow, isDeleted: true);
        
        var otherUser = TestHelper.CreateTestUser();
        var allUsers = new[] { deletedUser, otherUser };

        // Act
        var result = UserDomainService.FindNearbyUsers(deletedUser, allUsers, 1000.0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindNearbyUsers_ExcludesDeletedUsers_ShouldFilterCorrectly()
    {
        // Arrange
        var centerUser = TestHelper.CreateTestUser();
        var deletedNearbyUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "deleted@example.com", firstName: "Deleted", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 40.7130, longitude: -74.0062, aboutMe: "Deleted nearby user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, 
            deletedAt: DateTime.UtcNow, isDeleted: true);

        var allUsers = new[] { centerUser, deletedNearbyUser };

        // Act
        var result = UserDomainService.FindNearbyUsers(centerUser, allUsers, 1000.0);

        // Assert
        Assert.Empty(result); // Should exclude deleted users
    }

    #endregion

    #region CalculateCompatibilityScore Tests - SRP Violation: Combines age, geography, and profile logic

    [Fact]
    public void CalculateCompatibilityScore_CannotInteract_ShouldReturnZero()
    {
        // Arrange
        var adultUser = TestHelper.CreateTestUser();
        var minorUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "minor@example.com", firstName: "Teen", lastName: "User",
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
        var user1 = TestHelper.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "match@example.com", firstName: "Perfect", lastName: "Match",
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
            id: 1, email: "young@example.com", firstName: "Young", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-20), phoneNumber: "+1234567890",
            latitude: 40.7128, longitude: -74.0060, aboutMe: "", // No AboutMe
            passwordSalt: "salt", passwordHash: "hash", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);
        
        var oldUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "old@example.com", firstName: "Old", lastName: "User",
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
        var user = TestHelper.CreateTestUser();
        
        // We can't test with the same user due to ID check in CanUsersInteract
        // So we create a duplicate with different ID
        var duplicateUser = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: user.Email.Value, firstName: user.FullName.FirstName, 
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

    #region Geographical Distance Tests - SRP Violation: Duplicate distance calculation logic

    [Fact]
    public void CalculateDistanceBetweenUsers_ValidUsers_ShouldReturnDistance()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "other@example.com", firstName: "Other", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 41.0, longitude: -75.0, aboutMe: "Other user",
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var distance = UserDomainService.CalculateDistanceBetweenUsers(user1, user2);

        // Assert
        Assert.True(distance > 0);
    }

    [Fact]
    public void CalculateDistanceBetweenUsers_NullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = TestHelper.CreateTestUser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            UserDomainService.CalculateDistanceBetweenUsers(user, null!));
    }

    [Fact]
    public void AreUsersNearby_WithinDefaultRadius_ShouldReturnTrue()
    {
        // Arrange - Create users very close to each other
        var user1 = TestHelper.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "nearby@example.com", firstName: "Near", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 40.7130, longitude: -74.0062, aboutMe: "Nearby user", // Very close
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.AreUsersNearby(user1, user2);

        // Assert
        Assert.True(result); // Should be within default 50km radius
    }

    [Fact]
    public void AreUsersNearby_OutsideCustomRadius_ShouldReturnFalse()
    {
        // Arrange
        var user1 = TestHelper.CreateTestUser();
        var user2 = HexagonalSkeleton.Domain.User.Reconstitute(
            id: 2, email: "far@example.com", firstName: "Far", lastName: "User",
            birthdate: DateTime.UtcNow.AddYears(-30), phoneNumber: "+9876543210",
            latitude: 50.0, longitude: -80.0, aboutMe: "Far user", // Far coordinates
            passwordSalt: "salt2", passwordHash: "hash2", lastLogin: DateTime.UtcNow,
            createdAt: DateTime.UtcNow, updatedAt: null, deletedAt: null, isDeleted: false);

        // Act
        var result = UserDomainService.AreUsersNearby(user1, user2, 1.0); // 1km radius

        // Assert
        Assert.False(result);
    }

    #endregion
}
