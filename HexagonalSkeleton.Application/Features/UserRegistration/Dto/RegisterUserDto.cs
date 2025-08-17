using HexagonalSkeleton.Application.Features.UserAuthentication.Dto;

namespace HexagonalSkeleton.Application.Features.UserRegistration.Dto
{
    /// <summary>
    /// DTO for user registration response containing token and user information
    /// Used specifically for register operations
    /// </summary>
    public class RegisterUserDto
    {        /// <summary>
        /// JWT access token for the registered user
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (usually "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Registered user information
        /// </summary>
        public RegisterUserInfoDto User { get; set; } = new();
    }
}
