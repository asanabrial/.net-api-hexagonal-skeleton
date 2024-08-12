using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

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

            var user = await unitOfWork.Users.GetUserByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound();

            request.ToDomainEntity(user);
            await unitOfWork.Users.UpdateUser(user);
            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
