using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.Application.Command
{
    public class RegisterUserCommandResult : BaseResponseDto
    {
        public RegisterUserCommandResult(string? accessToken) : base()
        {
            AccessToken = accessToken;
        }

        public RegisterUserCommandResult(IDictionary<string, string[]> errors) : base(errors)
        {
            AccessToken = null;
        }

        public RegisterUserCommandResult(string errorMessage, bool isError) : base(errorMessage)
        {
            AccessToken = null;
        }

        public string? AccessToken { get; set; }
    }
}
