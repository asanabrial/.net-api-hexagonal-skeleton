namespace HexagonalSkeleton.Application.Features.UserManagement.Dto
{
    /// <summary>
    /// DTO for user deletion response
    /// Simple confirmation that a user was deleted
    /// </summary>
    public class UserDeletionDto
    {
        /// <summary>
        /// ID of the deleted user
        /// </summary>
        public Guid UserId { get; set; }
    }
}
