using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserProfile.Commands
{
    public class UpdateProfileUserCommandValidator : AbstractValidator<UpdateProfileUserCommand>
    {
        public UpdateProfileUserCommandValidator()
        {
            // AboutMe: not empty if provided
            RuleFor(r => r.AboutMe)
                .NotEmpty()
                .WithMessage("AboutMe should not be empty if provided");

            // FirstName: not empty if provided
            RuleFor(r => r.FirstName)
                .NotEmpty()
                .WithMessage("FirstName should not be empty if provided");

            // LastName: not empty if provided
            RuleFor(r => r.LastName)
                .NotEmpty()
                .WithMessage("LastName should not be empty if provided");

            // Birthdate: validate if provided
            RuleFor(r => r.Birthdate)
                .NotEmpty()
                .WithMessage("Birthdate should be valid if provided");

            // PhoneNumber: not empty if provided
            RuleFor(r => r.PhoneNumber)
                .NotEmpty()
                .WithMessage("PhoneNumber should not be empty if provided");
        }
    }
}
