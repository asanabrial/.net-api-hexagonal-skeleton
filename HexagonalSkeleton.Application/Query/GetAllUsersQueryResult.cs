using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{
    public class GetAllUsersQueryResult(User userEntity)
    {
        public int Id { get; set; } = userEntity.Id;
        public string? Name { get; set; } = userEntity.Name;
        public string? Surname { get; set; } = userEntity.Surname;
        public DateTime? Birthdate { get; set; } = userEntity.Birthdate;
        public string? Email { get; set; } = userEntity.Email;
        public DateTime LastLogin { get; set; } = userEntity.LastLogin;
    }
}
