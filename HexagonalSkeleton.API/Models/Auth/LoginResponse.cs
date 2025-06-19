using HexagonalSkeleton.API.Models.Common;

namespace HexagonalSkeleton.API.Models.Auth
{
    /// <summary>
    /// Response model for successful authentication
    /// </summary>
    public class LoginResponse : BaseApiResponse
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
        public UserInfo User { get; set; } = new();
    }

    /// <summary>
    /// Basic user information included in login response
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
