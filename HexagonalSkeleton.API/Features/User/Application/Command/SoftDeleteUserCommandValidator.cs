using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class SoftDeleteUserCommandValidator : AbstractValidator<SoftDeleteUserCommand>
    {
        public SoftDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty(); ;
        }
    }
}
