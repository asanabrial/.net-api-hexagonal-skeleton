using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Test.TestData;

namespace HexagonalSkeleton.Test.TestHelpers;

public static class CommandTestDataBuilder
{
    public static RegisterUserCommand CreateRegisterUserCommand(
        string? email = null,
        string? password = null,
        string? passwordConfirmation = null,
        string name = "John",
        string surname = "Doe",
        DateOnly? birthdate = null,
        string? phoneNumber = null,
        double? latitude = null,
        double? longitude = null,
        string aboutMe = "Test about me")
    {
        var actualEmail = email ?? $"test.user.{Guid.NewGuid():N}@example.com";
        var actualPassword = password ?? DefaultUserTestData.User.DefaultPassword;

        return new RegisterUserCommand
        {
            Email = actualEmail,
            Password = actualPassword,
            PasswordConfirmation = passwordConfirmation ?? actualPassword,
            FirstName = name,
            LastName = surname,
            Birthdate = birthdate?.ToDateTime(TimeOnly.MinValue) ?? DefaultUserTestData.User.DefaultBirthdate,
            PhoneNumber = phoneNumber ?? DefaultUserTestData.User.PhoneNumber,
            Latitude = latitude ?? DefaultUserTestData.User.Latitude,
            Longitude = longitude ?? DefaultUserTestData.User.Longitude,
            AboutMe = aboutMe
        };
    }

    public static RegisterUserCommand CreateValidRegisterUserCommand(
        string? email = null,
        string firstName = "John",
        string lastName = "Doe",
        DateTime? birthdate = null,
        string? phoneNumber = null,
        string aboutMe = "Test about me")
    {
        var actualEmail = email ?? $"test.{Guid.NewGuid():N}@example.com";

        return new RegisterUserCommand
        {
            Email = actualEmail,
            Password = DefaultUserTestData.User.DefaultPassword,
            PasswordConfirmation = DefaultUserTestData.User.DefaultPassword,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber ?? DefaultUserTestData.User.PhoneNumber,
            Latitude = DefaultUserTestData.User.Latitude,
            Longitude = DefaultUserTestData.User.Longitude,
            Birthdate = birthdate ?? new DateTime(1995, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            AboutMe = aboutMe
        };
    }

    public static UpdateProfileUserCommand CreateValidUpdateProfileCommand(
        Guid? userId = null,
        string firstName = "UpdatedJohn",
        string lastName = "UpdatedDoe",
        DateTime? birthdate = null,
        string aboutMe = "Updated about me")
    {
        return new UpdateProfileUserCommand
        {
            Id = userId ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Birthdate = birthdate ?? new DateTime(1965, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AboutMe = aboutMe
        };
    }

    public static GetUserQuery CreateGetUserQuery(Guid userId)
    {
        return new GetUserQuery { Id = userId };
    }

    public static SoftDeleteUserManagementCommand CreateSoftDeleteUserManagementCommand(Guid? userId = null)
    {
        return new SoftDeleteUserManagementCommand
        {
            Id = userId ?? Guid.NewGuid()
        };
    }

    public static HardDeleteUserManagementCommand CreateHardDeleteUserManagementCommand(Guid? userId = null)
    {
        return new HardDeleteUserManagementCommand
        {
            Id = userId ?? Guid.NewGuid()
        };
    }
}
