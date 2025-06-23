using HexagonalSkeleton.Application.Command;
using HexagonalSkeleton.Application.Query;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Helper class for creating test objects to reduce code duplication
    /// </summary>
    public static class TestHelper
    {        public static User CreateTestUser(
            int id = 1,
            string email = "test@example.com",
            string firstName = "John",
            string lastName = "Doe",
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060,
            DateTime? birthdate = null)
        {
            return User.Reconstitute(
                id: id,
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

        public static GetUserQuery CreateGetUserQuery(int id = 1)
        {
            return new GetUserQuery(id);
        }

        public static GetAllUsersQuery CreateGetAllUsersQuery()        {
            return new GetAllUsersQuery();
        }

        public static UpdateProfileUserCommand CreateUpdateProfileUserCommand(
            int id = 1,
            string firstName = "John",  
            string lastName = "Doe",
            string aboutMe = "Updated about me")
        {
            return new UpdateProfileUserCommand(
                id: id,
                aboutMe: aboutMe,
                firstName: firstName,
                lastName: lastName,
                birthdate: DateTime.UtcNow.AddYears(-25));
        }

        public static SoftDeleteUserCommand CreateSoftDeleteUserCommand(int id = 1)
        {
            return new SoftDeleteUserCommand(id);
        }

        public static HardDeleteUserCommand CreateHardDeleteUserCommand(int id = 1)
        {  
            return new HardDeleteUserCommand(id);
        }
    }
}
