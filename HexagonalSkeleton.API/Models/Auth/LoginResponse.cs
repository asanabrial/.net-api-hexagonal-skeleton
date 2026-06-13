using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;

namespace HexagonalSkeleton.API.Models.Auth
{
    /// <summary>
    /// Response model for successful authentication
    /// </summary>
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
}
