using AutoMapper;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;
using HexagonalSkeleton.Application.Features.UserRegistration.Dto;
using HexagonalSkeleton.API.Models.Users;

namespace HexagonalSkeleton.API.Models.Auth
{    /// <summary>
     /// Response model for successful user authentication with registration
     /// </summary>
    [AutoMap(typeof(RegisterUserDto), ReverseMap = true)]
    public class UserAuthenticationResponse
    {
        /// <summary>
        /// JWT access token for API authentication
        /// </summary>
        public required string AccessToken { get; set; }

        /// <summary>
        /// Token type (usually "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// User information
        /// </summary>
        public AuthenticatedUserInfoResponse User { get; set; } = new();
    }
}
