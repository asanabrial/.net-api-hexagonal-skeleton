using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;

namespace HexagonalSkeleton.Test
{
    /// <summary>
    /// Helper class for creating test data and objects
    /// Centralizes test data creation to ensure consistency across tests
    /// </summary>
    public static class TestHelper
    {
        #region User Domain Entity Creation

        /// <summary>
        /// Creates a test user with valid domain data for unit testing with optional parameter overrides
        /// Uses the User.Reconstitute method to create a user without domain events
        /// This provides a clean state for unit tests that need to test specific domain events
        /// Supports named parameters for validation testing
        /// </summary>
        public static User CreateTestUser(
            Guid? id = null,
            string email = "test@example.com",
            string firstName = "John",
            string lastName = "Doe",
            DateTime? birthdate = null,
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060,
            string aboutMe = "Test about me",
            string passwordSalt = "test_salt",
            string passwordHash = "test_hash",
            DateTime? lastLogin = null,
            DateTime? createdAt = null,
            DateTime? updatedAt = null,
            DateTime? deletedAt = null,
            bool isDeleted = false,
            string? profileImageName = null)
        {
            return User.Reconstitute(
                id: id ?? Guid.NewGuid(),
                email: email,
                firstName: firstName,
                lastName: lastName,
                birthdate: birthdate ?? new DateTime(1990, 1, 1),
                phoneNumber: phoneNumber,
                latitude: latitude,
                longitude: longitude,
                aboutMe: aboutMe,
                passwordSalt: passwordSalt,
                passwordHash: passwordHash,
                lastLogin: lastLogin ?? DateTime.UtcNow.AddDays(-1),
                createdAt: createdAt ?? DateTime.UtcNow,
                updatedAt: updatedAt ?? DateTime.UtcNow,
                deletedAt: deletedAt,
                isDeleted: isDeleted,
                profileImageName: profileImageName
            );
        }

        #endregion

        #region Command Creation Methods

        /// <summary>
        /// Creates a test register user command with valid data and optional parameter overrides
        /// Supports named parameters for validation testing
        /// </summary>
        public static RegisterUserCommand CreateRegisterUserCommand(
            string? email = null,
            string password = "TestPassword123!",
            string? passwordConfirmation = null,
            string name = "John",
            string surname = "Doe",
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060,
            DateOnly? birthdate = null,
            string aboutMe = "Test about me")
        {
            var actualEmail = email ?? $"test.user.{Guid.NewGuid():N}@example.com";
            return new RegisterUserCommand
            {
                Email = actualEmail,
                Password = password,
                PasswordConfirmation = passwordConfirmation ?? password,
                FirstName = name,
                LastName = surname,
                Birthdate = birthdate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now.AddYears(-25),
                PhoneNumber = phoneNumber,
                Latitude = latitude,
                Longitude = longitude,
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
            return CreateRegisterUserCommand(email: email, name: firstName, surname: lastName, 
                phoneNumber: phoneNumber, latitude: latitude, longitude: longitude, 
                birthdate: birthdate, password: password, aboutMe: aboutMe);
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
            return CreateRegisterUserCommand(email: email, name: firstName, surname: lastName, 
                phoneNumber: phoneNumber, latitude: latitude, longitude: longitude, 
                birthdate: DateOnly.FromDateTime(birthdate), password: password);
        }

        /// <summary>
        /// Creates a test update profile command with valid data and optional parameter overrides
        /// Supports named parameters for validation testing
        /// </summary>
        public static UpdateProfileUserCommand CreateUpdateProfileUserCommand(
            Guid? userId = null,
            string firstName = "Jane",
            string lastName = "Smith",
            string phoneNumber = "+1234567890",
            DateTime? birthdate = null,
            string aboutMe = "Updated profile information")
        {
            return new UpdateProfileUserCommand
            {
                Id = userId ?? Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Birthdate = birthdate ?? new DateTime(1985, 5, 15),
                AboutMe = aboutMe
            };
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
        /// Creates a valid GetUserQuery for testing
        /// </summary>
        public static GetUserQuery CreateGetUserQuery(Guid userId)
        {
            return new GetUserQuery { Id = userId };
        }

        /// <summary>
        /// Creates a test soft delete user command
        /// </summary>
        public static SoftDeleteUserCommand CreateSoftDeleteUserCommand(Guid? userId = null)
        {
            return new SoftDeleteUserCommand
            {
                Id = userId ?? Guid.NewGuid()
            };
        }

        /// <summary>
        /// Creates a test hard delete user command
        /// </summary>
        public static HardDeleteUserCommand CreateHardDeleteUserCommand(Guid? userId = null)
        {
            return new HardDeleteUserCommand
            {
                Id = userId ?? Guid.NewGuid()
            };
        }

        #endregion
    }
}
