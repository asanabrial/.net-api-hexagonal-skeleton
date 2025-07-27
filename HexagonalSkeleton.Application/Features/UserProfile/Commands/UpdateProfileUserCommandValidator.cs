using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserProfile.Commands
{
    public class UpdateProfileUserCommandValidator : AbstractValidator<UpdateProfileUserCommand>
    {
        public UpdateProfileUserCommandValidator()
        {
            RuleFor(r => r.AboutMe)
                .NotEmpty();

            RuleFor(r => r.FirstName)
                .NotEmpty();

            RuleFor(r => r.LastName)
                .NotEmpty();

            RuleFor(r => r.Birthdate)
                .NotEmpty();

            RuleFor(r => r.PhoneNumber)
                .NotEmpty()
                .When(r => !string.IsNullOrWhiteSpace(r.PhoneNumber));
        }
    }
}
