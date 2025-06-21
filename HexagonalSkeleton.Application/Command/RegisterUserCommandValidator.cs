using FluentValidation;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Validator for RegisterUserCommand following DDD principles
    /// Validates business rules and constraints
    /// </summary>
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(150)
                .WithMessage("Email must not exceed 150 characters");

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");

            RuleFor(r => r.PasswordConfirmation)
                .NotEmpty()
                .WithMessage("Password confirmation is required")
                .Equal(r => r.Password)
                .WithMessage("Password confirmation must match password");

            RuleFor(r => r.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(50)
                .WithMessage("First name must not exceed 50 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("First name can only contain letters and spaces");

            RuleFor(r => r.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(50)
                .WithMessage("Last name must not exceed 50 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Last name can only contain letters and spaces");

            RuleFor(r => r.Birthdate)
                .NotEmpty()
                .WithMessage("Birth date is required")
                .Must(BeAtLeast13YearsOld)
                .WithMessage("User must be at least 13 years old")
                .Must(BeReasonableAge)
                .WithMessage("Birth date must be within reasonable range");

            RuleFor(r => r.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^[\+]?[0-9\-\s]+$")
                .WithMessage("Phone number format is invalid");

            RuleFor(r => r.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90 degrees");

            RuleFor(r => r.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180 degrees");

            RuleFor(r => r.AboutMe)
                .MaximumLength(500)
                .WithMessage("About me must not exceed 500 characters");
        }

        private static bool BeAtLeast13YearsOld(DateTime birthdate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age)) age--;
            return age >= 13;
        }

        private static bool BeReasonableAge(DateTime birthdate)
        {
            var today = DateTime.Today;
            return birthdate >= today.AddYears(-120) && birthdate <= today;
        }
    }
}
