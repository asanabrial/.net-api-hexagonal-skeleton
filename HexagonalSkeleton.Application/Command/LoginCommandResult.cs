namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for LoginCommand operation.
    /// Returns access token on successful authentication.
    /// Exceptions are now used for error cases.
    /// </summary>
    public class LoginCommandResult
    {
        public LoginCommandResult(string? accessToken)
        {
            AccessToken = accessToken;
        }        /// <summary>
        /// The access token property, nullable string.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
