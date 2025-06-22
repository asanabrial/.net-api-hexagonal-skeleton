using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{    /// <summary>
    /// Result for GetAllUsersQuery operation.
    /// Contains list of users. Errors are handled via exceptions.
    /// </summary>
    public class GetAllUsersQueryResult
    {        /// <summary>
        /// List of users
        /// </summary>
        public List<UserDto> Users { get; set; } = new List<UserDto>();
    }    public class UserDto
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
        public string? ProfileImageName { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
