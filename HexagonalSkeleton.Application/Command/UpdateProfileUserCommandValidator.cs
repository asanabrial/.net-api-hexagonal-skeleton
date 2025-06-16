using FluentValidation;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateProfileUserCommandValidator : AbstractValidator<UpdateProfileUserCommand>
    {
        public UpdateProfileUserCommandValidator()
        {
            RuleFor(r => r.AboutMe)
                .NotEmpty();            RuleFor(r => r.FirstName)
                .NotEmpty();

            RuleFor(r => r.LastName)
                .NotEmpty();

            RuleFor(r => r.Birthdate)
                .NotEmpty();
        }
    }
}
