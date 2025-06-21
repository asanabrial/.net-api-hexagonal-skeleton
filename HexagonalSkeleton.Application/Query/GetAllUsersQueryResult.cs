using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Result for GetAllUsersQuery operation.
    /// Contains list of users. Errors are handled via exceptions.
    /// </summary>
    public class GetAllUsersQueryResult
    {
        public GetAllUsersQueryResult(IEnumerable<User> users)
        {
            Users = users.Select(u => new UserDto(u)).ToList();
        }

        /// <summary>
        /// List of users
        /// </summary>
        public IList<UserDto> Users { get; set; }
    }

    public class UserDto
    {
        public UserDto(User userEntity)
        {
            Id = userEntity.Id;
            FirstName = userEntity.FullName.FirstName;
            LastName = userEntity.FullName.LastName;
            Birthdate = userEntity.Birthdate;
            Email = userEntity.Email.Value;
            LastLogin = userEntity.LastLogin;
        }

        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
