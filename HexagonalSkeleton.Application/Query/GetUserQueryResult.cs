using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQueryResult
    {
        public GetUserQueryResult(User userEntity)
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
