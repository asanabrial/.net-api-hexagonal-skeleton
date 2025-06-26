using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class HardDeleteUserCommandValidator : AbstractValidator<HardDeleteUserCommand>
    {        public HardDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .GreaterThan(0);
        }
    }
}
