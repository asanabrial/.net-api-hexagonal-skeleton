using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Commands
{
    public class HardDeleteUserManagementCommandValidator : AbstractValidator<HardDeleteUserManagementCommand>
    {        public HardDeleteUserManagementCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEqual(Guid.Empty)
                .WithMessage("Id cannot be empty");
        }
    }
}
