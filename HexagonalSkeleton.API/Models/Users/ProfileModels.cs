namespace HexagonalSkeleton.API.Models.Users
{
    /// <summary>
    /// Request model for changing user password
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Current password for verification
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of new password
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
