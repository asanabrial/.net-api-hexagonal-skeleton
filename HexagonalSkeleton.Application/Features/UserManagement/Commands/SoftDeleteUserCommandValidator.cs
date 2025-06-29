using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class SoftDeleteUserCommandValidator : AbstractValidator<SoftDeleteUserCommand>
    {        public SoftDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEqual(Guid.Empty)
                .WithMessage("Id cannot be empty");
        }
    }
}
