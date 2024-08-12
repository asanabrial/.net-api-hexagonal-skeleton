using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {
        public GetUserQueryValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
        }
    }
}
