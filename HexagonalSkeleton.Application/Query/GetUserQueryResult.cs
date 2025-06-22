using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Query
{    public class GetUserQueryResult
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
