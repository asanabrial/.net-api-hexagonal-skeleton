namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Response DTO for ValidateUserCredentialsQuery operation.
    /// Returns validation result for user credentials.
    /// Errors are handled via exceptions.
    /// </summary>
    public class ValidateUserCredentialsQueryResult
    {
        public ValidateUserCredentialsQueryResult(bool areCredentialsValid)
        {
            AreCredentialsValid = areCredentialsValid;
        }

        /// <summary>
        /// Indicates whether the credentials are valid.
        /// </summary>
        public bool AreCredentialsValid { get; set; }
    }
}
