using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// Response DTO for ValidateUserCredentialsQuery operation.
    /// Returns validation result for user credentials.
    /// </summary>
    public class ValidateUserCredentialsQueryResult : BaseResponseDto
    {
        public ValidateUserCredentialsQueryResult(bool areCredentialsValid) : base()
        {
            AreCredentialsValid = areCredentialsValid;
        }

        public ValidateUserCredentialsQueryResult(IDictionary<string, string[]> errors) : base(errors)
        {
            AreCredentialsValid = false;
        }

        public ValidateUserCredentialsQueryResult(string error, bool isError) : base(error)
        {
            AreCredentialsValid = false;
        }

        /// <summary>
        /// Indicates whether the credentials are valid.
        /// </summary>
        public bool AreCredentialsValid { get; set; }
    }
}
