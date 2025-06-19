using HexagonalSkeleton.API.Models.Common;

namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Response model for successful user deletion
    /// </summary>
    public class DeleteUserResponse : BaseApiResponse
    {
        /// <summary>
        /// ID of the deleted user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Type of deletion performed
        /// </summary>
        public string DeletionType { get; set; } = string.Empty; // "Hard" or "Soft"

        /// <summary>
        /// When the deletion was performed
        /// </summary>
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    }
}
