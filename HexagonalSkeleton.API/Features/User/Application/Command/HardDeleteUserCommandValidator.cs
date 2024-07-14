using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class HardDeleteUserCommandValidator : AbstractValidator<HardDeleteUserCommand>
    {
        public HardDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty(); ;
        }
    }
}
