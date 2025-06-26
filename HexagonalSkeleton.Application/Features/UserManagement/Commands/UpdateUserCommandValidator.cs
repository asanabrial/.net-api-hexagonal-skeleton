using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {        public UpdateUserCommandValidator()
        {
            RuleFor(r => r.FirstName)
                .NotEmpty();

            RuleFor(r => r.LastName)
                .NotEmpty();

            RuleFor(r => r.Birthdate)
                .NotEmpty();

            RuleFor(r => r.PhoneNumber)
                .NotEmpty();
        }
    }
}
