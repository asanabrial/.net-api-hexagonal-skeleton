using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Query
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {
        public GetUserQueryValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
        }
    }
}
