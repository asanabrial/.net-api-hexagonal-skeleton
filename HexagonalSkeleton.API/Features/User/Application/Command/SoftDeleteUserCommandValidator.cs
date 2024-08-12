using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class SoftDeleteUserCommandValidator : AbstractValidator<SoftDeleteUserCommand>
    {
        public SoftDeleteUserCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
        }
    }
}
