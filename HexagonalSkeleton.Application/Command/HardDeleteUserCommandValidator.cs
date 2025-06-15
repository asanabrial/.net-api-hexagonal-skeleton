using FluentValidation;

namespace HexagonalSkeleton.Application.Command
{
    public class HardDeleteUserCommandValidator : AbstractValidator<HardDeleteUserCommand>
    {
        public HardDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty(); ;
        }
    }
}
