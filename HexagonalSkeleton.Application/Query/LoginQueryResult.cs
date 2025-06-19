namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class is a Data Transfer Object (DTO) used to encapsulate data for the response to a login request.
    /// Now simplified - just contains the access token. Error cases throw exceptions.
    /// </summary>
    public class LoginQueryResult
    {
        public LoginQueryResult(string accessToken)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        /// <summary>
        /// The access token for the authenticated user.
        /// </summary>
        public string AccessToken { get; }
    }
}
