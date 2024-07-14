using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class handles a LoginCommand.
    /// </summary>
    public class HardDeleteUserCommandHandler(
        IValidator<HardDeleteUserCommand> validator,
        ILogger<HardDeleteUserCommandHandler> logger,
        IUnitOfWork unitOfWork)
        : IRequestHandler<HardDeleteUserCommand, IResult>
    {
        public async Task<IResult> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            await unitOfWork.Users.HardDelete(request.Id);

            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
