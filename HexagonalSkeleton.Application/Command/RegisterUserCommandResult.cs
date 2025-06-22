namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Result for user registration command
    /// Contains only success data - errors are handled via exceptions
    /// </summary>
    public class RegisterUserCommandResult
    {        
        public RegisterUserCommandResult() 
        { 
            AccessToken = string.Empty;
            User = new UserInfoResult();
        }

        public RegisterUserCommandResult(string accessToken)
        {
            AccessToken = accessToken;
            User = new UserInfoResult();
        }

        /// <summary>
        /// JWT access token for the registered user
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
}
