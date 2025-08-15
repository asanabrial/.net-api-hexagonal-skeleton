using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class SoftDeleteUserManagementCommandValidator : AbstractValidator<SoftDeleteUserManagementCommand>
    {        public SoftDeleteUserManagementCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEqual(Guid.Empty)
                .WithMessage("Id cannot be empty");
        }
    }
}
