using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UpdateProfileUserCommandValidator : AbstractValidator<UpdateProfileUserCommand>
    {
        public UpdateProfileUserCommandValidator()
        {
            RuleFor(r => r.AboutMe)
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
