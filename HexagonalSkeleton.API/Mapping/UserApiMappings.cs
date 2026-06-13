using HexagonalSkeleton.API.Models;
using HexagonalSkeleton.API.Models.Common;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Common.Pagination;
using HexagonalSkeleton.Application.Features.UserManagement.Commands;
using HexagonalSkeleton.Application.Features.UserManagement.Dto;
using HexagonalSkeleton.Application.Features.UserManagement.Queries;
using HexagonalSkeleton.Application.Features.UserProfile.Commands;
using HexagonalSkeleton.Application.Features.UserProfile.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// Hand-written mappers between API user models (requests/responses) and the
    /// Application layer commands, queries, and DTOs. Replaces the previous
    /// AutoMapper-based ApiMappingProfile and the [AutoMap] attributes on the models.
    /// Requests map to commands/queries via ToCommand()/ToQuery(); DTOs map to
    /// responses via ToResponse() overloaded by source type.
    /// </summary>
    public static class UserApiMappings
    {
        // ---- Requests -> Commands / Queries ----

        /// <summary>
        /// Maps a user-list request to the management query. The query's paging
        /// members are set through its constructor; the remaining filters are set
        /// via object initializer (mirroring the previous flat AutoMapper mapping).
        /// </summary>
        public static GetAllUsersManagementQuery ToQuery(this GetAllUsersRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new GetAllUsersManagementQuery(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.SortBy,
                request.SortDirection)
            {
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                OnlyAdults = request.OnlyAdults,
                OnlyActive = request.OnlyActive,
                OnlyCompleteProfiles = request.OnlyCompleteProfiles,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusInKm = request.RadiusInKm
            };
        }

        /// <summary>
        /// Maps a full user-update request to the management command.
        /// Birthdate is required on the command, so a null request value falls
        /// back to default(DateTime), preserving the previous mapping behaviour.
        /// </summary>
        public static UpdateUserManagementCommand ToCommand(this UpdateUserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new UpdateUserManagementCommand
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Birthdate = request.Birthdate ?? default,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AboutMe = request.AboutMe
            };
        }

        /// <summary>
        /// Maps a profile-update request to the profile command.
        /// The Id is assigned separately by the controller (route value or current
        /// user), matching the original behaviour where AutoMapper left Id unset.
        /// </summary>
        public static UpdateProfileUserCommand ToCommand(this UpdateProfileRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new UpdateProfileUserCommand
            {
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                Birthdate = request.Birthdate ?? default,
                AboutMe = request.AboutMe ?? string.Empty
            };
        }

        // ---- DTOs -> Responses ----

        /// <summary>Maps a single-user query DTO to the user response.</summary>
        public static UserResponse ToResponse(this GetUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserResponse
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.Birthdate,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AboutMe = dto.AboutMe,
                LastLogin = dto.LastLogin,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Maps a user-list DTO to the user response.</summary>
        public static UserResponse ToResponse(this GetAllUsersDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserResponse
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.Birthdate,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AboutMe = dto.AboutMe,
                LastLogin = dto.LastLogin,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Maps a profile DTO to the user response.</summary>
        public static UserResponse ToResponse(this UserProfileDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserResponse
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.Birthdate,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AboutMe = dto.AboutMe,
                LastLogin = dto.LastLogin,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Maps an update-result DTO to the user response.</summary>
        public static UserResponse ToResponse(this UpdateUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserResponse
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Birthdate = dto.Birthdate,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AboutMe = dto.AboutMe,
                LastLogin = dto.LastLogin,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        /// <summary>Maps a deletion DTO to the deletion response.</summary>
        public static DeleteUserResponse ToResponse(this UserDeletionDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new DeleteUserResponse
            {
                UserId = dto.UserId
            };
        }

        /// <summary>
        /// Maps a registration-user DTO to the registration-only response
        /// (no authentication token). Preserves the fixed success message used by
        /// the previous AutoMapper configuration.
        /// </summary>
        public static UserRegistrationResponse ToRegistrationResponse(this RegisterUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserRegistrationResponse
            {
                UserId = dto.User.Id,
                Email = dto.User.Email,
                FirstName = dto.User.FirstName,
                LastName = dto.User.LastName,
                Message = "User registered successfully"
            };
        }

        // ---- Generic paged mapping ----

        /// <summary>
        /// Maps a paged query result to a paged response, projecting each item with
        /// the supplied element mapper. Generic and reusable for any item/response
        /// pair, replacing the open-generic AutoMapper PagedQueryResult -> PagedResponse map.
        /// </summary>
        public static PagedResponse<TResponse> ToResponse<TItem, TResponse>(
            this PagedQueryResult<TItem> src,
            Func<TItem, TResponse> itemMapper)
        {
            ArgumentNullException.ThrowIfNull(src);
            ArgumentNullException.ThrowIfNull(itemMapper);

            return new PagedResponse<TResponse>
            {
                Data = src.Items.Select(itemMapper).ToList(),
                TotalCount = src.Metadata.TotalCount,
                PageNumber = src.Metadata.PageNumber,
                PageSize = src.Metadata.PageSize
            };
        }

        /// <summary>
        /// Maps a paged query result to a paged response of the same item type
        /// (no element transformation).
        /// </summary>
        public static PagedResponse<T> ToResponse<T>(this PagedQueryResult<T> src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new PagedResponse<T>
            {
                Data = src.Items,
                TotalCount = src.Metadata.TotalCount,
                PageNumber = src.Metadata.PageNumber,
                PageSize = src.Metadata.PageSize
            };
        }
    }
}
