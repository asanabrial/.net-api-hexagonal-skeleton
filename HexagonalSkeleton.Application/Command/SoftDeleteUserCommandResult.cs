namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for SoftDeleteUserCommand operation.
    /// Indicates the success of the logical user deletion operation.
    /// Errors are handled via exceptions.
    /// </summary>
    public class SoftDeleteUserCommandResult
    {
        public SoftDeleteUserCommandResult()
        {
            Success = true;
            Message = "User soft deleted successfully";
        }

        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; }
    }
}
