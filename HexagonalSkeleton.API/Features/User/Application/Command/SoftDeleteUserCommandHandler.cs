using FluentValidation;
using HexagonalSkeleton.API.Data;
using MediatR;

namespace HexagonalSkeleton.API.Features.User.Application.Command
{
    public class SoftDeleteUserCommandHandler(
        IValidator<SoftDeleteUserCommand> validator,
        IUnitOfWork unitOfWork)
        : IRequestHandler<SoftDeleteUserCommand, IResult>
    {
        public async Task<IResult> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            await unitOfWork.Users.SoftDeleteUser(request.Id);


            return Results.Ok(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
