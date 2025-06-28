using AutoMapper;
using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;

namespace HexagonalSkeleton.API.Models.Auth
{    /// <summary>
     /// Response model for successful authentication
     /// </summary>
    [AutoMap(typeof(AuthenticationDto), ReverseMap = true)]
    public class LoginResponse
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
        public UserInfoResponse User { get; set; } = new();
    }

    /// <summary>
    /// Basic user information included in login response
    /// </summary>
    [AutoMap(typeof(AuthenticatedUserDto), ReverseMap = true)]
    public class UserInfoResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AboutMe { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
