using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    /// <summary>
    /// Validator for GetAllUsersManagementQuery to ensure valid pagination parameters
    /// </summary>
    public class GetAllUsersManagementQueryValidator : AbstractValidator<GetAllUsersManagementQuery>
    {
        public GetAllUsersManagementQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}
