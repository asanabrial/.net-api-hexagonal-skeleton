namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for LoginCommand operation.
    /// Returns access token and user data on successful authentication.
    /// Exceptions are now used for error cases.
    /// </summary>
    public class LoginCommandResult
    {
        public LoginCommandResult() 
        { 
            AccessToken = string.Empty;
            User = new UserInfoResult();
        }

        public LoginCommandResult(string accessToken)
        {
            AccessToken = accessToken;
            User = new UserInfoResult();
        }

        /// <summary>
        /// JWT access token for the authenticated user
        /// </summary>
        public string AccessToken { get; set; }

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
        public UserInfoResult User { get; set; }
    }

    /// <summary>
    /// User information included in command results
    /// </summary>
    public class UserInfoResult
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
