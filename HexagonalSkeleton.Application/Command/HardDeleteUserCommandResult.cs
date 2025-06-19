namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for HardDeleteUserCommand operation.
    /// Success is indicated by not throwing an exception.
    /// </summary>
    public class HardDeleteUserCommandResult
    {
        public HardDeleteUserCommandResult()
        {
            Message = "User permanently deleted successfully";
            DeletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Success message for the deletion operation.
        /// </summary>
        public string Message { get; set; }        /// <summary>
        /// Timestamp when the deletion occurred.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
