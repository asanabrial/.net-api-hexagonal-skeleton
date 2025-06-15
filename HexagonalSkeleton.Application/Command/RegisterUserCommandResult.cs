namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommandResult(string? accessToken)
    {
        public string? AccessToken { get; set; } = accessToken;
    }
}
