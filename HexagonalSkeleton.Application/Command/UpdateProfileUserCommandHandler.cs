using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateProfileUserCommandHandler(
        IValidator<UpdateProfileUserCommand> validator,
        IUserRepository unitOfWork)
        : IRequestHandler<UpdateProfileUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var user = await unitOfWork.GetUserByIdAsync(id: request.Id, cancellationToken: cancellationToken);
            //if (user is null) return Results.NotFound();

            var entity = request.ToDomainEntity();
            await unitOfWork.UpdateProfileUser(entity);
            return new ResultDto(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
