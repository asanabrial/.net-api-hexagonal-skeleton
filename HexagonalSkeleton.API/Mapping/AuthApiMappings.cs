using HexagonalSkeleton.API.Models.Auth;
using HexagonalSkeleton.API.Models.Users;
using HexagonalSkeleton.Application.Features.UserAuthentication.Commands;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Commands;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// Hand-written mappers for authentication and registration flows between API
    /// models and Application layer commands and DTOs. Replaces the previous
    /// AutoMapper-based ApiMappingProfile entries and the [AutoMap] attributes on
    /// the authentication models.
    /// </summary>
    public static class AuthApiMappings
    {
        // ---- Requests -> Commands ----

        /// <summary>
        /// Maps a login request to the login command. RememberMe is intentionally
        /// not mapped because the command does not expose it (matching the original
        /// AutoMapper configuration).
        /// </summary>
        public static LoginCommand ToCommand(this LoginRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };
        }

        /// <summary>Maps a create-user request to the registration command.</summary>
        public static RegisterUserCommand ToCommand(this CreateUserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new RegisterUserCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                Birthdate = request.Birthdate,
                PasswordConfirmation = request.PasswordConfirmation,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AboutMe = request.AboutMe
            };
        }

        // ---- DTOs -> Responses ----

        /// <summary>
        /// Maps an authentication DTO (login result) to the login response,
        /// including the nested authenticated-user information.
        /// </summary>
        public static LoginResponse ToResponse(this AuthenticationDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new LoginResponse
            {
                AccessToken = dto.AccessToken,
                TokenType = dto.TokenType,
                ExpiresIn = dto.ExpiresIn,
                User = dto.User.ToUserInfoResponse()
            };
        }

        /// <summary>
        /// Maps a registration DTO to the authenticated-registration response,
        /// including the nested registered-user information and the auth token.
        /// </summary>
        public static AuthenticatedRegistrationResponse ToResponse(this RegisterUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new AuthenticatedRegistrationResponse
            {
                AccessToken = dto.AccessToken,
                TokenType = dto.TokenType,
                ExpiresIn = dto.ExpiresIn,
                User = dto.User.ToRegisterUserInfoResponse()
            };
        }

        /// <summary>
        /// Maps a registration DTO to the user-authentication response,
        /// including the nested authenticated-user information.
        /// </summary>
        public static UserAuthenticationResponse ToAuthenticationResponse(this RegisterUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserAuthenticationResponse
            {
                AccessToken = dto.AccessToken,
                TokenType = dto.TokenType,
                ExpiresIn = dto.ExpiresIn,
                User = dto.User.ToAuthenticatedUserInfoResponse()
            };
        }

        // ---- Nested user info mappers ----

        /// <summary>Maps an authenticated-user DTO to the auth user-info response.</summary>
        public static UserInfoResponse ToUserInfoResponse(this AuthenticatedUserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new UserInfoResponse
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
                CreatedAt = dto.CreatedAt
            };
        }

        /// <summary>Maps a registration user-info DTO to the registration user-info response.</summary>
        public static RegisterUserInfoResponse ToRegisterUserInfoResponse(this RegisterUserInfoDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new RegisterUserInfoResponse
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
                CreatedAt = dto.CreatedAt
            };
        }

        /// <summary>Maps a registration user-info DTO to the authenticated user-info response.</summary>
        public static AuthenticatedUserInfoResponse ToAuthenticatedUserInfoResponse(this RegisterUserInfoDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new AuthenticatedUserInfoResponse
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
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
