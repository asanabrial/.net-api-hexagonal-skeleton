using FluentValidation;

namespace HexagonalSkeleton.Application.Query
{
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {        public GetUserQueryValidator()
        {
            RuleFor(r => r.Id)
                .GreaterThan(0);
        }
    }
}
