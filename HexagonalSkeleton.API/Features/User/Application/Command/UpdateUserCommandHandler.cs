using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
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
            await unitOfWork.Users.UpdateUser(user);
            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
