using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UploadProfileImageCommandValidator : AbstractValidator<UploadProfileImageCommand>
    {
        public UploadProfileImageCommandValidator()
        {
            RuleFor(r => r.ProfileImage)
                .NotEmpty();
        }
    }
}
