using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{
    public class GetAllUsersQueryResult : BaseResponseDto
    {
        public GetAllUsersQueryResult(IEnumerable<User> users) : base()
        {
            Users = users.Select(u => new UserDto(u)).ToList();
        }

        public GetAllUsersQueryResult(IDictionary<string, string[]> errors) : base(errors)
        {
            Users = new List<UserDto>();
        }

        public GetAllUsersQueryResult(string errorMessage, bool isError) : base(errorMessage)
        {
            Users = new List<UserDto>();
        }

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
