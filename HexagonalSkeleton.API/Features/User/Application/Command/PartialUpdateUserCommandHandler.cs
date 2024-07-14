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
    public class PartialUpdateUserCommandHandler(
        IValidator<PartialUpdateUserCommand> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<PartialUpdateUserCommand, IResult>
    {
        public async Task<IResult> Handle(PartialUpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var user = await unitOfWork.Users.FindOneAsync(request.Id, cancellationToken);
            if (user is null) return Results.NotFound();

            request.ToDomainEntity(user);
            await unitOfWork.Users.Update(user);
            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
