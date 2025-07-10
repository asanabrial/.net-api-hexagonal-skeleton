using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.API.Models
{
    /// <summary>
    /// Response model for user registration operation
    /// Follows Screaming Architecture by clearly indicating registration response context
    /// </summary>
    public class UserRegistrationResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Message { get; set; } = "User registered successfully";
    }
}
