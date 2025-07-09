namespace HexagonalSkeleton.Domain.Events
{
    /// <summary>
    /// Integration event for user creation - used for read model synchronization
    /// </summary>
    public class UserCreatedIntegrationEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? Age { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Integration event for user profile updates - used for read model synchronization
    /// </summary>
    public class UserProfileUpdatedIntegrationEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? Age { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}