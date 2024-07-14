using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class PartialUpdateUserCommandValidator : AbstractValidator<PartialUpdateUserCommand>
    {
        public PartialUpdateUserCommandValidator()
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
