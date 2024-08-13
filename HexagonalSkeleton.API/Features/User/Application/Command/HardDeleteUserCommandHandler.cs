using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class HardDeleteUserCommandHandler(
        IValidator<HardDeleteUserCommand> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<HardDeleteUserCommand, IResult>
    {
        public async Task<IResult> Handle(HardDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            await unitOfWork.Users.HardDeleteUser(request.Id);

            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
