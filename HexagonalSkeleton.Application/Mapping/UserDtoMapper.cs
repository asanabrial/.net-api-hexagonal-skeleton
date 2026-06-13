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

            return new GetUserDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location != null ? user.Location.Latitude : (double?)null,
                Longitude = user.Location != null ? user.Location.Longitude : (double?)null,
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

            return new GetAllUsersDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location != null ? user.Location.Latitude : (double?)null,
                Longitude = user.Location != null ? user.Location.Longitude : (double?)null,
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

            return new AuthenticatedUserDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location != null ? user.Location.Latitude : (double?)null,
                Longitude = user.Location != null ? user.Location.Longitude : (double?)null,
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

            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location != null ? user.Location.Latitude : (double?)null,
                Longitude = user.Location != null ? user.Location.Longitude : (double?)null,
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

            return new UpdateUserDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                FullName = user.FullName.GetFullName(),
                Email = user.Email.Value,
                PhoneNumber = user.PhoneNumber.Value,
                Birthdate = user.Birthdate,
                Latitude = user.Location != null ? (double?)user.Location.Latitude : null,
                Longitude = user.Location != null ? (double?)user.Location.Longitude : null,
                AboutMe = user.AboutMe,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
