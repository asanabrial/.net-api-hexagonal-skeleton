using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.Application.Features.UserAuthentication.Dto
{
    /// <summary>
    /// DTO for authentication response containing token and user information
    /// Used specifically for login/register operations
    /// </summary>
    public class AuthenticationDto
    {        /// <summary>
        /// JWT access token for the authenticated user
        /// </summary>
        public required string AccessToken { get; set; } = string.Empty;

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
        public UserDto User { get; set; } = new();
    }
}
