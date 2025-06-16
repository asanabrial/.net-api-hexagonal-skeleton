using FluentValidation;

namespace HexagonalSkeleton.Application.Command
{
    public class SoftDeleteUserCommandValidator : AbstractValidator<SoftDeleteUserCommand>
    {        public SoftDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .GreaterThan(0);
        }
    }
}
