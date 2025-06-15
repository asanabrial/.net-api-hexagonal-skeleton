using FluentValidation;

namespace HexagonalSkeleton.Application.Query
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty();
            RuleFor(r => r.Password)
                .NotEmpty();
        }
    }
}
