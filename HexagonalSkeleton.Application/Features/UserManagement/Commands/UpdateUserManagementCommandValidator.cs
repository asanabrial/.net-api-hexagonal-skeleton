using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    /// <summary>
    /// Validator for UpdateUserManagementCommand
    /// </summary>
    public class UpdateUserManagementCommandValidator : AbstractValidator<UpdateUserManagementCommand>
    {        
        public UpdateUserManagementCommandValidator()
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
