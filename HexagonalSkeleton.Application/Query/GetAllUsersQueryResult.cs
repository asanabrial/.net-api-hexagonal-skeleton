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
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
