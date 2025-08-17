using FluentValidation;

namespace HexagonalSkeleton.Application.Features.UserProfile.Queries
{
    public class GetMyProfileQueryValidator : AbstractValidator<GetMyProfileQuery>
    {
        public GetMyProfileQueryValidator()
        {
            RuleFor(r => r.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("User ID is required");
        }
    }
}
