using FluentValidation;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class validates a LoginCommand.
    /// </summary>
    public class UploadProfileImageCommandValidator : AbstractValidator<UploadProfileImageCommand>
    {
        public UploadProfileImageCommandValidator()
        {
            RuleFor(r => r.ProfileImage)
                .NotEmpty();
        }
    }
}
