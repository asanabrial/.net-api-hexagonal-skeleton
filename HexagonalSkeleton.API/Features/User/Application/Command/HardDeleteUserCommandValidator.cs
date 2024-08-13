using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
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
