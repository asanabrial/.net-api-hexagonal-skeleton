using HexagonalSkeleton.API.Features.User.Domain;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    /// <summary>
    /// This class is a Data Transfer Object (DTO) used to encapsulate data for the response.
    /// </summary>
    public class GetUserQueryResult(UserEntity userEntity)
    {
        public int Id { get; set; } = userEntity.Id;
        public string? Name { get; set; } = userEntity.Name;
        public string? Surname { get; set; } = userEntity.Surname;
        public DateTime? Birthdate { get; set; } = userEntity.Birthdate;
        public string? Email { get; set; } = userEntity.Email;
        public DateTime LastLogin { get; set; } = userEntity.LastLogin;
        public DateTime CreatedAt { get; set; } = userEntity.CreatedAt;
        public DateTime UpdatedAt { get; set; } = userEntity.UpdatedAt;
    }
}
