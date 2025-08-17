using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Test.TestData;

namespace HexagonalSkeleton.Test.TestHelpers;

public static class UserTestDataBuilder
{
    public static User CreateTestUser(
        Guid? id = null,
        string? email = null,
        string firstName = "John",
        string lastName = "Doe",
        DateTime? birthdate = null,
        string? phoneNumber = null,
        double? latitude = null,
        double? longitude = null,
        string aboutMe = "Test about me",
        string passwordSalt = "test_salt",
        string passwordHash = "test_hash",
        DateTime? lastLogin = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? deletedAt = null,
        bool isDeleted = false)
    {
        var actualEmail = email ?? $"test.user.{Guid.NewGuid():N}@example.com";

        return User.Reconstitute(
            id: id ?? Guid.NewGuid(),
            email: actualEmail,
            firstName: firstName,
            lastName: lastName,
            birthdate: birthdate ?? DefaultUserTestData.User.DefaultBirthdate,
            phoneNumber: phoneNumber ?? DefaultUserTestData.User.PhoneNumber,
            latitude: latitude ?? DefaultUserTestData.User.Latitude,
            longitude: longitude ?? DefaultUserTestData.User.Longitude,
            aboutMe: aboutMe,
            passwordSalt: passwordSalt,
            passwordHash: passwordHash,
            lastLogin: lastLogin ?? DateTime.UtcNow.AddDays(-1),
            createdAt: createdAt ?? DateTime.UtcNow,
            updatedAt: updatedAt ?? DateTime.UtcNow,
            deletedAt: deletedAt,
            isDeleted: isDeleted
        );
    }
}
