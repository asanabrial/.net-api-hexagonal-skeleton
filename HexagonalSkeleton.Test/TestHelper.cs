using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Helper class for creating test objects to reduce code duplication
    /// </summary>
    public static class TestHelper
    {        public static User CreateTestUser(
            Guid id = default,
            string email = "test@example.com",
            string firstName = "John",
            string lastName = "Doe",
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060,
            DateTime? birthdate = null)
        {
            var userId = id == default ? Guid.NewGuid() : id;
            return User.Reconstitute(
                id: userId,
                email: email,
                firstName: firstName, 
                lastName: lastName,
                birthdate: birthdate ?? DateTime.UtcNow.AddYears(-25),
                phoneNumber: phoneNumber,
                latitude: latitude,
                longitude: longitude,
                aboutMe: "Test user",
                passwordSalt: "salt",
                passwordHash: "hash",
                lastLogin: DateTime.UtcNow,
                createdAt: DateTime.UtcNow.AddDays(-30),
                updatedAt: null,
                deletedAt: null,
                isDeleted: false);
        }

        public static RegisterUserCommand CreateRegisterUserCommand(
            string email = "test@example.com",
            string password = "Password123!",
            string passwordConfirmation = "Password123!",
            string name = "John",
            string surname = "Doe",
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060)
        {
            return new RegisterUserCommand(
                email: email,
                password: password,
                passwordConfirmation: passwordConfirmation,
                name: name,
                surname: surname,
                birthdate: DateTime.UtcNow.AddYears(-25),
                phoneNumber: phoneNumber,
                latitude: latitude,
                longitude: longitude,
                aboutMe: "Test about me");
        }

        public static GetUserQuery CreateGetUserQuery(Guid id = default)
        {
            if (id == default) id = Guid.NewGuid();
            return new GetUserQuery(id);
        }

        public static GetAllUsersQuery CreateGetAllUsersQuery()        {
            return new GetAllUsersQuery();
        }

        public static UpdateProfileUserCommand CreateUpdateProfileUserCommand(
            Guid id = default,
            string firstName = "John",  
            string lastName = "Doe",
            string aboutMe = "Updated about me")
        {
            if (id == default) id = Guid.NewGuid();
            return new UpdateProfileUserCommand(
                id: id,
                aboutMe: aboutMe,
                firstName: firstName,
                lastName: lastName,
                birthdate: DateTime.UtcNow.AddYears(-25));
        }

        public static SoftDeleteUserCommand CreateSoftDeleteUserCommand(Guid? id = null)
        {
            var userId = id ?? Guid.NewGuid();
            return new SoftDeleteUserCommand(userId);
        }

        public static HardDeleteUserCommand CreateHardDeleteUserCommand(Guid? id = null)
        {  
            var userId = id ?? Guid.NewGuid();
            return new HardDeleteUserCommand(userId);
        }
    }
}
