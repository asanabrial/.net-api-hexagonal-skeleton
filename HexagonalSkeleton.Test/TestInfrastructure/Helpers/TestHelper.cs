using HexagonalSkeleton.Application.Features.UserAuthentication.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Test.TestInfrastructure.Helpers;

/// <summary>
/// Helper class for creating test data and commands for unit and integration tests.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Creates a test User entity for testing
    /// </summary>
    public static User CreateTestUser(
        Guid? id = null,
        string? email = null,
        string firstName = "John",
        string lastName = "Doe",
        string phoneNumber = "+1234567890",
        double latitude = 40.7128,
        double longitude = -74.0060,
        DateTime? birthdate = null,
        bool isActive = true,
        string? profileImageName = null)
    {
        var actualEmail = email ?? $"test.user.{Guid.NewGuid():N}@example.com";
        var actualBirthdate = birthdate ?? DateTime.Now.AddYears(-25);
        return User.Reconstitute(
            id: id ?? Guid.NewGuid(),
            email: actualEmail,
            firstName: firstName,
            lastName: lastName,
            birthdate: actualBirthdate,
            phoneNumber: phoneNumber,
            latitude: latitude,
            longitude: longitude,
            aboutMe: "Test about me",
            passwordSalt: "test-salt",
            passwordHash: "test-hash",
            lastLogin: DateTime.Now.AddDays(-1),
            createdAt: DateTime.Now.AddDays(-30),
            updatedAt: null,
            deletedAt: null,
            isDeleted: !isActive,
            profileImageName: profileImageName
        );
    }

    /// <summary>
    /// Creates a RegisterUserCommand for testing
    /// </summary>
    public static RegisterUserCommand CreateRegisterUserCommand(
        string? email = null,
        string name = "John",
        string surname = "Doe",
        string phoneNumber = "+1234567890",
        double latitude = 40.7128,
        double longitude = -74.0060,
        DateOnly? birthdate = null,
        string password = "TestPassword123!",
        string passwordConfirmation = "TestPassword123!",
        string aboutMe = "Test about me")
    {
        var actualEmail = email ?? $"test.user.{Guid.NewGuid():N}@example.com";
        return new RegisterUserCommand
        {
            Email = actualEmail,
            FirstName = name,
            LastName = surname,
            PhoneNumber = phoneNumber,
            Latitude = latitude,
            Longitude = longitude,
            Birthdate = birthdate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now.AddYears(-25),
            Password = password,
            PasswordConfirmation = passwordConfirmation,
            AboutMe = aboutMe
        };
    }

    /// <summary>
    /// Creates a valid RegisterUserCommand for testing
    /// </summary>
    public static RegisterUserCommand CreateValidRegisterUserCommand(
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        double latitude,
        double longitude,
        DateOnly birthdate,
        string password = "TestPassword123!",
        string aboutMe = "Test about me")
    {
        return CreateRegisterUserCommand(email, firstName, lastName, phoneNumber, latitude, longitude, birthdate, password, password, aboutMe);
    }

    /// <summary>
    /// Creates a valid RegisterUserCommand for testing
    /// </summary>
    public static RegisterUserCommand CreateValidRegisterUserCommand(
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        double latitude,
        double longitude,
        DateTime birthdate,
        string password = "TestPassword123!")
    {
        return CreateRegisterUserCommand(email, firstName, lastName, phoneNumber, latitude, longitude, DateOnly.FromDateTime(birthdate), password);
    }

    /// <summary>
    /// Creates a valid UpdateProfileUserCommand for testing
    /// </summary>
    public static UpdateProfileUserCommand CreateValidUpdateProfileCommand(
        Guid userId,
        string firstName = "UpdatedJohn",
        string lastName = "UpdatedDoe",
        DateTime? birthdate = null,
        string aboutMe = "Updated about me")
    {
        return new UpdateProfileUserCommand
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            Birthdate = birthdate ?? DateTime.Now.AddYears(-25),
            AboutMe = aboutMe
        };
    }

    /// <summary>
    /// Creates an UpdateProfileUserCommand for testing
    /// </summary>
    public static UpdateProfileUserCommand CreateUpdateProfileUserCommand(
        Guid? userId = null,
        string firstName = "UpdatedJohn",
        string lastName = "UpdatedDoe",
        DateTime? birthdate = null,
        string aboutMe = "Updated about me")
    {
        return CreateValidUpdateProfileCommand(userId ?? Guid.NewGuid(), firstName, lastName, birthdate, aboutMe);
    }

    /// <summary>
    /// Creates a valid GetUserQuery for testing
    /// </summary>
    public static GetUserQuery CreateGetUserQuery(Guid userId)
    {
        return new GetUserQuery { Id = userId };
    }

    /// <summary>
    /// Creates a SoftDeleteUserCommand for testing
    /// </summary>
    public static SoftDeleteUserCommand CreateSoftDeleteUserCommand(Guid userId)
    {
        return new SoftDeleteUserCommand { Id = userId };
    }

    /// <summary>
    /// Creates a SoftDeleteUserCommand for testing
    /// </summary>
    public static SoftDeleteUserCommand CreateSoftDeleteUserCommand()
    {
        return new SoftDeleteUserCommand { Id = Guid.NewGuid() };
    }

    /// <summary>
    /// Creates a HardDeleteUserCommand for testing
    /// </summary>
    public static HardDeleteUserCommand CreateHardDeleteUserCommand(Guid userId)
    {
        return new HardDeleteUserCommand { Id = userId };
    }

    /// <summary>
    /// Creates a HardDeleteUserCommand for testing
    /// </summary>
    public static HardDeleteUserCommand CreateHardDeleteUserCommand()
    {
        return new HardDeleteUserCommand { Id = Guid.NewGuid() };
    }
}
