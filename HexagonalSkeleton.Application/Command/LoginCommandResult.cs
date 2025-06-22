namespace HexagonalSkeleton.Application.Command
{    /// <summary>
    /// Response DTO for LoginCommand operation.
    /// Returns access token and user data on successful authentication.
    /// Exceptions are now used for error cases.
    /// </summary>
    public class LoginCommandResult
    {
        public LoginCommandResult() 
        { 
            AccessToken = string.Empty;
        }

        public LoginCommandResult(string accessToken)
        {
            AccessToken = accessToken;
        }

        /// <summary>
        /// JWT access token for the authenticated user
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User's birth date
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// User's location latitude
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// User's location longitude
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// User's about me description
        /// </summary>
        public string? AboutMe { get; set; }

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
