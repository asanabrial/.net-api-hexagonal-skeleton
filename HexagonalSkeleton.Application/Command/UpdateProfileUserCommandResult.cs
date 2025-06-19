using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for UpdateProfileUserCommand operation.
    /// Returns the updated user profile data on successful update.
    /// </summary>
    public class UpdateProfileUserCommandResult : BaseResponseDto
    {
        public UpdateProfileUserCommandResult(User userEntity) : base()
        {
            Id = userEntity.Id;
            FirstName = userEntity.FullName.FirstName;
            LastName = userEntity.FullName.LastName;
            Birthdate = userEntity.Birthdate;
            Email = userEntity.Email.Value;
            LastLogin = userEntity.LastLogin;
        }

        public UpdateProfileUserCommandResult(IDictionary<string, string[]> errors) : base(errors)
        {
        }

        public UpdateProfileUserCommandResult(string error) : base(error)
        {
        }

        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }        public DateTime LastLogin { get; set; }
    }
}
