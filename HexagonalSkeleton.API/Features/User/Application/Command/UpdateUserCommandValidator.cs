using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty();

            RuleFor(r => r.Name)
                .NotEmpty();

            RuleFor(r => r.Surname)
                .NotEmpty();

            RuleFor(r => r.Birthdate)
                .NotEmpty();
        }
    }
}
