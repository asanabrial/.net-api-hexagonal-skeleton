namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class is a Data Transfer Object (DTO) used to encapsulate data for the response to a login request.
    /// </summary>
    public class RegisterUserCommandResult(string? accessToken)
    {
        /// <summary>
        /// The access token property, nullable string.
        /// </summary>
        public string? AccessToken { get; set; } = accessToken;
    }
}
