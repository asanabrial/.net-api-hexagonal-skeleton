using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;

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
            string email = "test@example.com",
            string password = "TestPassword123!",
            string? passwordConfirmation = null,
            string name = "John",
            string surname = "Doe",
            string phoneNumber = "+1234567890",
            double latitude = 40.7128,
            double longitude = -74.0060,
            string aboutMe = "Test about me")
        {
            return new RegisterUserCommand
            {
                Email = email,
                Password = password,
                PasswordConfirmation = passwordConfirmation ?? password,
                FirstName = name,
                LastName = surname,
                Birthdate = new DateTime(1990, 1, 1),
                PhoneNumber = phoneNumber,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = aboutMe
            };
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
