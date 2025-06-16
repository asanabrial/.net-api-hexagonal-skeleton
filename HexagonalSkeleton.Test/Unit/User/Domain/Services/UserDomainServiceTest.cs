using Xunit;
using Moq;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Domain.Ports;

namespace HexagonalSkeleton.Test.Unit.User.Domain.Services;

public class UserDomainServiceTest
{
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
    }    [Fact]
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

    [Fact]
    public async Task IsUserRegistrationDataUniqueAsync_UniqueData_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";
        var mockRepository = new Mock<IUserReadRepository>();
        var cancellationToken = CancellationToken.None;

        mockRepository.Setup(r => r.ExistsByEmailAsync(email, cancellationToken))
                     .ReturnsAsync(false);
        mockRepository.Setup(r => r.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
                     .ReturnsAsync(false);

        // Act
        var result = await UserDomainService.IsUserRegistrationDataUniqueAsync(
            email, phoneNumber, mockRepository.Object, cancellationToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserRegistrationDataUniqueAsync_EmailExists_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";
        var mockRepository = new Mock<IUserReadRepository>();
        var cancellationToken = CancellationToken.None;

        mockRepository.Setup(r => r.ExistsByEmailAsync(email, cancellationToken))
                     .ReturnsAsync(true);
        mockRepository.Setup(r => r.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
                     .ReturnsAsync(false);

        // Act
        var result = await UserDomainService.IsUserRegistrationDataUniqueAsync(
            email, phoneNumber, mockRepository.Object, cancellationToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUserRegistrationDataUniqueAsync_PhoneExists_ShouldReturnFalse()
    {
        // Arrange
        var email = "test@example.com";
        var phoneNumber = "+1234567890";
        var mockRepository = new Mock<IUserReadRepository>();
        var cancellationToken = CancellationToken.None;

        mockRepository.Setup(r => r.ExistsByEmailAsync(email, cancellationToken))
                     .ReturnsAsync(false);
        mockRepository.Setup(r => r.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
                     .ReturnsAsync(true);

        // Act
        var result = await UserDomainService.IsUserRegistrationDataUniqueAsync(
            email, phoneNumber, mockRepository.Object, cancellationToken);

        // Assert
        Assert.False(result);
    }

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
}
