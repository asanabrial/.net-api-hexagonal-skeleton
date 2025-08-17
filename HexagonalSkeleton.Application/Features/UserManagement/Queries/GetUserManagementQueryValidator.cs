using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Validator for GetUserManagementQuery
    /// Ensures that the user ID is valid for management operations
    /// </summary>
    public class GetUserManagementQueryValidator : AbstractValidator<GetUserManagementQuery>
    {
        public GetUserManagementQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}
