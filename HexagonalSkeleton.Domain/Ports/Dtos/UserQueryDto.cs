namespace HexagonalSkeleton.Domain.Ports.Dtos
{
    /// <summary>
    /// DTO for read operations
    /// Optimized for query responses and UI display
    /// </summary>
    public class UserQueryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? Age { get; set; }
        public double ProfileCompleteness { get; set; }
        public List<string> SearchTerms { get; set; } = new();
    }
}
