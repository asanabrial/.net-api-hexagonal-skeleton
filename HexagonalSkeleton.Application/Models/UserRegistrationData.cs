namespace HexagonalSkeleton.Application.Models
{
    /// <summary>
    /// Data Transfer Object for user registration
    /// Contains all required data for user registration process
    /// Follows Screaming Architecture by clearly indicating registration context
    /// </summary>
    public class UserRegistrationData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsAdult { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
