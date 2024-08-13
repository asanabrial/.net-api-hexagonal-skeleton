using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty();

            RuleFor(r => r.Password)
                .NotEmpty()
                .Equal(r => r.PasswordConfirmation)
                .When(r => !string.IsNullOrWhiteSpace(r.Password));

            RuleFor(r => r.PasswordConfirmation)
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
