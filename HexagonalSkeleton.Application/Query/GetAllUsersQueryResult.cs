using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{    public class GetAllUsersQueryResult(User userEntity)
    {
        public int Id { get; set; } = userEntity.Id;
        public string? FirstName { get; set; } = userEntity.FullName.FirstName;
        public string? LastName { get; set; } = userEntity.FullName.LastName;
        public DateTime? Birthdate { get; set; } = userEntity.Birthdate;
        public string? Email { get; set; } = userEntity.Email.Value;
        public DateTime LastLogin { get; set; } = userEntity.LastLogin;
    }
}
