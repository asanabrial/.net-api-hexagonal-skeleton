using FluentValidation;
using HexagonalSkeleton.Application.Dto;
using HexagonalSkeleton.Domain;
using MediatR;

namespace HexagonalSkeleton.Application.Command
{
    public class UpdateUserCommandHandler(
        IValidator<UpdateUserCommand> validator,
        IUserRepository unitOfWork)
        : IRequestHandler<UpdateUserCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            if (!result.IsValid)
                return new ResultDto(result.ToDictionary());

            var user = request.ToDomainEntity();
            await unitOfWork.UpdateUser(user);
            return new ResultDto(await unitOfWork.SaveChangesAsync(cancellationToken));
        }
    }
}
