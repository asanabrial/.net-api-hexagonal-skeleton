using FluentValidation;
using HexagonalSkeleton.API.Config;
using HexagonalSkeleton.API.Data;
using MediatR;
using Microsoft.Extensions.Options;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    /// <summary>
    /// This class handles a LoginCommand.
    /// </summary>
    public class UpdateUserCommandHandler(
        IValidator<UpdateUserCommand> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateUserCommand, IResult>
    {
        public async Task<IResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var user = request.ToDomainEntity();
            await unitOfWork.Users.Update(user);
            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
