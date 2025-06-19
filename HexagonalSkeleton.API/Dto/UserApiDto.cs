namespace HexagonalSkeleton.API.Dto
{
    /// <summary>
    /// User data transfer object for API responses
    /// </summary>
    public class UserApiDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthdate { get; set; }
        public DateTime LastLogin { get; set; }
        
        // Computed property for full name
        public string Name => $"{FirstName} {LastName}".Trim();
    }
}
