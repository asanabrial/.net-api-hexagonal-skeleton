using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserManagement.Queries
{
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {        public GetUserQueryValidator()
        {
            RuleFor(r => r.Id)
                .NotEqual(Guid.Empty);
        }
    }
}
