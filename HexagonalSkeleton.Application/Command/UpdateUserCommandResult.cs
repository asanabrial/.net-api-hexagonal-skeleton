using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for UpdateUserCommand operation.
    /// Returns the updated user data on successful update.
    /// Errors are handled via exceptions.
    /// </summary>
    public class UpdateUserCommandResult
    {
        public UpdateUserCommandResult()
        {
        }

        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Email { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
