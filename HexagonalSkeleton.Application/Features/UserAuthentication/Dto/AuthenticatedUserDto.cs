namespace HexagonalSkeleton.Application.Features.UserAuthentication.Dto
{
    /// <summary>
    /// DTO for user information in authentication responses
    /// Contains essential user data needed after login/register
    /// </summary>
    public class AuthenticatedUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AboutMe { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
