using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Mapping
{
    /// <summary>
    /// Hand-written mappers for the User aggregate to the various Application-layer DTOs.
    /// Replaces the previous AutoMapper-based ApplicationMappingProfile. Value objects are
    /// flattened by accessing their properties directly, and Location is read defensively
    /// to preserve the original null-tolerant mapping behaviour.
    /// </summary>
    public static class UserDtoMapper
    {
        /// <summary>
        /// Maps a User aggregate to the single-user query DTO (includes deletion metadata).
        /// </summary>
        public static GetUserDto ToGetUserDto(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var name = NameParts(user);
            var (latitude, longitude) = Coordinates(user);

            return new GetUserDto
            {
                Id = user.Id,
                FirstName = name.First,
                LastName = name.Last,
                FullName = name.Full,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                DeletedAt = user.DeletedAt,
                IsDeleted = user.IsDeleted
            };
        }

        /// <summary>
        /// Maps a User aggregate to the user-list DTO.
        /// </summary>
        public static GetAllUsersDto ToGetAllUsersDto(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var name = NameParts(user);
            var (latitude, longitude) = Coordinates(user);

            return new GetAllUsersDto
            {
                Id = user.Id,
                FirstName = name.First,
                LastName = name.Last,
                FullName = name.Full,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a User aggregate to the authentication-response user DTO.
        /// </summary>
        public static AuthenticatedUserDto ToAuthenticatedUserDto(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var name = NameParts(user);
            var (latitude, longitude) = Coordinates(user);

            return new AuthenticatedUserDto
            {
                Id = user.Id,
                FirstName = name.First,
                LastName = name.Last,
                FullName = name.Full,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a User aggregate to the profile DTO.
        /// </summary>
        public static UserProfileDto ToUserProfileDto(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var name = NameParts(user);
            var (latitude, longitude) = Coordinates(user);

            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = name.First,
                LastName = name.Last,
                FullName = name.Full,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        /// <summary>
        /// Maps a User aggregate to the update-response DTO.
        /// </summary>
        public static UpdateUserDto ToUpdateUserDto(this User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var name = NameParts(user);
            var (latitude, longitude) = Coordinates(user);

            return new UpdateUserDto
            {
                Id = user.Id,
                FirstName = name.First,
                LastName = name.Last,
                FullName = name.Full,
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = latitude,
                Longitude = longitude,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // ---- Shared flattening helpers ----

        /// <summary>
        /// Flattens the FullName value object into its first, last, and composed parts.
        /// </summary>
        private static (string First, string Last, string Full) NameParts(User user) =>
            (user.FullName.FirstName, user.FullName.LastName, user.FullName.GetFullName());

        /// <summary>
        /// Reads the optional Location value object into nullable coordinates,
        /// preserving the original null-tolerant mapping behaviour.
        /// </summary>
        private static (double? Latitude, double? Longitude) Coordinates(User user) =>
            user.Location != null
                ? (user.Location.Latitude, user.Location.Longitude)
                : (null, null);
    }
}
