using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class UpdateProfileUserCommandHandler(
        IValidator<UpdateProfileUserCommand> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateProfileUserCommand, IResult>
    {
        public async Task<IResult> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            var user = await unitOfWork.Users.GetUserByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound();

            var entity = request.ToDomainEntity();
            await unitOfWork.Users.UpdateProfileUser(entity);
            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
