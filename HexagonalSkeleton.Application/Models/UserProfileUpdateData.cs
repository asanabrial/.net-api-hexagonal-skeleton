namespace HexagonalSkeleton.Application.Models
{
    /// <summary>
    /// Data Transfer Object for user profile updates
    /// Contains all modifiable user profile data
    /// Follows Screaming Architecture by clearly indicating profile context
    /// </summary>
    public class UserProfileUpdateData
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AboutMe { get; set; }
    }
}
