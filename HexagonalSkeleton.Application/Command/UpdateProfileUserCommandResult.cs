using HexagonalSkeleton.Domain;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for UpdateProfileUserCommand operation.
    /// Returns the updated user profile data on successful update.
    /// Errors are handled via exceptions.
    /// </summary>
    public class UpdateProfileUserCommandResult
    {
        public UpdateProfileUserCommandResult()
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
