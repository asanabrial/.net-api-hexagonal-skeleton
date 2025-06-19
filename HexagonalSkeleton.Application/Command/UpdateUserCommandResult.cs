using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for UpdateUserCommand operation.
    /// Returns the updated user data on successful update.
    /// </summary>
    public class UpdateUserCommandResult : BaseResponseDto
    {
        public UpdateUserCommandResult(User userEntity) : base()
        {
            Id = userEntity.Id;
            FirstName = userEntity.FullName.FirstName;
            LastName = userEntity.FullName.LastName;
            Birthdate = userEntity.Birthdate;
            Email = userEntity.Email.Value;
            LastLogin = userEntity.LastLogin;
        }

        public UpdateUserCommandResult(IDictionary<string, string[]> errors) : base(errors)
        {
        }

        public UpdateUserCommandResult(string error, bool isError) : base(error)
        {
        }

        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }        public DateTime LastLogin { get; set; }
    }
}
